using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

using Sark.Common.Geometry;
using Sark.Common.GridUtil;
using Sark.Terminals;

using Input = UnityEngine.Input;
using KeyCode = UnityEngine.KeyCode;
using Sark.RNG.RandomExtensions;

namespace DotsRogue
{
    public struct MapSize : IComponentData
    {
        public int2 Value;
        public static implicit operator int2(MapSize b) => b.Value;
        public static implicit operator MapSize(int2 v) =>
            new MapSize { Value = v };
        public int Length => Value.x * Value.y;
        public int Width => Value.x;
        public int Height => Value.y;
    }

    public struct Map : IComponentData
    {
    };

    public struct GenerateMap : IComponentData
    {
        public Entity PlayerPrefab;
        public GenMapSettings Settings;

        public static GenerateMap Default =>
            new GenerateMap
            {
                PlayerPrefab = default,
                Settings = GenMapSettings.Default
            };

        public static void AddToEntity(EntityManager em, Entity e,
            GenMapSettings settings,
            Entity playerPrefab,
            NativeList<Entity> monsterPrefabs,
            NativeList<Entity> itemPrefabs)
        {
            em.AddComponentData(e, new GenerateMap
            {
                PlayerPrefab = playerPrefab,
                Settings = settings
            });

            var monsterBuffer = em.AddBuffer<MonsterPrefabsBuffer>(e);
            foreach (var monster in monsterPrefabs)
                monsterBuffer.Add(monster);

            var itemsBuffer = em.AddBuffer<ItemPrefabsBuffer>(e);
            foreach (var item in itemPrefabs)
                itemsBuffer.Add(item);

            if (!monsterPrefabs.IsCreated)
                return;

        }
    }

    [System.Serializable]
    public struct GenMapSettings
    {
        public uint Seed;
        public int Iterations;
        public int MinRoomSize;
        public int MaxRoomSize;
        public int MonstersPerRoomMin;
        public int MonstersPerRoomMax;
        public int ItemsPerRoomMin;
        public int ItemsPerRoomMax;

        public static GenMapSettings Default => new GenMapSettings
        {
            Seed = 1,
            Iterations = 15,
            MinRoomSize = 5,
            MaxRoomSize = 10,
            MonstersPerRoomMin = 0,
            MonstersPerRoomMax = 4,
            ItemsPerRoomMin = 0,
            ItemsPerRoomMax = 2,
        };
    }

    public struct MonsterPrefabsBuffer : IBufferElementData
    {
        public Entity Value;
        public static implicit operator Entity(MonsterPrefabsBuffer b) => b.Value;
        public static implicit operator MonsterPrefabsBuffer(Entity v) =>
            new MonsterPrefabsBuffer { Value = v };
    }

    public struct ItemPrefabsBuffer : IBufferElementData
    {
        public Entity Value;
        public static implicit operator Entity(ItemPrefabsBuffer b) => b.Value;
        public static implicit operator ItemPrefabsBuffer(Entity v) =>
            new ItemPrefabsBuffer { Value = v };
    }

    public enum MapTileType : byte
    {
        Wall,
        Floor
    };

    public struct MapTileAssetsBuffer : IBufferElementData
    {
        public TerminalTile value;
        public static implicit operator TerminalTile(MapTileAssetsBuffer b) => b.value;
        public static implicit operator MapTileAssetsBuffer(TerminalTile v) =>
            new MapTileAssetsBuffer { value = v };
    }

    public struct MapTileAssetsBufferJobContext
    {
        BufferFromEntity<MapTileAssetsBuffer> bfe;
        public Entity entity;

        public MapTileAssetsBufferJobContext(SystemBase sys, bool readOnly) : 
            this(sys, sys.GetSingletonEntity<MapTileAssetsBuffer>(), readOnly)
        { }

        public MapTileAssetsBufferJobContext(SystemBase sys, Entity entity, bool readOnly)
        {
            bfe = sys.GetBufferFromEntity<MapTileAssetsBuffer>(readOnly);
            this.entity = entity;
        }

        public NativeArray<TerminalTile> Tiles => bfe[entity].Reinterpret<TerminalTile>().AsNativeArray(); 
    }

    public struct MapTilesBuffer : IBufferElementData
    {
        public MapTileType Value;
        public static implicit operator MapTileType(MapTilesBuffer b) => b.Value;
        public static implicit operator MapTilesBuffer(MapTileType v) =>
            new MapTilesBuffer { Value = v };
    }

    public struct MapJobContext
    {
        BufferFromEntity<MapTilesBuffer> tilesFromEntity;
        ComponentDataFromEntity<MapSize> sizeFromEntity;
        public Entity entity;

        public int2 Size => sizeFromEntity[entity];
        public GridData2D<MapTileType> Grid => new GridData2D<MapTileType>(
            tilesFromEntity[entity].Reinterpret<MapTileType>().AsNativeArray(),
            Size
            );

        public MapJobContext(SystemBase sys, Entity e, bool readOnly = false)
        {
            tilesFromEntity = sys.GetBufferFromEntity<MapTilesBuffer>(readOnly);
            sizeFromEntity = sys.GetComponentDataFromEntity<MapSize>(readOnly);
            entity = e;
        }

