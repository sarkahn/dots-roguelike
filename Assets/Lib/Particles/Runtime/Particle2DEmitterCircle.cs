using Unity.Entities;
using Unity.Mathematics;

namespace Sark.Particles2D
{
    public struct Particle2DEmitterCircle : IComponentData
    {
        public float angleRadians;
        public float radius;
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(Particle2DInitializationSystem))]
    [UpdateBefore(typeof(Particle2DRandomInitialSpeedSystem))]
    public class Particle2DEmitterCircleSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var rand = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));

            Entities.ForEach((
                ref DynamicBuffer<Particle2DVelocity> velBuffer,
                in DynamicBuffer<Particle2DInitializedIndex> initIndicesBuffer,
                in Particle2DEmitterCircle circle) =>
            {
                float r = circle.angleRadians;
                float halfR = circle.radius / 2;
                float min = r - halfR;
                float max = r + halfR;

                var indices = initIndicesBuffer.AsNativeArray();
                var velArr = velBuffer.AsNativeArray();

                for(int ii = 0; ii < indices.Length; ++ii )
                {
                    int i = indices[ii];

                    r = rand.NextFloat(min, max);
                    float2 dir = new float2(math.cos(r), math.sin(r));
                    velArr[i] = dir;
                }
            }).ScheduleParallel();
        }
    }


    public static class EmitterCircleBuilderExtension
    {
        public static Particle2DEmitterBuilder WithCircleEmitter(
            this Particle2DEmitterBuilder b,
            float3 dir, float radiusRadians)
        {
            b.em.AddComponentData(b.e, new Particle2DEmitterCircle
            {
                angleRadians = GetRadians(math.right(), dir),
                radius = radiusRadians,
            });
            return b;
        }
        public static Particle2DEmitterBuilder WithCircleEmitter(
            this Particle2DEmitterBuilder b,
            float angleRadians, float radiusRadians)
        {
            b.em.AddComponentData(b.e, new Particle2DEmitterCircle
            {
                angleRadians = angleRadians,
                radius = radiusRadians,
            });
            return b;
        }

        static float GetRadians(float3 from, float3 to)
        {
            return math.acos(math.dot(math.normalize(from), math.normalize(to)));
        }
    }
}