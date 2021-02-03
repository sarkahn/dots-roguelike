using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;

namespace Sark.Particles2D
{
    public static class Particle2DUtility
    {
        static ComponentTypes particleSystemComponents = new ComponentTypes(new ComponentType[]
        {
            // Components
            ComponentType.ReadWrite<Translation>(),
            ComponentType.ReadWrite<Particle2DEmitter>(),
            ComponentType.ReadWrite<Particle2DCountAlive>(),
            ComponentType.ReadWrite<Particle2DCountMax>(),
            ComponentType.ReadWrite<Particle2DEmissionRate>(),
            ComponentType.ReadWrite<Particle2DEmissionTimer>(),
            ComponentType.ReadWrite<Particle2DNewParticleLifetime>(),

            // Buffers
            ComponentType.ReadWrite<Particle2DPosition>(),
            ComponentType.ReadWrite<Particle2DAcceleration>(),
            ComponentType.ReadWrite<Particle2DVelocity>(),
            ComponentType.ReadWrite<Particle2DColor>(),
            ComponentType.ReadWrite<Particle2DLifetime>(),
            ComponentType.ReadWrite<Particle2DGlyph>(),

            ComponentType.ReadWrite<Particle2DExpiredIndex>(),
        });

        public static void AddParticleEmitterComponents(EntityManager em, Entity e, int emissionRate = 5, int particleLifetime = 5, int maxParticles = 10000)
        {
            em.AddComponents(e, particleSystemComponents);
            em.SetComponentData<Particle2DCountMax>(e, maxParticles);
            em.SetComponentData<Particle2DEmissionRate>(e, emissionRate);
            em.SetComponentData<Particle2DNewParticleLifetime>(e, particleLifetime);

            Reserve<Particle2DPosition>(em, e, maxParticles);
            Reserve<Particle2DAcceleration>(em, e, maxParticles);
            Reserve<Particle2DVelocity>(em, e, maxParticles);
            Reserve<Particle2DColor>(em, e, maxParticles);
            Reserve<Particle2DLifetime>(em, e, maxParticles);
            Reserve<Particle2DGlyph>(em, e, maxParticles);
            Reserve<Particle2DExpiredIndex>(em, e, maxParticles / 5 + 1);
        }

        public static void Reserve<T>(EntityManager em, Entity e, int len) 
            where T : unmanaged, IBufferElementData
        {
            var buff = em.GetBuffer<T>(e);
            buff.Capacity = len;
        }

        public static void AddParticleEmitterComponents(EntityCommandBuffer ecb, Entity e, int emissionRate = 5, int particleLifetime = 5, int maxParticles = 10000)
        {
            ecb.AddComponent(e, particleSystemComponents);
            ecb.SetComponent<Particle2DCountMax>(e, maxParticles);
            ecb.SetComponent<Particle2DEmissionRate>(e, emissionRate);
            ecb.SetComponent<Particle2DNewParticleLifetime>(e, particleLifetime);
        }
    }
}