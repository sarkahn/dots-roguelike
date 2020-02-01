
using RLTK;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace RLTKTutorial.Part1_3
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
            
            NativeList<IntRect> rooms = new NativeList<IntRect>(50, Allocator.TempJob);

            inputDeps = Job
                .WithoutBurst()
                .WithCode(() =>
            {
                int w = mapData.width;
                int h = mapData.height;

                InitializeMap(map, w, h);

                genData.seed = genData.seed == 0 ? randomSeed : genData.seed;
                GenerateRooms(map, mapData, genData, rooms);

            }).Schedule(inputDeps);

            inputDeps = Entities
                .WithReadOnly(rooms)
                .WithAll<Player>()
                .ForEach((ref Position pos) =>
                {
                    var p = rooms[0].Center;
                    pos = p;
                }).Schedule(inputDeps);

            rooms.Dispose(inputDeps);
            
            commandBuffer.RemoveComponent<GenerateMap>(_generateMapQuery);
            _barrier.AddJobHandleForProducer(inputDeps);
            
            return inputDeps;
        }


        static void InitializeMap(DynamicBuffer<TileBuffer> map, int w, int h)
        {
            map.ResizeUninitialized(w * h);

            for (int i = 0; i < map.Length; ++i)
                map[i] = TileType.Wall;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int At(int x, int y, int width) => y * width + x;

        static void GenerateRooms(DynamicBuffer<TileBuffer> map, MapData mapData, GenerateMap genData, NativeList<IntRect> rooms )
        {
            Random rand = new Random((uint)genData.seed);
            
            for( int i = 0; i < genData.iterationCount; ++i )
            {
                int w = rand.NextInt(genData.minRoomSize, genData.maxRoomSize);
                int h = rand.NextInt(genData.minRoomSize, genData.maxRoomSize);
                int x = rand.NextInt(1, mapData.width - w - 1);
                int y = rand.NextInt(1, mapData.height - h - 1);
                IntRect newRoom = IntRect.FromPositionSize(x, y, w, h);

                bool ok = true;

                for(int roomIndex = 0; roomIndex < rooms.Length; ++roomIndex)
                    if( newRoom.Intersect(rooms[roomIndex]))
                    {
                        ok = false;
                        break;
                    }

                if (ok)
                {
                    BuildRoom(map, mapData, newRoom);

                    if(rooms.Length > 0)
                    {
                        var newPos = newRoom.Center;
                        var prevPos = rooms[rooms.Length - 1].Center;
                        if( rand.NextInt(0, 2) == 1)
                        {
                            BuildHorizontalTunnel(map, mapData, prevPos.x, newPos.x, prevPos.y);
                            BuildVerticalTunnel(map, mapData, prevPos.y, newPos.y, newPos.x);
                        }
                        else
                        {
                            BuildVerticalTunnel(map, mapData, prevPos.y, newPos.y, prevPos.x);
                            BuildHorizontalTunnel(map, mapData, prevPos.x, newPos.x, newPos.y);
                        }
                    }

                    rooms.Add(newRoom);
                }
            }
        }
        

        static void BuildRoom(DynamicBuffer<TileBuffer> map, MapData mapData, IntRect room)
        {
            for( int x = room.Min.x; x <= room.Max.x; ++x )
                for( int y = room.Min.y; y <= room.Max.y; ++y )
                {
                    map[At(x, y, mapData.width)] = TileType.Floor;
                }
        }

        static void BuildHorizontalTunnel(DynamicBuffer<TileBuffer> map, MapData mapData, 
            int x1, int x2, int y )
        {
            int xMin = math.min(x1, x2);
            int xMax = math.max(x1, x2);

            for (int x = xMin; x <= xMax; ++x)
                map[At(x, y,mapData.width)] = TileType.Floor;
        }

        static void BuildVerticalTunnel(DynamicBuffer<TileBuffer> map, MapData mapData,
            int y1, int y2, int x)
        {
            int yMin = math.min(y1, y2);
            int yMax = math.max(y1, y2);

            for (int y = yMin; y <= yMax; ++y)
                map[At(x, y, mapData.width)] = TileType.Floor;
        }
    }
}