using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using Sark.Common.GridUtil;

namespace Sark.Terminals
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(TerminalResizeSystem))]
    public class TerminalRenderInitSystem : SystemBase
    {

        protected override void OnUpdate()
        {
        Entities
            .WithName("ResizeTerminalMeshData")
            .WithChangeFilter<TerminalSize>()
            .ForEach((
                ref DynamicBuffer<TerminalMeshVertsBuffer> vertsBuffer,
                ref DynamicBuffer<TerminalMeshIndexBuffer> indexBuffer,
                ref DynamicBuffer<TerminalMeshTileDataBuffer> tileDataBuffer,
                in TerminalSize size,
                in TileSize tileSize) =>
            {
                Debug.Log("TerminalBuffer Size Change");
                int tileCount = size.Length;
                vertsBuffer.ResizeUninitialized(tileCount * 4);
                indexBuffer.ResizeUninitialized(tileCount * 6);
                tileDataBuffer.ResizeUninitialized(tileCount * 4);

                var verts = vertsBuffer.Reinterpret<float3>().AsNativeArray();
                var indices = indexBuffer.Reinterpret<ushort>().AsNativeArray();
                var tileData = tileDataBuffer.Reinterpret<VertTileData>().AsNativeArray();

                float3 worldSize = new float3(size.Value * tileSize.Value, 0);
                float3 start = -worldSize / 2f;

                float3 right = new float3(tileSize.Width, 0, 0);
                float3 up = new float3(0, tileSize.Height, 0);

                for (int tileIndex = 0; tileIndex < tileCount; ++tileIndex)
                {
                    int vi = tileIndex * 4;
                    int ii = tileIndex * 6;

                    float2 xy = Grid2D.IndexToPos(tileIndex, size.Width);
                    xy *= tileSize;

                    float3 p = new float3(xy, 0);
                    // 0---1
                    // | / | 
                    // 2---3
                    verts[vi + 0] = start + p + up;
                    verts[vi + 1] = start + p + right + up;
                    verts[vi + 2] = start + p;
                    verts[vi + 3] = start + p + right;

                    indices[ii + 0] = (ushort)(vi + 0);
                    indices[ii + 1] = (ushort)(vi + 1);
                    indices[ii + 2] = (ushort)(vi + 2);
                    indices[ii + 3] = (ushort)(vi + 3);
                    indices[ii + 4] = (ushort)(vi + 2);
                    indices[ii + 5] = (ushort)(vi + 1);
                }

                for (int i = 0; i < tileData.Length; ++i)
                {
                    tileData[i] = default;
                }
            }).Schedule();
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class TerminalUpdateRenderDataSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
           .WithName("UpdateTerminalRenderDataFromTileData")
           .WithChangeFilter<TerminalTilesBuffer>()
           .ForEach((
               ref DynamicBuffer<TerminalMeshTileDataBuffer> vertDataBuffer,
               in DynamicBuffer<TerminalTilesBuffer> tilesBuffer) =>
           {
               //Debug.Log("Updating tile data");
               float2 uvSize = 1f / 16f;
               float2 uvRight = new float2(uvSize.x, 0);
               float2 uvUp = new float2(0, uvSize.y);

               var vertData = vertDataBuffer.Reinterpret<VertTileData>().AsNativeArray();
               var tiles = tilesBuffer.Reinterpret<TerminalTile>().AsNativeArray();

               for (int tileIndex = 0; tileIndex < tiles.Length; ++tileIndex)
               {
                   var tile = tiles[tileIndex];

                   int vi = tileIndex * 4; // Vert Index

                            int glyph = tile.Glyph;

                            // UVs
                            int2 glyphIndex = new int2(
                       glyph % 16,
                       // Y is flipped on the spritesheet
                       16 - 1 - (glyph / 16));
                   float2 uvOrigin = (float2)glyphIndex * uvSize;

                   float4 fg = FromColor(tile.FGColor);
                   float4 bg = FromColor(tile.BGColor);

                   vertData[vi + 0] = new VertTileData
                   {
                       UV = uvOrigin + uvUp,
                       FGColor = fg,
                       BGColor = bg
                   };
                   vertData[vi + 1] = new VertTileData
                   {
                       UV = uvOrigin + uvRight + uvUp,
                       FGColor = fg,
                       BGColor = bg
                   };
                   vertData[vi + 2] = new VertTileData
                   {
                       UV = uvOrigin,
                       FGColor = fg,
                       BGColor = bg
                   };
                   vertData[vi + 3] = new VertTileData
                   {
                       UV = uvOrigin + uvRight,
                       FGColor = fg,
                       BGColor = bg
                   };
               }
           }).Schedule();
        }

        static float4 FromColor(Color c) =>
            new float4(c.r, c.g, c.b, c.a);
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class TerminalRenderSystem : SystemBase
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
            .WithName("UpdateTerminalMeshVerts")
            .WithoutBurst()
            .WithChangeFilter<TerminalMeshVertsBuffer>()
            .ForEach((Entity e,
                MeshFilter filter,
                in DynamicBuffer<TerminalMeshVertsBuffer> meshVertsBuffer,
                in DynamicBuffer<TerminalMeshIndexBuffer> meshIndexBuffer,
                in TerminalSize size) =>
            {
                Debug.Log("Rebuilding Mesh Verts");
                int tileCount = size.Length;
                if (filter == null)
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
            .WithName("UpdateTerminalRendererTileData")
            .WithChangeFilter<TerminalMeshTileDataBuffer>()
            .ForEach((Entity e, MeshFilter filter,
            in DynamicBuffer<TerminalMeshTileDataBuffer> tileDataBuffer) =>
            {
                //Debug.Log("Updating mesh data");
                if (filter == null)
                    return;

                var tileData = tileDataBuffer.Reinterpret<VertTileData>().AsNativeArray();
                var mesh = filter.sharedMesh;
                mesh.SetVertexBufferData(tileData, 0, 0, tileData.Length, 1, Flags);
            }).Run();
        }
    }
}
