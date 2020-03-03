using UnityEngine;
using System.Collections;

using Unity.Entities;
using Unity.Collections;

using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using RLTK;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    [UpdateAfter(typeof(GenerateMapSystem))]
    public class ChangeMonsterCountSystem : SystemBase
    {
        EntityQuery _changeMonsterCountQuery;
        EntityQuery _monsterPrefabsQuery;
        EntityQuery _monstersQuery;
        EntityQuery _mapQuery;

        BeginSimulationEntityCommandBufferSystem _barrier;

        public int MonsterCount => _monstersQuery.CalculateEntityCount();

        protected override void OnCreate()
        {
            base.OnCreate();

            _monsterPrefabsQuery = GetEntityQuery(
                ComponentType.ReadOnly<Prefab>(),
                ComponentType.ReadOnly<Monster>()
                );

            _monstersQuery = GetEntityQuery(
                ComponentType.ReadOnly<Monster>()
                );

            _changeMonsterCountQuery = GetEntityQuery(
                ComponentType.ReadOnly<ChangeMonsterCount>()
                );

            _mapQuery = GetEntityQuery(
                ComponentType.ReadWrite<MapState>(),
                ComponentType.ReadOnly<MapData>()
                );

            _barrier = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            RequireForUpdate(_changeMonsterCountQuery);
        }

        void SetMonsterCount(int current, int target)
        {

            if (current > target)
            {
                var monsters = _monstersQuery.ToEntityArray(Allocator.TempJob);

                int removeCount = current - target;
                var slice = new NativeSlice<Entity>(monsters, current - removeCount, removeCount);

                EntityManager.DestroyEntity(slice);

                monsters.Dispose();
                return;
            }
            
            uint seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);
            Random rand = new Random(seed);

            var buffer = _barrier.CreateCommandBuffer();

            var roomsEntity = GetSingletonEntity<MapRooms>();
            var rooms = EntityManager.GetBuffer<MapRooms>(roomsEntity);

            if (rooms.Length <= 1)
                return;

            var prefabs = _monsterPrefabsQuery.ToEntityArray(Allocator.TempJob);

            var mapEntity = _mapQuery.GetSingletonEntity();
            var mapState = EntityManager.GetBuffer<MapState>(mapEntity);
            int mapWidth = EntityManager.GetComponentData<MapData>(mapEntity).width;

            Job
                .WithReadOnly(prefabs)
                .WithReadOnly(rooms)
                .WithCode(() =>
            {
                for (int i = 0; i < target - current; ++i)
                {
                    int prefabIndex = rand.NextInt(0, prefabs.Length);

                    // Try to find a viable spot up to 5 times
                    for( int j = 0; j < 5; ++j )
                    {
                        var room = rooms[rand.NextInt(1, rooms.Length)];
                        var p = RandomPointInRoom(ref rand, room);

                        int index = MapUtility.PosToIndex(p, mapWidth);

                        if (mapState[index].blocked)
                            continue;

                        var monster = buffer.Instantiate(prefabs[prefabIndex]);

                        mapState[index] = new MapState
                        {
                            blocked = true,
                            content = monster
                        };

                        buffer.SetComponent<Position>(monster, p);
                        buffer.SetComponent<Speed>(monster, rand.NextInt(20, 30));
                        break;
                    }
                }
            }).Schedule();

            prefabs.Dispose(Dependency);
        }

        static int2 RandomPointInRoom(ref Random rand, IntRect room)
        {
            int x = rand.NextInt(room.xMin, room.xMax + 1);
            int y = rand.NextInt(room.yMin, room.yMax + 1);
            return new int2(x, y);
        }

        protected override void OnUpdate()
        {
            var e = GetSingletonEntity<ChangeMonsterCount>();
            var spawn = EntityManager.GetComponentData<ChangeMonsterCount>(e);

            int monsterCount = _monstersQuery.CalculateEntityCount();
            int target = math.max(0, spawn.value);

            _barrier.CreateCommandBuffer().RemoveComponent<ChangeMonsterCount>(e);
            _barrier.AddJobHandleForProducer(Dependency);

            if (monsterCount == target)
                return;

            SetMonsterCount(monsterCount, spawn.value);
        }
    }
}
