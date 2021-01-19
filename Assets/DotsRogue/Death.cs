using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace DotsRogue
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class DeathSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithChangeFilter<HitPoints>()
                .ForEach((int entityInQueryIndex, Entity e, 
                    in HitPoints hp,
                    in MaxHitPoints max) =>
                {
                    if (hp <= 0)
                    {
                        ecb.DestroyEntity(entityInQueryIndex, e);
                    }
                }).Schedule();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }
}