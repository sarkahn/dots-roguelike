using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace DotsRogue
{
    public struct Combat : IComponentData
    {
        public Entity Attacker;
        public Entity Defender;
        public int Damage;
    }

    public struct CombatAttackBuffer : IBufferElementData
    {
        public Entity Target;
        public int Damage;
    }

    public struct CombatDefendBuffer : IBufferElementData
    {
        public Entity Attacker;
        public int Damage;
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateBefore(typeof(DeathSystem))]
    public class ResolveCombatSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer();
            Entities
                .WithChangeFilter<CombatDefendBuffer>()
                .ForEach((Entity e, 
                ref DynamicBuffer<CombatDefendBuffer> defendBuffer,
                in Defense defense) =>
                {
                    for (int i = 0; i < defendBuffer.Length; ++i)
                    {
                        var combat = defendBuffer[i]; 
                        combat.Damage = math.max(0, combat.Damage - defense);
                        defendBuffer[i] = combat;
                    }
                }).Schedule();

            Entities
                .WithChangeFilter<CombatDefendBuffer>()
                .ForEach((Entity e, ref HitPoints hp,
                ref DynamicBuffer<CombatDefendBuffer> defendBuffer) =>
                {
                    for (int i = 0; i < defendBuffer.Length; ++i)
                    {
                        var combat = defendBuffer[i];
                        hp = math.max(0, hp - combat.Damage);
                    }
                    // Clear the buffer - we use an ECB so other systems
                    // can respond to combat after it's resolved
                    // IE: For the combat log
                    ecb.SetBuffer<CombatDefendBuffer>(e);
                }).Schedule();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }


}
