using RLTK;
using RLTKTutorial.Game;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace RLTKTutorial.Part3.Map
{
    [DisableAutoCreation]
    public class GenerateMapSystem : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem _barrier;
        EntityQuery _generateMapQuery;
        EntityQuery _playerQuery;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int At(int x, int y, int width) => y * width + x;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _generateMapQuery = GetEntityQuery(
                ComponentType.ReadOnly<GenerateMapProxy>(), 
                ComponentType.ReadOnly<MapData>(),
                ComponentType.ReadWrite<TileBuffer>()
                );

            _playerQuery = GetEntityQuery(
                ComponentType.ReadOnly<Player>(),
                ComponentType.ReadWrite<Position>());

            RequireForUpdate(_generateMapQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            var commandmap = _barrier.CreateCommandBuffer();


            var mapEntity = _generateMapQuery.GetSingletonEntity();
            var map = EntityManager.GetBuffer<TileBuffer>(mapEntity);
            var genData = EntityManager.GetComponentData<GenerateMapProxy>(mapEntity);
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);
            int2 mapSize = new int2(mapData.width, mapData.height);

            uint seed = genData.seed == 0 ? (uint)UnityEngine.Random.Range(1, int.MaxValue) : (uint)genData.seed;
            
            inputDeps = Job.WithCode(() =>
            {
                Random rand = new Random(seed);

                int w = mapData.width;
                int h = mapData.height;
                int volume = w * h;

                map.ResizeUninitialized(volume);
                
                for (int i = 0; i < map.Length; ++i)
                    map[i] = TileType.Floor;

                for (int x = 0; x < w; ++x)
                {
                    map[At(x, 0, w)] = TileType.Wall;
                    map[At(x, h - 1, w)] = TileType.Wall;
                }

                for (int y = 0; y < h; ++y)
                {
                    map[At(0, y, w)] = TileType.Wall;
                    map[At(w - 1, y, w)] = TileType.Wall;
                }

                for (int i = 0; i < genData.iterationCount; ++i)
                {
                    int x = rand.NextInt(1, mapData.width - 2);
                    int y = rand.NextInt(1, mapData.height - 2);

                    if (x == genData.playerPos.x && y == genData.playerPos.y)
                        continue;

                    int idx = y * mapData.width + x;
                    map[idx] = TileType.Wall;
                }
            }).Schedule(inputDeps);

            inputDeps = Entities
                .WithAll<Player>()
                .ForEach((ref Position pos) =>
                {
                    var p = genData.playerPos;
                    pos = p;
                }).Schedule(inputDeps);
            
            commandmap.RemoveComponent<GenerateMapProxy>(_generateMapQuery);
            _barrier.AddJobHandleForProducer(inputDeps);
            
            return inputDeps;
        }
        

    }
}