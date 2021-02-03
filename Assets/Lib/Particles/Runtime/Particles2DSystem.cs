using Unity.Entities;
using Unity.Mathematics;
using Unity.Assertions;

using Color32 = UnityEngine.Color32;

namespace Sark.Particles2D
{
    public class Particle2DSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;

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

                    accelArray[i] = 0;
                }
            }).ScheduleParallel();
        }
    }
}


