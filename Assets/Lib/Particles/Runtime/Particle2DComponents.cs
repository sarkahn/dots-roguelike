using Unity.Entities;
using Unity.Mathematics;

using Color32 = UnityEngine.Color32;

namespace Sark.Particles2D
{
    public struct Particle2DEmitter : IComponentData
    { }


    public struct Particle2DCountAlive : IComponentData
    {
        public int value;
        public static implicit operator int(Particle2DCountAlive b) => b.value;
        public static implicit operator Particle2DCountAlive(int v) =>
            new Particle2DCountAlive { value = v };
    }

    public struct Particle2DCountMax : IComponentData
    {
        public int value;
        public static implicit operator int(Particle2DCountMax b) => b.value;
        public static implicit operator Particle2DCountMax(int v) =>
            new Particle2DCountMax { value = v };
    }

    public struct Particle2DPosition : IBufferElementData
    {
        public float2 value;
        public static implicit operator float2(Particle2DPosition b) => b.value;
        public static implicit operator Particle2DPosition(float2 v) =>
            new Particle2DPosition { value = v };
    }

    public struct Particle2DColor : IBufferElementData
    {
        public Color32 value;
        public static implicit operator Color32(Particle2DColor b) => b.value;
        public static implicit operator Particle2DColor(Color32 v) =>
            new Particle2DColor { value = v };
    }

    public struct Particle2DVelocity : IBufferElementData
    {
        public float2 value;
        public static implicit operator float2(Particle2DVelocity b) => b.value;
        public static implicit operator Particle2DVelocity(float2 v) =>
            new Particle2DVelocity { value = v };
    }

    public struct Particle2DAcceleration : IBufferElementData
    {
        public float2 value;
        public static implicit operator float2(Particle2DAcceleration b) => b.value;
        public static implicit operator Particle2DAcceleration(float2 v) =>
            new Particle2DAcceleration { value = v };
    }

    public struct Particle2DGlyph : IBufferElementData
    {
        public byte value;
        public static implicit operator byte(Particle2DGlyph b) => b.value;
        public static implicit operator Particle2DGlyph(byte v) =>
            new Particle2DGlyph { value = v };
    }

    public struct Particle2DLifetime : IBufferElementData
    {
        public float value;
        public static implicit operator float(Particle2DLifetime b) => b.value;
        public static implicit operator Particle2DLifetime(float v) =>
            new Particle2DLifetime { value = v };
    } 
}