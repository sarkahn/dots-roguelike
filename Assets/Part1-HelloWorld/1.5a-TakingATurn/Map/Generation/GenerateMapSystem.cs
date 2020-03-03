using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

using RLTK;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    public class GenerateMapSystem : SystemBase
    {
        EntityQuery _mapQuery;
        EntityQuery _npcQuery;

        EndSimulationEntityCommandBufferSystem _endSimBarrier;

        protected override void OnCreate()
        {
            _endSimBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<GenerateMap>(), 
                ComponentType.ReadOnly<MapData>(),
                ComponentType.ReadWrite<MapTiles>(),
                ComponentType.ReadWrite<MapRooms>()
                );

            _npcQuery = GetEntityQuery(
                ComponentType.ReadOnly<Renderable>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.Exclude<Player>()
                );

            RequireForUpdate(_mapQuery);
        }

        protected override void OnUpdate()
        {
            var buffer = _endSimBarrier.CreateCommandBuffer();

            var mapEntity = _mapQuery.GetSingletonEntity();
            var genData = EntityManager.GetComponentData<GenerateMap>(mapEntity);
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);
            var map = EntityManager.GetBuffer<MapTiles>(mapEntity);
            var rooms = EntityManager.GetBuffer<MapRooms>(mapEntity).Reinterpret<IntRect>();
            
            // If the seed is '0' we want to pass in a random seed.
            int randomSeed = UnityEngine.Random.Range(1, int.MaxValue);
            genData.seed = genData.seed == 0 ? randomSeed : genData.seed;
            uint seed = (uint)genData.seed;

            Debug.Log($"Generating map size {mapData.Size}");
            
            Job.WithCode(() =>
            {
                int w = mapData.width;
                int h = mapData.height;
                
                InitializeTiles(map, w, h);

                rooms.Clear();
                
                GenerateRooms(map, mapData, genData, rooms);

            }).Schedule();

            Entities
                .WithReadOnly(rooms)
                .WithAll<Player>()
                .ForEach((int entityInQueryIndex, Entity e, ref Position p) =>
                {
                    p.value = rooms[0].Center;
                }).Schedule();

            buffer.DestroyEntity(_npcQuery);

            buffer.RemoveComponent<GenerateMap>(_mapQuery);

            buffer.AddComponent(mapEntity, new ChangeMonsterCount { value = genData.monsterCount });

            _endSimBarrier.AddJobHandleForProducer(Dependency);
        }

        static void InitializeTiles(DynamicBuffer<MapTiles> map, int w, int h)
        {
            map.ResizeUninitialized(w * h);

            for (int i = 0; i < map.Length; ++i)
                map[i] = TileType.Wall;
        }

        static void GenerateRooms(DynamicBuffer<MapTiles> map, MapData mapData, GenerateMap genData, DynamicBuffer<IntRect> rooms )
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
                {
                    var existing = rooms[roomIndex];

                    // Accounting for walls during overlap test
                    existing.Size += 2;
                    existing.Position--;
                    if (newRoom.Intersect(existing))
                    {
                        ok = false;
                        break;
                    }
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
        
        static void BuildRoom(DynamicBuffer<MapTiles> map, MapData mapData, IntRect room)
        {
            for( int x = room.Min.x; x <= room.Max.x; ++x )
                for( int y = room.Min.y; y <= room.Max.y; ++y )
                {
                    int i = MapUtility.PosToIndex(x, y, mapData.width);
                    map[i] = TileType.Floor;
                }
        }

        static void BuildHorizontalTunnel(DynamicBuffer<MapTiles> map, MapData mapData, 
            int x1, int x2, int y )
        {
            int xMin = math.min(x1, x2);
            int xMax = math.max(x1, x2);

            for (int x = xMin; x <= xMax; ++x)
            {
                int i = MapUtility.PosToIndex(x, y, mapData.width);
                map[i] = TileType.Floor;
            }
        }

        static void BuildVerticalTunnel(DynamicBuffer<MapTiles> map, MapData mapData,
            int y1, int y2, int x)
        {
            int yMin = math.min(y1, y2);
            int yMax = math.max(y1, y2);

            for (int y = yMin; y <= yMax; ++y)
            {
                int i = MapUtility.PosToIndex(x, y, mapData.width);
                map[i] = TileType.Floor;
            }
        }
    }
}