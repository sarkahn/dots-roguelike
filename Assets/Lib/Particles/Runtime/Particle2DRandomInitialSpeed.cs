using Unity.Entities;
using Unity.Mathematics;

namespace Sark.Particles2D
{
    public struct Particle2DRandomInitialSpeed : IComponentData
    {
        public float min;
        public float max;
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(Particle2DInitializationSystem))]
    public class Particle2DRandomInitialSpeedSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Random rand = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
            Entities.ForEach((
                ref DynamicBuffer<Particle2DVelocity> velBuffer,
                in DynamicBuffer<Particle2DInitializedIndex> initIndexBuffer,
                in Particle2DRandomInitialSpeed speed) =>
            {
                var indices = initIndexBuffer.AsNativeArray();
                var velArr = velBuffer.AsNativeArray();

                for(int ii = 0; ii < indices.Length; ++ii)
                {
                    int i = indices[ii];

                    velArr[i] = math.normalize(velArr[i]) * 
                        rand.NextFloat(speed.min, speed.max);
                }
            }).ScheduleParallel();
        }
    }
    public static class InitSpeedBuilderExtension
    {
        public static Particle2DEmitterBuilder WithRandomInitialSpeed(
            this Particle2DEmitterBuilder builder,
            float min, float max)
        {
            builder.em.AddComponentData(builder.e,
                new Particle2DRandomInitialSpeed
                {
                    min = min,
                    max = max
                });
            return builder;
        }
    }
}