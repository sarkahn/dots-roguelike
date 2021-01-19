using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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

public struct ParticleTimeAlivePercent : IBufferElementData
{
    public float value;
    public static implicit operator float(ParticleTimeAlivePercent b) => b.value;
    public static implicit operator ParticleTimeAlivePercent(float v) =>
        new ParticleTimeAlivePercent { value = v };
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

[InternalBufferCapacity(10)]
public struct ParticleBackgroundColorBuffer : IBufferElementData
{
    public Color value;
    public static implicit operator Color(ParticleBackgroundColorBuffer b) => b.value;
    public static implicit operator ParticleBackgroundColorBuffer(Color v) =>
        new ParticleBackgroundColorBuffer { value = v };
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class InitializeTerminalParticleSystem : SystemBase
{
    protected override void OnUpdate()
    {
    }
}

public class TerminalParticleSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dt = Time.DeltaTime;
        Entities
        .ForEach((ref DynamicBuffer <ParticlePositionBuffer> posBuffer,
            in DynamicBuffer <ParticleVelocityBuffer> velBuffer) =>
        {
            var posArr = posBuffer.Reinterpret<float2>().AsNativeArray();
            var velArr = posBuffer.Reinterpret<float2>().AsNativeArray();
            for(int i = 0; i < posBuffer.Length; ++i)
            {
                posArr[i] += velArr[i] * dt;
            }
        }).ScheduleParallel();
    }
}

public class TerminalParticleShapeSystem : SystemBase
{
    protected override void OnUpdate()
    {
    }
}