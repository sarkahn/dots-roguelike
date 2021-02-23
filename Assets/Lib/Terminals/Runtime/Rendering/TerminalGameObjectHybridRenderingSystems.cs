using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sark.Terminals.Rendering
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class TerminalGameObjectHybridRenderSystem : SystemBase
    {
        public static readonly VertexAttributeDescriptor[] Descriptors = new[]
        {
            // Positions
            new VertexAttributeDescriptor(
                VertexAttribute.Position,
                VertexAttributeFormat.Float32,
                3, 0),
            // UVs
            new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0,
                VertexAttributeFormat.Float32,
                2, 1),
            // Foreground Colors
            new VertexAttributeDescriptor(
                VertexAttribute.TexCoord1,
                VertexAttributeFormat.Float32,
                4, 1),
            // Background Colors
            new VertexAttributeDescriptor(
                VertexAttribute.TexCoord2,
                VertexAttributeFormat.Float32,
                4, 1),
        };

        static readonly MeshUpdateFlags Flags = MeshUpdateFlags.DontValidateIndices |
        MeshUpdateFlags.DontNotifyMeshUsers |
        MeshUpdateFlags.DontRecalculateBounds |
        MeshUpdateFlags.DontResetBoneBounds;

        protected override void OnUpdate()
        {

            Entities
            .WithName("TerminalGOHybridUpdateVerts")
            .WithoutBurst()
            .WithChangeFilter<TerminalMeshDataVertices>()
            .ForEach((Entity e,
                MeshFilter filter,
                in DynamicBuffer<TerminalMeshDataVertices> meshVertsBuffer,
                in DynamicBuffer<TerminalMeshDataIndices> meshIndexBuffer,
                in TerminalSize size) =>
            {
                //Debug.Log("Update Terminal MeshData verts");
                int tileCount = size.Length;
                if (filter == null || meshVertsBuffer.Length == 0)
                    return;

                var verts = meshVertsBuffer.Reinterpret<float3>().AsNativeArray();
                var indices = meshIndexBuffer.Reinterpret<ushort>().AsNativeArray();
                var mesh = filter.sharedMesh;

                mesh.Clear();
                mesh.SetVertexBufferParams(4 * tileCount, Descriptors);
                mesh.SetIndexBufferParams(6 * tileCount, IndexFormat.UInt16);

                mesh.SetVertexBufferData(verts, 0, 0, verts.Length, 0, Flags);
                mesh.SetIndexBufferData(indices, 0, 0, indices.Length, Flags);

                mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length), Flags);

                mesh.RecalculateBounds();
            }).Run();

            Entities
            .WithoutBurst()
            .WithName("TerminalGOHybridUpdateTiles")
            .WithChangeFilter<TerminalMeshDataTiles>()
            .ForEach((Entity e, MeshFilter filter,
            in DynamicBuffer<TerminalMeshDataTiles> tileDataBuffer) =>
            {
                if (filter == null || tileDataBuffer.Length == 0)
                    return;
                //Debug.Log("Updating terminal mesh data (rendering)");

                var tileData = tileDataBuffer.Reinterpret<VertTileData>().AsNativeArray();
                var mesh = filter.sharedMesh;
                mesh.SetVertexBufferData(tileData, 0, 0, tileData.Length, 1, Flags);
            }).Run();

        }
    }
}