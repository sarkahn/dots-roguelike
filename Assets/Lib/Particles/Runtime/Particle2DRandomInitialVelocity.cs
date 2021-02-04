using Unity.Collections;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Sark.Particles2D
{
    public struct Particle2DRandomInitialVelocity : IComponentData
    {
        public float2 minVel;
        public float2 maxVel;
        public float2 minAccel;
        public float2 maxAccel;
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(Particle2DInitializationSystem))]
    public class Particle2DRandomInitialVelocitySystem : SystemBase
    {
        NativeArray<Random> _randArray;

        protected override void OnCreate()
        {
            _randArray = new NativeArray<Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);
            for (int i = 0; i < _randArray.Length; ++i)
                _randArray[i] = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        }

        protected override void OnDestroy()
        {
            _randArray.Dispose(Dependency);
        }

        protected override void OnUpdate()
        {
            var randArray = _randArray;
            Entities
                .WithNativeDisableParallelForRestriction(randArray)
                .ForEach((int nativeThreadIndex,
                ref DynamicBuffer<Particle2DVelocity> velBuffer,
                ref DynamicBuffer<Particle2DAcceleration> accelBuffer,
                in Particle2DRandomInitialVelocity initialVel,
                in DynamicBuffer<Particle2DInitializedIndex> initializedBuffer) =>
            {
                var newParticleIndices = initializedBuffer.AsNativeArray();
                var accelArr = accelBuffer.Reinterpret<float2>().AsNativeArray();
                var velArr = velBuffer.Reinterpret<float2>().AsNativeArray();
                for(int arrIndex = 0; arrIndex < newParticleIndices.Length;++arrIndex)
                {
                    int newParticleIndex = newParticleIndices[arrIndex];
                    var rand = randArray[nativeThreadIndex];

                    float2 vel = rand.NextFloat2(initialVel.minVel, initialVel.maxVel);
                    float2 accel = rand.NextFloat2(initialVel.minAccel, initialVel.maxAccel);

                    velArr[newParticleIndex] = vel;
                    accelArr[newParticleIndex] = accel;

                    randArray[nativeThreadIndex] = rand;
                }
            }).WithoutBurst().ScheduleParallel();
        }
    }

    public static class InitialVelBuilderExtension
    {
        public static Particle2DEmitterBuilder WithRandomInitialVelocity(
            this Particle2DEmitterBuilder builder,
            float2 minVel, float2 maxVel,
            float2 minAccel, float2 maxAccel)
        {
            builder.em.AddComponentData<Particle2DRandomInitialVelocity>(builder.e,
                new Particle2DRandomInitialVelocity
                {
                    minVel = minVel,
                    maxVel = maxVel,
                    minAccel = minAccel,
                    maxAccel = maxAccel
                });
            return builder;
        }
    }
}