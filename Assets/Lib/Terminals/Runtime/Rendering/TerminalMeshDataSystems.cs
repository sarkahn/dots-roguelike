using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;

using Sark.Common.GridUtil;

using Debug = UnityEngine.Debug;
using Color = UnityEngine.Color;

namespace Sark.Terminals.Rendering
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(TerminalResizeSystem))]
    public class TerminalMeshDataResizeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
        Entities
            .WithName("TerminalResizeMeshData")
            .WithChangeFilter<TerminalSize>()
            .ForEach((
                ref DynamicBuffer<TerminalMeshDataVertices> vertsBuffer,
                ref DynamicBuffer<TerminalMeshDataIndices> indexBuffer,
                ref DynamicBuffer<TerminalMeshDataTiles> tileDataBuffer,
                in TerminalSize size,
                in TileSize tileSize) =>
            {
                //Debug.Log("Resize Terminal MeshData");
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

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public class TerminalUpdateMeshDataTilesSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
           .WithName("TerminalUpdateMeshDataTiles")
           .WithChangeFilter<TerminalTilesBuffer>()
           .ForEach((
               ref DynamicBuffer<TerminalMeshDataTiles> vertDataBuffer,
               in DynamicBuffer<TerminalTilesBuffer> tilesBuffer) =>
           {
               //Debug.Log("Updating terminal MeshData tiles");
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
}
