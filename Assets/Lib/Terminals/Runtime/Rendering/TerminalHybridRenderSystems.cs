using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Rendering;

namespace Sark.Terminals
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(TerminalRenderInitSystem))]
    public class TerminalUpdateRenderMeshBoundsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithName("UpdateRenderMeshBounds")
                .WithChangeFilter<TerminalSize>()
                .ForEach((
                    ref RenderBounds bounds,
                    in TerminalSize size,
                    in TileSize tileSize) =>
                {
                    float2 halfSize = (size.Value * tileSize.Value) / 2;
                    var aabb = new AABB
                    {
                        Center = new float3(halfSize, 0),
                        Extents = new float3(halfSize, 2)
                    };
                    bounds.Value = aabb;
                }).Schedule();
        }
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class TerminalRenderMeshSystem : SystemBase
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
            .WithName("UpdateTerminalRenderMeshVerts")
            .WithStructuralChanges()
            .WithChangeFilter<TerminalMeshVertsBuffer>()
            .ForEach((Entity e,
                RenderMesh renderMesh,
                in DynamicBuffer<TerminalMeshVertsBuffer> meshVertsBuffer,
                in DynamicBuffer<TerminalMeshIndexBuffer> meshIndexBuffer,
                in TerminalSize size) =>
            {
                if (renderMesh.mesh == null)
                {
                    var newMesh = new Mesh();
                    newMesh.MarkDynamic();
                    renderMesh.mesh = newMesh;
                }

                var mesh = renderMesh.mesh;

                int tileCount = size.Length;

                var verts = meshVertsBuffer.Reinterpret<float3>().AsNativeArray();
                var indices = meshIndexBuffer.Reinterpret<ushort>().AsNativeArray();

                mesh.Clear();
                mesh.SetVertexBufferParams(4 * tileCount, Descriptors);
                mesh.SetIndexBufferParams(6 * tileCount, IndexFormat.UInt16);

                mesh.SetVertexBufferData(verts, 0, 0, verts.Length, 0, Flags);
                mesh.SetIndexBufferData(indices, 0, 0, indices.Length, Flags);

                mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length), Flags);

                mesh.RecalculateBounds();

                renderMesh.mesh = mesh;
                EntityManager.SetSharedComponentData(e, renderMesh);
            }).Run();

            Entities
            .WithStructuralChanges()
            .WithName("UpdateRenderMeshTileData")
            .WithChangeFilter<TerminalMeshTileDataBuffer>()
            .ForEach((Entity e, RenderMesh renderMesh,
            in DynamicBuffer<TerminalMeshTileDataBuffer> tileDataBuffer) =>
            {
                Debug.Log("Updating rendermesh data");

                var tileData = tileDataBuffer.Reinterpret<VertTileData>().AsNativeArray();
                var mesh = renderMesh.mesh;
                mesh.SetVertexBufferData(tileData, 0, 0, tileData.Length, 1, Flags);
                EntityManager.SetSharedComponentData(e, renderMesh);
            }).Run();
        }
    }
}
