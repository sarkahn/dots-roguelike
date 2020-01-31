
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace RLTKTutorial.Part1_2
{
    [DisableAutoCreation]
    public class GenerateMapSystem : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem _barrier;
        EntityQuery _generateMapQuery;
        EntityQuery _playerQuery;


        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _generateMapQuery = GetEntityQuery(
                ComponentType.ReadOnly<GenerateMap>(), 
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
            var commandBuffer = _barrier.CreateCommandBuffer();
            
            var mapEntity = _generateMapQuery.GetSingletonEntity();
            var genData = EntityManager.GetComponentData<GenerateMap>(mapEntity);
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);
            var map = EntityManager.GetBuffer<TileBuffer>(mapEntity);
            
            // If the seed is '0' we want to pass in a random seed.
            int randomSeed = UnityEngine.Random.Range(1, int.MaxValue);

            inputDeps = Job.WithCode(() =>
            {
                int w = mapData.width;
                int h = mapData.height;

                InitializeMap(map, w, h);

                BuildBorder(map, w, h);

                genData.seed = genData.seed == 0 ? randomSeed : genData.seed;
                GenerateWalls(map, w, h, genData);

            }).Schedule(inputDeps);

            inputDeps = Entities
                .WithAll<Player>()
                .ForEach((ref Position pos) =>
                {
                    var p = genData.playerPos;
                    pos = p;
                }).Schedule(inputDeps);
            
            commandBuffer.RemoveComponent<GenerateMap>(_generateMapQuery);
            _barrier.AddJobHandleForProducer(inputDeps);
            
            return inputDeps;
        }


        static void InitializeMap(DynamicBuffer<TileBuffer> map, int w, int h)
        {
            map.ResizeUninitialized(w * h);

            for (int i = 0; i < map.Length; ++i)
                map[i] = TileType.Floor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int At(int x, int y, int width) => y * width + x;

        static void BuildBorder(DynamicBuffer<TileBuffer> map, int w, int h)
        {
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
        }

        static void GenerateWalls(DynamicBuffer<TileBuffer> map, int w, int h, GenerateMap genData )
        {
            Random rand = new Random((uint)genData.seed);
            for (int i = 0; i < genData.iterationCount; ++i)
            {
                int x = rand.NextInt(1, w - 2);
                int y = rand.NextInt(1, h - 2);

                if (x == genData.playerPos.x && y == genData.playerPos.y)
                    continue;
                
                map[At(x,y,w)] = TileType.Wall;
            }
        }
    }
}