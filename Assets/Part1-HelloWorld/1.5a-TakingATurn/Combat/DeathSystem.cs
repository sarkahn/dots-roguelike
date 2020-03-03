using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    public class DeathSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            base.OnCreate();

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var buffer = _barrier.CreateCommandBuffer().ToConcurrent();

            Entities
                .WithChangeFilter<Health>()
                .ForEach((int entityInQueryIndex, Entity e, in Health health) =>
                {
                    if (health <= 0)
                    {
                        buffer.DestroyEntity(entityInQueryIndex, e);
                    }
                }).ScheduleParallel();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    } 
}
