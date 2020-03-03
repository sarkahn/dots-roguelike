using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    public class TurnBeginSystem : SystemBase
    {
        EntityQuery _actingActors;

        BeginSimulationEntityCommandBufferSystem _beginSimBarrier;

        protected override void OnCreate()
        {
            base.OnCreate();

            _actingActors = GetEntityQuery(
                ComponentType.ReadOnly<Actor>(),
                ComponentType.ReadOnly<TakingATurn>()
                );

            _beginSimBarrier = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            // Don't start any new turns until all current turns are complete
            if (!_actingActors.IsEmptyIgnoreFilter)
                return;

            var buffer = _beginSimBarrier.CreateCommandBuffer().ToConcurrent();

            Entities.ForEach((Entity e, int entityInQueryIndex, ref Energy energy, in Speed speed) =>
            {
                energy += speed;
                if (energy >= Energy.ActionThreshold)
                    buffer.AddComponent<TakingATurn>(entityInQueryIndex, e);
            }).ScheduleParallel();

            _beginSimBarrier.AddJobHandleForProducer(Dependency);
        }
    } 
}
