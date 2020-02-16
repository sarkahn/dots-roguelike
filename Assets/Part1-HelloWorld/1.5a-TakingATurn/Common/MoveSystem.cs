
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    public class MoveSystem : JobComponentSystem
    {
        EntityQuery _mapQuery;
        EntityQuery _moveQuery;

        EndSimulationEntityCommandBufferSystem _barrier;
        
        protected override void OnCreate()
        {
            _mapQuery = GetEntityQuery(
                ComponentType.ReadWrite<MapTiles>(),
                ComponentType.ReadOnly<MapData>()
                );

            _moveQuery = GetEntityQuery(
                ComponentType.ReadWrite<Position>(),
                ComponentType.ReadOnly<Movement>()
                );
            
            _moveQuery.AddChangedVersionFilter(typeof(Movement));

            RequireForUpdate(_mapQuery);
            RequireForUpdate(_moveQuery);

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_moveQuery.CalculateEntityCount() == 0)
                return inputDeps;

            var mapEntity = _mapQuery.GetSingletonEntity();
            var map = EntityManager.GetBuffer<MapTiles>(mapEntity);
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            var buffer = _barrier.CreateCommandBuffer().ToConcurrent();

            var nameFromEntity = GetComponentDataFromEntity<Name>(true);

            inputDeps = Entities
                //.WithReadOnly(nameFromEntity)
                .WithReadOnly(map)
                //.WithoutBurst()
                .WithAll<Actor>()
                .WithAll<TakingATurn>()
                .WithNone<ActionPerformed>()
                .ForEach((int entityInQueryIndex, Entity e, ref Position p, ref Movement move) =>
            {

                if (move.value.x == 0 && move.value.y == 0)
                    return;

                int2 dest = p.value + move.value;
                int index = dest.y * mapData.width + dest.x;
                
                move = int2.zero;

                //if(nameFromEntity.HasComponent(e))
                //    Debug.Log($"{nameFromEntity[e].ToString()} moved");

                var performed = new ActionPerformed { cost = 100 };
                buffer.AddComponent(entityInQueryIndex, e, performed);

                if (index < 0 || index >= map.Length)
                    return;

                if( map[index] != TileType.Wall )
                    p = dest;
                
            }).Schedule(inputDeps);

            _barrier.AddJobHandleForProducer(inputDeps);

            return inputDeps;
        }
    }
}