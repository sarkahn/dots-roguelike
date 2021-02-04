using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Sark.Particles2D
{
    public static class Particle2DUtility
    {
        public static Particle2DEmitterBuilder MakeEmitter(EntityManager em)
        {
            var e = em.CreateEntity();
            AddParticleEmitterComponents(em, e);
            return new Particle2DEmitterBuilder(em, e);
        }

        public static Particle2DEmitterBuilder AddParticleEmitterComponents(EntityManager em, Entity e)
        {
            em.AddComponents(e, particleSystemComponents);

            return new Particle2DEmitterBuilder(em, e);

            //em.SetComponentData<Particle2DCountMax>(e, maxParticles);
            //em.SetComponentData<Particle2DEmissionRate>(e, emissionRate);
            //em.SetComponentData<Particle2DNewParticleLifetime>(e, particleLifetime);


        }

        public static void AddParticleEmitterComponents(EntityCommandBuffer ecb, Entity e, int emissionRate = 5, int particleLifetime = 5, int maxParticles = 10000)
        {
            ecb.AddComponent(e, particleSystemComponents);
            ecb.SetComponent<Particle2DCountMax>(e, maxParticles);
            ecb.SetComponent<Particle2DEmissionRate>(e, emissionRate);
            ecb.SetComponent<Particle2DNewParticleLifetime>(e, particleLifetime);
        }

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
            ComponentType.ReadWrite<Particle2DInitializedIndex>(),
        });
    }

    public struct Particle2DEmitterBuilder
    {
        public Entity e;
        public EntityManager em;

        public Particle2DEmitterBuilder(EntityManager em, Entity e)
        {
            this.em = em;
            this.e = e;
        }

        public Particle2DEmitterBuilder WithMaxParticles(int max)
        {
            em.SetComponentData<Particle2DCountMax>(e, max);
            Reserve<Particle2DPosition>(em, e, max);
            Reserve<Particle2DAcceleration>(em, e, max);
            Reserve<Particle2DVelocity>(em, e, max);
            Reserve<Particle2DColor>(em, e, max);
            Reserve<Particle2DLifetime>(em, e, max);
            Reserve<Particle2DGlyph>(em, e, max);
            Reserve<Particle2DExpiredIndex>(em, e, max / 5 + 1);
            Reserve<Particle2DInitializedIndex>(em, e, max / 5 + 1);
            return this;
        }
        public void Reserve<T>(EntityManager em, Entity e, int len)
            where T : unmanaged, IBufferElementData
        {
            var buff = em.GetBuffer<T>(e);
            buff.Capacity = len;
        }

        public Particle2DEmitterBuilder WithParticleLifetime(float lifetime)
        {
            em.SetComponentData<Particle2DNewParticleLifetime>(e, lifetime);
            return this;
        }

        public Particle2DEmitterBuilder WithEmissionRate(float rate)
        {
            em.SetComponentData<Particle2DEmissionRate>(e, rate);
            return this;
        }
    }
}