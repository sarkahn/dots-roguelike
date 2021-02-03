using Unity.Entities;
using Unity.Mathematics;
using Unity.Assertions;

using Color32 = UnityEngine.Color32;
using Unity.Burst;
using Unity.Collections;

namespace Sark.Particles2D
{
    public class Particle2DMainUpdateSystem : SystemBase
    {
        EntityQuery _emitterQuery;

        protected override void OnCreate()
        {
            _emitterQuery = GetEntityQuery(
                ComponentType.ReadOnly<Particle2DEmitter>());
        }

        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;
            double elapsed = Time.ElapsedTime;

            Dependency = new EmissionJob
            {
                dt = dt,
                Particle2DAccelerationHandle = GetBufferTypeHandle<Particle2DAcceleration>(false),
                Particle2DVelocityHandle = GetBufferTypeHandle<Particle2DVelocity>(false),
                Particle2DPositionHandle = GetBufferTypeHandle<Particle2DPosition>(false),
                Particle2DColorHandle = GetBufferTypeHandle<Particle2DColor>(false),
                Particle2DGlyphHandle = GetBufferTypeHandle<Particle2DGlyph>(false),
                Particle2DLifetimeHandle = GetBufferTypeHandle<Particle2DLifetime>(false),
                Particle2DCountAliveHandle = GetComponentTypeHandle<Particle2DCountAlive>(false),
                Particle2DEmissionTimerHandle = GetComponentTypeHandle<Particle2DEmissionTimer>(false),
                Particle2DCountMaxHandle = GetComponentTypeHandle<Particle2DCountMax>(true),
                Particle2DEmissionRateHandle = GetComponentTypeHandle<Particle2DEmissionRate>(true),
                Particle2DNewParticleLifetimeHandle = GetComponentTypeHandle<Particle2DNewParticleLifetime>(true),
            }.ScheduleParallel(_emitterQuery, Dependency);

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

            Entities.ForEach((int entityInQueryIndex,
                ref DynamicBuffer<Particle2DExpiredIndex> expiredIndicesBuffer,
                ref DynamicBuffer<Particle2DLifetime> lifeBuffer) =>
            {
                var arr = lifeBuffer.AsNativeArray();
                for(int i = 0; i < arr.Length; ++i)
                {
                    var lifetime = arr[i];

                    lifetime.remaining = math.max(0, lifetime.remaining - dt);

                    if( lifetime.remaining == 0 )
                    {
                        expiredIndicesBuffer.Add(i);
                    }

                    arr[i] = lifetime;
                }
            }).ScheduleParallel();
        }
    }

    [UpdateAfter(typeof(Particle2DMainUpdateSystem))]
    public class Particle2DCleanupParticlesSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((
                ref Particle2DCountAlive alive,
                ref DynamicBuffer<Particle2DVelocity> velBuffer,
                ref DynamicBuffer<Particle2DAcceleration> accelBuffer,
                ref DynamicBuffer<Particle2DColor> colorBuffer,
                ref DynamicBuffer<Particle2DPosition> posBuffer,
                ref DynamicBuffer<Particle2DGlyph> glyphBuffer,
                ref DynamicBuffer<Particle2DLifetime> lifetimeBuffer,
                ref DynamicBuffer<Particle2DExpiredIndex> expiredIndicesBuffer) =>
            {
                var expiredArr = expiredIndicesBuffer.Reinterpret<int>().AsNativeArray();
                for(int i = 0; i < expiredArr.Length; ++i)
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
            }).ScheduleParallel();
        }
    }

    [BurstCompile]
    struct EmissionJob : IJobChunk
    {
        [ReadOnly]
        public ComponentTypeHandle<Particle2DCountMax> Particle2DCountMaxHandle;
        [ReadOnly]
        public ComponentTypeHandle<Particle2DEmissionRate> Particle2DEmissionRateHandle;
        [ReadOnly]
        public ComponentTypeHandle<Particle2DNewParticleLifetime> Particle2DNewParticleLifetimeHandle;

        public ComponentTypeHandle<Particle2DCountAlive> Particle2DCountAliveHandle;
        public ComponentTypeHandle<Particle2DEmissionTimer> Particle2DEmissionTimerHandle;

        public BufferTypeHandle<Particle2DPosition> Particle2DPositionHandle;
        public BufferTypeHandle<Particle2DAcceleration> Particle2DAccelerationHandle;
        public BufferTypeHandle<Particle2DVelocity> Particle2DVelocityHandle;
        public BufferTypeHandle<Particle2DColor> Particle2DColorHandle;
        public BufferTypeHandle<Particle2DLifetime> Particle2DLifetimeHandle;
        public BufferTypeHandle<Particle2DGlyph> Particle2DGlyphHandle;

        public float dt;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var aliveCountArr = chunk.GetNativeArray(Particle2DCountAliveHandle);
            var timerArr = chunk.GetNativeArray(Particle2DEmissionTimerHandle);

            var maxArr = chunk.GetNativeArray(Particle2DCountMaxHandle);
            var newLifetimeArr = chunk.GetNativeArray(Particle2DNewParticleLifetimeHandle);
            var emissionRateArr = chunk.GetNativeArray(Particle2DEmissionRateHandle);

            var posAccess = chunk.GetBufferAccessor(Particle2DPositionHandle);
            var accelAccess = chunk.GetBufferAccessor(Particle2DAccelerationHandle);
            var velAccess = chunk.GetBufferAccessor(Particle2DVelocityHandle);
            var colAccess = chunk.GetBufferAccessor(Particle2DColorHandle);
            var lifetimeAccess = chunk.GetBufferAccessor(Particle2DLifetimeHandle);
            var glyphAccess = chunk.GetBufferAccessor(Particle2DGlyphHandle);

            for(int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
            {
                int alive = aliveCountArr[entityIndex];
                int max = maxArr[entityIndex];

                if (alive >= max)
                    continue;

                float timer = timerArr[entityIndex];
                float rate = emissionRateArr[entityIndex];
                float newLifetime = newLifetimeArr[entityIndex];

                timer += dt;
                float delay = 1f / rate;

                while( timer >= delay && alive < max)
                {
                    timer -= delay;
                    alive++;

                    Add(posAccess, entityIndex);
                    Add(accelAccess, entityIndex);
                    Add(velAccess, entityIndex);
                    Add(colAccess, entityIndex);
                    Add(lifetimeAccess, entityIndex, new Particle2DLifetime
                    {
                        remaining = newLifetime,
                        total = newLifetime
                    });
                    Add(glyphAccess, entityIndex);
                }

                timerArr[entityIndex] = timer;
                aliveCountArr[entityIndex] = alive;
            }
        }

        static void Add<T>(BufferAccessor<T> accessor, int index, T value = default)
            where T : unmanaged, IBufferElementData
        {
            accessor[index].Add(value);
        }
    }
}


