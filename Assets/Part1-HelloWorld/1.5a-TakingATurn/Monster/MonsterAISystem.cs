using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    [UpdateAfter(typeof(VisibilitySystem))]
    public class MonsterAISystem : JobComponentSystem
    {
        EntityQuery _playerQuery;
        EntityQuery _mapQuery;

        EndSimulationEntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _playerQuery = GetEntityQuery(
                ComponentType.ReadOnly<Player>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<Name>()
                );

            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<MapData>()
                );

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var mapEntity = _mapQuery.GetSingletonEntity();
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            // Early out if the map is regenerating.
            if (EntityManager.HasComponent<GenerateMap>(mapEntity))
                return inputDeps;

            if (_playerQuery.IsEmptyIgnoreFilter)
                return inputDeps;
            
            var playerEntity = _playerQuery.GetSingletonEntity();
            var playerPos = (int2)EntityManager.GetComponentData<Position>(playerEntity);
            var playerName = EntityManager.GetComponentData<Name>(playerEntity);
            
            int playerIndex = playerPos.y * mapData.width + playerPos.x;

            var buffer = _barrier.CreateCommandBuffer().ToConcurrent();

            inputDeps = Entities
                .WithoutBurst()
                .WithAll<Monster>()
                .WithAll<TakingATurn>()
                .WithNone<ActionPerformed>()
                .ForEach((int entityInQueryIndex, Entity e, in DynamicBuffer<TilesInView> view, in Name name) =>
                {
                    var action = new ActionPerformed();

                    if(view[playerIndex] )
                    {
                        //Debug.Log($"{name} shouts angrily at {playerName}");
                        action.cost = 100;
                    } else
                    {
                        //Debug.Log($"{name} stands around");
                        action.cost = 50;
                    }

                    buffer.AddComponent(entityInQueryIndex, e, action);
                }).Schedule(inputDeps);


            _barrier.AddJobHandleForProducer(inputDeps);

            return inputDeps;
        }
    }
}