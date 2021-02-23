using Unity.Collections;
using Unity.Entities;

namespace DotsRogue
{
    public struct Energy : IComponentData
    {
        public int Value;
        public static implicit operator int(Energy b) => b.Value;
        public static implicit operator Energy(int v) =>
            new Energy { Value = v };

        public static readonly int ActionThreshold = 100;
    }


    public struct Speed : IComponentData
    {
        public int Value;
        public static implicit operator int(Speed b) => b.Value;
        public static implicit operator Speed(int v) =>
            new Speed { Value = v };
    }

    public struct Actor : IComponentData
    { }

    public struct TakingATurn : IComponentData
    { }

    public class TurnActor
    {}

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class TurnBeginSystem : SystemBase
    {
        EntityQuery _actingActors;

        protected override void OnCreate()
        {
            
            var actorType = new ComponentType[]
            {
                ComponentType.ReadOnly<Actor>(),
                ComponentType.ReadOnly<Energy>(),
                ComponentType.ReadOnly<Speed>()
            };

            var actingActorType = new ComponentType[]
            {
                ComponentType.ReadOnly<Actor>(),
                ComponentType.ReadOnly<Energy>(),
                ComponentType.ReadOnly<Speed>(),
                ComponentType.ReadOnly<TakingATurn>()
            };

            RequireForUpdate(GetEntityQuery(actorType));

            _actingActors = GetEntityQuery(actingActorType);
        }

        protected override void OnUpdate()
        {
            // Don't hand out any more turns until all current actors with turns have acted
            if (!_actingActors.IsEmptyIgnoreFilter)
                return;

            while (_actingActors.IsEmptyIgnoreFilter)
            {
                var ecb = new EntityCommandBuffer(Allocator.TempJob);
                Entities
                    .WithAll<Actor>()
                    .ForEach((Entity e,
                    ref Energy energy, in Speed speed) =>
                {
                    energy += speed;
                    if (energy >= Energy.ActionThreshold)
                        ecb.AddComponent<TakingATurn>(e);
                }).Run();
                ecb.Playback(EntityManager);
                ecb.Dispose();
            }
        }
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class TurnEndSystem : SystemBase
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
                .WithAll<Actor, TakingATurn>()
                .WithChangeFilter<Energy>()
                .ForEach((int entityInQueryIndex, Entity e, in Energy energy) =>
            {
                if (energy < Energy.ActionThreshold)
                {
                    ecb.RemoveComponent<TakingATurn>(entityInQueryIndex, e);
                }
            }).ScheduleParallel();
            _barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