        public MapJobContext(SystemBase sys, bool readOnly = false)
            : this(sys, sys.GetSingletonEntity<Map>(), readOnly)
        { }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class MapInputSystem : SystemBase
    {
        EntityQuery _playerPrefabQuery;
        EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _playerPrefabQuery = GetEntityQuery(typeof(Player), typeof(Prefab));
            _barrier = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
            RequireSingletonForUpdate<Map>();
        }

        protected override void OnUpdate()
        {
            if(ShouldRebuildMap())
            {
                QueueMapRebuild();
            }
        }

        bool ShouldRebuildMap()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }

        void QueueMapRebuild()
        {
            var map = GetSingletonEntity<Map>();
            var playerPrefab = _playerPrefabQuery.IsEmptyIgnoreFilter ? Entity.Null :
                _playerPrefabQuery.GetSingletonEntity();
            uint seed = (uint)(Time.ElapsedTime * 1000);

            var ecb = _barrier.CreateCommandBuffer();
            var settings = GenMapSettings.Default;
            settings.Seed = seed;
            ecb.AddComponent(map, new GenerateMap
            {
                PlayerPrefab = playerPrefab,
                Settings = settings
            }); 
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class GenerateMapSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;
        EntityQuery _generateMapQuery;
        EntityQuery _monstersQuery;

        NativeReference<Random> _rng;

        protected override void OnCreate()
        {
            _monstersQuery = GetEntityQuery(
                ComponentType.ReadOnly<Monster>());
            _barrier = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
            RequireForUpdate(_generateMapQuery);
            //Debug.Log("Creating GenerateMapSystem");
            _rng = new NativeReference<Random>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _rng.Dispose(Dependency);
        }

        protected override void OnUpdate()
        {
            //Debug.Log("Generating map");
            NativeList<Rect2D> rooms = new NativeList<Rect2D>(50, Allocator.TempJob);

            GenerateMap(rooms);

            PlacePlayer(rooms);

            NativeHashSet<int2> usedPoints = new NativeHashSet<int2>(50, Allocator.TempJob);
            PlaceMonsters(rooms, usedPoints);
            PlaceItems(rooms, usedPoints);

            rooms.Dispose(Dependency);
            usedPoints.Dispose(Dependency);

            _barrier.AddJobHandleForProducer(Dependency);
        }

        void PlacePlayer(NativeList<Rect2D> rooms)
        {
            var ecb = _barrier.CreateCommandBuffer();

            Entity existingPlayer = HasSingleton<Player>() ?
                GetSingletonEntity<Player>() : Entity.Null;

            Entities
                .WithReadOnly(rooms)
                .ForEach((in GenerateMap generate) =>
                {
                    if (generate.PlayerPrefab == Entity.Null)
                        return;

                    if (existingPlayer != Entity.Null)
                        ecb.DestroyEntity(existingPlayer);

                    int2 pos = rooms[0].Center;
                    var player = ecb.Instantiate(generate.PlayerPrefab);
                    ecb.SetComponent<Position>(player, pos);
                }).Schedule();
        }

        void PlaceMonsters(NativeList<Rect2D> rooms, NativeHashSet<int2> placed)
        {
            var ecb = _barrier.CreateCommandBuffer();
            ecb.DestroyEntity(_monstersQuery);
            var rng = _rng;
            Entities
                //.WithoutBurst()
                .WithReadOnly(rooms)
                .ForEach((
                    in DynamicBuffer<MonsterPrefabsBuffer> monstersBuffer,
                    in GenerateMap genMap) =>
                {
                    if (monstersBuffer.IsEmpty)
                        return;

                    var gen = genMap.Settings;

                    var monsters = monstersBuffer.Reinterpret<Entity>().AsNativeArray();

                    SpawnInRooms(rooms, monsters, 
                        gen.MonstersPerRoomMin, gen.MonstersPerRoomMax,
                        placed, rng, ecb);
                }).Schedule();
        }

        void PlaceItems(NativeList<Rect2D> rooms, NativeHashSet<int2> placed)
        {
            var ecb = _barrier.CreateCommandBuffer();
            var rng = _rng;
            Entities
                //.WithoutBurst()
                .WithReadOnly(rooms)
                .ForEach((
                    in DynamicBuffer<ItemPrefabsBuffer> itemsBuffer,
                    in GenerateMap genMap) =>
                {
                    if (itemsBuffer.IsEmpty)
                        return;

                    var gen = genMap.Settings;

                    var items = itemsBuffer.Reinterpret<Entity>().AsNativeArray();

                    SpawnInRooms(rooms, items, 
                        gen.ItemsPerRoomMin, gen.ItemsPerRoomMax, placed, rng, ecb);

                }).Schedule();
        }

        static void SpawnInRooms(
            NativeList<Rect2D> rooms, 
            NativeArray<Entity> entities, int min, int max, 
            NativeHashSet<int2> usedPoints,
            NativeReference<Random> rng,
            EntityCommandBuffer ecb)
        {
            // Room 0 is the player spawn room
            for (int i = 1; i < rooms.Length; ++i)
            {
                int count = rng.NextInt(min, max + 1);

                for (int j = 0; j < count; ++j)
                {
                    var room = rooms[i];
                    int2 p = rng.NextInt2(room.Min, room.Max + 1);
                    if (usedPoints.Contains(p))
                    {
                        p = rng.NextInt2(room.Min, room.Max + 1);
                        if (usedPoints.Contains(p))
                            p = rng.NextInt2(room.Min, room.Max + 1);
                        if (usedPoints.Contains(p))
                            continue;
                    }

                    int ei = rng.NextInt(0, entities.Length);
                    var e = ecb.Instantiate(entities[ei]);
                    ecb.SetComponent<Position>(e, p);
                    usedPoints.Add(p);
                }
            }
        }

        void GenerateMap(NativeList<Rect2D> rooms)
        {
            uint randSeed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
            var ecb = _barrier.CreateCommandBuffer();
            var rng = _rng;
            Entities
                //.WithoutBurst()
                .WithStoreEntityQueryInField(ref _generateMapQuery)
                .WithAll<Map>()
                .ForEach((Entity e,
                    ref DynamicBuffer<MapTilesBuffer> tilesBuffer,
                    in MapSize size,
                    in GenerateMap genMap) =>
                {
                    var gen = genMap.Settings;
                    Assert.IsTrue(size.Length > 0);
                    Assert.IsTrue(gen.MinRoomSize <= gen.MaxRoomSize);
                    Assert.IsTrue(gen.Seed > 0);

                    rng.Value = new Random(gen.Seed == 0 ? randSeed : gen.Seed);

                    ecb.RemoveComponent<GenerateMap>(e);

                    unsafe
                    {
                        UnsafeUtility.MemClear(tilesBuffer.GetUnsafePtr(), tilesBuffer.Length);
                    }

                    var tiles = tilesBuffer.Reinterpret<MapTileType>().AsNativeArray();

                    for(int i = 0; i < gen.Iterations; ++i)
                    {
                        int2 roomSize = rng.NextInt2(gen.MinRoomSize,
                            gen.MaxRoomSize);
                        int2 roomPos = rng.NextInt2(1, size - roomSize - 1);
                        Rect2D newRoom = Rect2D.FromPositionSize(roomPos, roomSize);

                        bool ok = true;

                        for(int roomIndex = 0; roomIndex < rooms.Length; ++roomIndex)
                            if(newRoom.Intersect(rooms[roomIndex]))
                            {
                                ok = false;
                                break;
                            }

                        if(ok)
                        {
                            BuildRoom(tiles, size.Width, newRoom);

                            if(rooms.Length > 0)
                            {
                                var prevRoom = rooms[rooms.Length - 1];
                                BuildTunnelsBetweenRooms(prevRoom, newRoom, 
                                    tiles, size.Width, rng);
                            }

                            rooms.Add(newRoom);
                        }
                    }
                }).Schedule();
        }

        static void BuildRoom(NativeArray<MapTileType> tiles, int mapWidth, Rect2D room)
        {
            for (int x = room.Min.x; x <= room.Max.x; ++x)
                for (int y = room.Min.y; y <= room.Max.y; ++y)
                {
                    int i = Grid2D.PosToIndex(x, y, mapWidth);
                    tiles[i] = MapTileType.Floor;
                }
        }

        static void BuildTunnelsBetweenRooms(
            Rect2D roomA, 
            Rect2D roomB,
            NativeArray<MapTileType> tiles, 
            int mapWidth, 
            NativeReference<Random> rng)
        {
            var newPos = roomB.Center;
            var prevPos = roomA.Center;

            if (rng.NextInt(0, 2) == 1)
            {
                BuildHorizontalTunnel(tiles, mapWidth, prevPos.x, newPos.x, prevPos.y);
                BuildVerticalTunnel(tiles, mapWidth, prevPos.y, newPos.y, newPos.x);
            }
            else
            {
                BuildVerticalTunnel(tiles, mapWidth, prevPos.y, newPos.y, prevPos.x);
                BuildHorizontalTunnel(tiles, mapWidth, prevPos.x, newPos.x, newPos.y);
            }
        }

        static void BuildHorizontalTunnel(NativeArray<MapTileType> tiles, int mapWidth,
            int x1, int x2, int y)
        {
            int xMin = math.min(x1, x2);
            int xMax = math.max(x1, x2);

            for (int x = xMin; x <= xMax; ++x)
            {
                int i = Grid2D.PosToIndex(x, y, mapWidth);
                tiles[i] = MapTileType.Floor;
            }
        }

        static void BuildVerticalTunnel(NativeArray<MapTileType> tiles, int mapWidth,
            int y1, int y2, int x)
        {
            int yMin = math.min(y1, y2);
            int yMax = math.max(y1, y2);

            for (int y = yMin; y <= yMax; ++y)
            {
                int i = Grid2D.PosToIndex(x, y, mapWidth);
                tiles[i] = MapTileType.Floor;
            }
        }
    }
}

