using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;

namespace Sark.Terminals.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertTileData
    {
        public float2 UV;
        public float4 FGColor;
        public float4 BGColor;
    }

    public struct TerminalMeshDataVertices : IBufferElementData
    {
        public float3 Value;
        public static implicit operator float3(TerminalMeshDataVertices b) => b.Value;
        public static implicit operator TerminalMeshDataVertices(float3 v) =>
            new TerminalMeshDataVertices { Value = v };
    }

    public struct TerminalMeshDataIndices : IBufferElementData
    {
        public ushort Value;
        public static implicit operator ushort(TerminalMeshDataIndices b) => b.Value;
        public static implicit operator TerminalMeshDataIndices(ushort v) =>
            new TerminalMeshDataIndices { Value = v };
    }

    [InternalBufferCapacity(10)]
    public struct TerminalMeshDataTiles : IBufferElementData
    {
        public VertTileData Value;
        public static implicit operator VertTileData(TerminalMeshDataTiles b) => b.Value;
        public static implicit operator TerminalMeshDataTiles(VertTileData v) =>
            new TerminalMeshDataTiles { Value = v };
    }

}