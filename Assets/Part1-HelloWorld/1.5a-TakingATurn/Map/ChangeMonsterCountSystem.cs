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

        BeginSimulationEntityCommandBufferSystem _barrier;

        public int MonsterCount { get; private set; } = 0;

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

            var prefabs = _monsterPrefabsQuery.ToEntityArray(Allocator.TempJob);

            var buffer = _barrier.CreateCommandBuffer();

            var roomsEntity = GetSingletonEntity<MapRooms>();
            var rooms = EntityManager.GetBuffer<MapRooms>(roomsEntity);

            for( int i = 0; i < target - current; ++i)
            {
                int prefabIndex = rand.NextInt(0, prefabs.Length);
                var monster = buffer.Instantiate(prefabs[prefabIndex]);

                var room = rooms[rand.NextInt(1, rooms.Length)];
                var p = RandomPointInRoom(ref rand, room);
                buffer.SetComponent<Position>(monster, p);
            }

            prefabs.Dispose();

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
            int target = math.max(0, spawn.count);

            _barrier.CreateCommandBuffer().RemoveComponent<ChangeMonsterCount>(e);

            _barrier.AddJobHandleForProducer(Dependency);

            if (monsterCount == target)
                return;

            SetMonsterCount(monsterCount, spawn.count);

            MonsterCount = target;
        }
    }
}
