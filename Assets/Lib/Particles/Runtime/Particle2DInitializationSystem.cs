using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Sark.Particles2D
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class Particle2DInitializationSystem : SystemBase
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
                Particle2DInitializedHandle = GetBufferTypeHandle<Particle2DInitializedIndex>(false)
            }.ScheduleParallel(_emitterQuery, Dependency);

            // Initialize positions based on emitter position
            Entities.ForEach((ref DynamicBuffer<Particle2DPosition> posBuffer,
                in DynamicBuffer<Particle2DInitializedIndex> initIndicesBuffer,
                in Translation translation) =>
            {
                var initIndicesArr = initIndicesBuffer.AsNativeArray();

                for(int indicesIndex = 0; indicesIndex < initIndicesBuffer.Length; ++indicesIndex)
                {
                    int i = initIndicesArr[indicesIndex];
                    posBuffer[i] += translation.Value.xy;
                }
            }).ScheduleParallel();

            Entities.ForEach((int entityInQueryIndex,
                ref DynamicBuffer<Particle2DExpiredIndex> expiredIndicesBuffer,
                ref DynamicBuffer<Particle2DLifetime> lifeBuffer) =>
            {
                var arr = lifeBuffer.AsNativeArray();
                for (int i = 0; i < arr.Length; ++i)
                {
                    var lifetime = arr[i];

                    lifetime.remaining = math.max(0, lifetime.remaining - dt);

                    if (lifetime.remaining == 0)
                    {
                        expiredIndicesBuffer.Add(i);
                    }

                    arr[i] = lifetime;
                }
            }).ScheduleParallel();
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

            public BufferTypeHandle<Particle2DInitializedIndex> Particle2DInitializedHandle;

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

                var newParticleIndexAccess = chunk.GetBufferAccessor(Particle2DInitializedHandle);

                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
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

                    while (timer >= delay && alive < max)
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

                        newParticleIndexAccess[entityIndex].Add(alive - 1);
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
}