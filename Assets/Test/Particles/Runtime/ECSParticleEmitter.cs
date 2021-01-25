using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Sark.Particles
{
    public struct ParticlePositionBuffer : IBufferElementData
    {
        public float2 value;
        public static implicit operator float2(ParticlePositionBuffer b) => b.value;
        public static implicit operator ParticlePositionBuffer(float2 v) =>
            new ParticlePositionBuffer { value = v };
    }

    public struct ParticleVelocityBuffer : IBufferElementData
    {
        public float2 value;
        public static implicit operator float2(ParticleVelocityBuffer b) => b.value;
        public static implicit operator ParticleVelocityBuffer(float2 v) =>
            new ParticleVelocityBuffer { value = v };
    }

    public struct ParticleLifetimeBuffer : IBufferElementData
    {
        public float value;
        public static implicit operator float(ParticleLifetimeBuffer b) => b.value;
        public static implicit operator ParticleLifetimeBuffer(float v) =>
            new ParticleLifetimeBuffer { value = v };
    }

    public struct ParticleAgeBuffer : IBufferElementData
    {
        public float value;
        public static implicit operator float(ParticleAgeBuffer b) => b.value;
        public static implicit operator ParticleAgeBuffer(float v) =>
            new ParticleAgeBuffer { value = v };
    }

    public struct ParticleGlyphBuffer : IBufferElementData
    {
        public byte value;
        public static implicit operator byte(ParticleGlyphBuffer b) => b.value;
        public static implicit operator ParticleGlyphBuffer(byte v) =>
            new ParticleGlyphBuffer { value = v };
    }

    public struct ParticleForegroundColorBuffer : IBufferElementData
    {
        public Color value;
        public static implicit operator Color(ParticleForegroundColorBuffer b) => b.value;
        public static implicit operator ParticleForegroundColorBuffer(Color v) =>
            new ParticleForegroundColorBuffer { value = v };
    }

    public struct ParticleBackgroundColorBuffer : IBufferElementData
    {
        public Color value;
        public static implicit operator Color(ParticleBackgroundColorBuffer b) => b.value;
        public static implicit operator ParticleBackgroundColorBuffer(Color v) =>
            new ParticleBackgroundColorBuffer { value = v };
    }

    public struct ParticleLifetime : IComponentData
    {
        public float value;
        public static implicit operator float(ParticleLifetime b) => b.value;
        public static implicit operator ParticleLifetime(float v) =>
            new ParticleLifetime { value = v };
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializeTerminalParticleSystem : SystemBase
    {
        protected override void OnUpdate()
        {
        }
    }

    public class UpdateParticlesSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;

            Entities
            .ForEach((ref DynamicBuffer<ParticlePositionBuffer> posBuffer,
                in DynamicBuffer<ParticleVelocityBuffer> velBuffer) =>
            {
                var posArr = posBuffer.Reinterpret<float2>().AsNativeArray();
                var velArr = posBuffer.Reinterpret<float2>().AsNativeArray();

                for (int i = 0; i < posBuffer.Length; ++i)
                {
                    posArr[i] += velArr[i] * dt;
                }

                
            }).ScheduleParallel();
        }
    }

    public class ParticleLifetimeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float dt = Time.DeltaTime;

            Entities.ForEach((ref DynamicBuffer<ParticleAgeBuffer> ageBuffer,
            in ParticleLifetime lifetime) =>
            {
                var ageArr = ageBuffer.Reinterpret<float>().AsNativeArray();
                for (int i = ageArr.Length - 1; i >= 0; --i)
                {
                    if(ageArr[i] >= lifetime)
                    {
                        // Do we need to ensure ordering...?
                        ageBuffer.RemoveAtSwapBack(i);
                    }
                }
            }).ScheduleParallel();

            Entities.ForEach((ref DynamicBuffer<ParticleAgeBuffer> ageBuffer,
            in ParticleLifetime lifetime) =>
            {
                var ageArr = ageBuffer.Reinterpret<float>().AsNativeArray();
                for(int i = 0; i < ageArr.Length; ++i)
                {
                    ageArr[i] += dt;
                }
            }).ScheduleParallel();
        }
    }

    //public struct SyncBufferJob<T> : IJobChunk
    //    where T : unmanaged, IBufferElementData
    //{
    //    public BufferTypeHandle<T> bufferHandle;

    //    [ReadOnly]
    //    public BufferTypeHandle<ParticleAgeBuffer> ageHandle;

    //    public float particleLifetime;

    //    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
    //    {
    //        var tAccessor = chunk.GetBufferAccessor(bufferHandle);
    //        var ageAccessor = chunk.GetBufferAccessor(ageHandle);

    //        for(int i = 0; i < chunk.Count; ++i)
    //        {
    //            var ageArr = ageAccessor[i].Reinterpret<float>().AsNativeArray();
    //            var tBuffer = tAccessor[i];

    //            for(int j = ageArr.Length; j >= 0; --j)
    //            {
    //                var arr = buffer
    //                if (buffer[j] >= particleLifetime)
    //                    buffer.RemoveAtSwapBack(j);
    //            }
    //        }
    //    }
    //}

    public class CleanupParticlesSystem : SystemBase
    {
        protected override void OnUpdate()
        {

        }
    }

    public class TerminalParticleShapeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
        }
    } 
}