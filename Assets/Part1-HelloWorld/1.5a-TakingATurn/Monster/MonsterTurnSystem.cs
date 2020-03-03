using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    public class MonsterTurnSystem : SystemBase
    {
        EntityQuery _mapQuery;
        EntityQuery _monsterQuery;

        NativeArray<Random> _rand;

        protected override void OnCreate()
        {
            base.OnCreate();
            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<MapData>(),
                ComponentType.ReadWrite<MapState>()
                );

            _rand = new NativeArray<Random>(1, Allocator.Persistent);
            _rand[0] = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));

            RequireForUpdate(_mapQuery);
            RequireForUpdate(_monsterQuery);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _rand.Dispose(Dependency);
        }

        protected override void OnUpdate()
        {
            var rand = _rand;

            var mapEntity = _mapQuery.GetSingletonEntity();
            var mapState = EntityManager.GetBuffer<MapState>(mapEntity);
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            Entities
                .WithAll<Monster>()
                .WithAll<TakingATurn>()
                .WithStoreEntityQueryInField(ref _monsterQuery)
                .ForEach((Entity e, ref Position pos, ref Energy energy) =>
                {
                    // Come on man
                    var r = rand[0];

                    var dir = GetRandomDirection(ref r);
                    rand[0] = r;

                    int2 dest = pos + dir;
                    int destIndex = dest.y * mapData.width + dest.x;

                    if( MapUtility.CellIsUnblocked( destIndex, mapState ))
                    {
                        int oldIndex = MapUtility.PosToIndex(pos, mapData.width);
                        pos = dest;

                        MapUtility.MoveActor(e, oldIndex, destIndex, mapState);
                    }

                    energy -= Energy.ActionThreshold;
                }).Run();
        }

        static int2 GetRandomDirection(ref Random rand)
        {
            int i = rand.NextInt(0, 5);
            switch (i)
            {
                case 0: return new int2(-1, 0);
                case 1: return new int2(1, 0);
                case 2: return new int2(0, -1);
                case 3: return new int2(0, 1);
            }
            return default;
        }
    } 
}
