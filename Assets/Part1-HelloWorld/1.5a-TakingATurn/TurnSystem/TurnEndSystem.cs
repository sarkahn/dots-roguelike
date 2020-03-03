using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    public class TurnEndSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _endSimBarrier;

        protected override void OnCreate()
        {
            base.OnCreate();

            _endSimBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var buffer = _endSimBarrier.CreateCommandBuffer().ToConcurrent();

            Entities
                .WithAll<TakingATurn>()
                .WithAll<Actor>()
                .ForEach((Entity e, int entityInQueryIndex, in Energy energy) =>
                {
                    if (energy < Energy.ActionThreshold)
                        buffer.RemoveComponent<TakingATurn>(entityInQueryIndex, e);
                }).ScheduleParallel();

            _endSimBarrier.AddJobHandleForProducer(Dependency);
        }
    } 
}
