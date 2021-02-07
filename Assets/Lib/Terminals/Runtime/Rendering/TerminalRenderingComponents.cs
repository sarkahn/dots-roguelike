using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;

namespace Sark.Terminals
{
    public struct TerminalRenderer
    {
        public static void AddToEntity(EntityManager em, Entity e)
        {
            em.AddBuffer<TerminalMeshTileDataBuffer>(e);
            em.AddBuffer<TerminalMeshIndexBuffer>(e);
            em.AddBuffer<TerminalMeshVertsBuffer>(e);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VertTileData
    {
        public float2 UV;
        public float4 FGColor;
        public float4 BGColor;
    }

    public struct TerminalMeshVertsBuffer : IBufferElementData
    {
        public float3 Value;
        public static implicit operator float3(TerminalMeshVertsBuffer b) => b.Value;
        public static implicit operator TerminalMeshVertsBuffer(float3 v) =>
            new TerminalMeshVertsBuffer { Value = v };
    }

    public struct TerminalMeshIndexBuffer : IBufferElementData
    {
        public ushort Value;
        public static implicit operator ushort(TerminalMeshIndexBuffer b) => b.Value;
        public static implicit operator TerminalMeshIndexBuffer(ushort v) =>
            new TerminalMeshIndexBuffer { Value = v };
    }

    [InternalBufferCapacity(10)]
    public struct TerminalMeshTileDataBuffer : IBufferElementData
    {
        public VertTileData Value;
        public static implicit operator VertTileData(TerminalMeshTileDataBuffer b) => b.Value;
        public static implicit operator TerminalMeshTileDataBuffer(VertTileData v) =>
            new TerminalMeshTileDataBuffer { Value = v };
    }

}