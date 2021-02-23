using Unity.Entities;
using Unity.Mathematics;
using Unity.Assertions;

using Unity.Jobs;

namespace Sark.Particles2D
{
    public class Particle2DSteeringSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            double elapsed = Time.ElapsedTime;

            Entities.ForEach((
                ref DynamicBuffer<Particle2DAcceleration> accelBuffer,
                ref DynamicBuffer<Particle2DVelocity> velBuffer,
                ref DynamicBuffer<Particle2DPosition> posBuffer,
                ref Particle2DCountAlive alive,
                in Particle2DCountMax max) =>
            {
                var accelArray = accelBuffer.Reinterpret<float2>().AsNativeArray();
                var velArray = velBuffer.Reinterpret<float2>().AsNativeArray();
                var posArray = posBuffer.Reinterpret<float2>().AsNativeArray();

                for(int i = 0; i < alive; ++i)
                {
                    float2 accel = accelArray[i];
                    float2 vel = velArray[i];
                    float2 pos = posArray[i];

                    vel += accel * dt;
                    pos += vel * dt;

                    velArray[i] = vel;
                    posArray[i] = pos;
                }
            }).ScheduleParallel();
        }
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class Particle2DCleanupSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach((
                ref Particle2DCountAlive alive,
                ref DynamicBuffer<Particle2DVelocity> velBuffer,
                ref DynamicBuffer<Particle2DAcceleration> accelBuffer,
                ref DynamicBuffer<Particle2DColor> colorBuffer,
                ref DynamicBuffer<Particle2DPosition> posBuffer,
                ref DynamicBuffer<Particle2DGlyph> glyphBuffer,
                ref DynamicBuffer<Particle2DLifetime> lifetimeBuffer,
                ref DynamicBuffer<Particle2DExpiredIndex> expiredIndicesBuffer,
                ref DynamicBuffer<Particle2DInitializedIndex> initializedIndicesBuffer) =>
            {
                var expiredArr = expiredIndicesBuffer.Reinterpret<int>().AsNativeArray();
                for (int i = 0; i < expiredArr.Length; ++i)
                {
                    int j = expiredArr[i];
                    velBuffer.RemoveAtSwapBack(j);
                    accelBuffer.RemoveAtSwapBack(j);
                    colorBuffer.RemoveAtSwapBack(j);
                    posBuffer.RemoveAtSwapBack(j);
                    glyphBuffer.RemoveAtSwapBack(j);
                    lifetimeBuffer.RemoveAtSwapBack(j);
                    --alive;
                }
                expiredIndicesBuffer.Clear();
                initializedIndicesBuffer.Clear();
            }).ScheduleParallel();
        }


    }

    // https://docs.unity3d.com/Packages/com.unity.entities@0.17/manual/ecs_entities_foreach.html#custom-delegates
    static class NineRefForEach
    {
        [Unity.Entities.CodeGeneratedJobForEach.EntitiesForEachCompatible]
        public delegate void CustomForEach<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
            ref T0 t0, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4,
            ref T5 t5, ref T6 t6, ref T7 t7, ref T8 t8);

        public static TDescription ForEach<TDescription, T0, T1, T2, T3, T4, T5, T6, T7, T8>(
            this TDescription description, CustomForEach<T0, T1, T2, T3, T4, T5, T6, T7, T8> codeToRun)
            where TDescription : struct, Unity.Entities.CodeGeneratedJobForEach.ISupportForEachWithUniversalDelegate =>
                LambdaForEachDescriptionConstructionMethods.ThrowCodeGenException<TDescription>();
    }
}


