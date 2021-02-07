using Unity.Mathematics;

using Sark.Common;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;

namespace Sark.Terminals
{
    public static class TerminalUtility
    {
        public static TerminalBuilder MakeTerminal(EntityManager em)
        {
            var e = em.CreateEntity();
            return AddComponents(em, e);
        }

        public static TerminalBuilder AddComponents(EntityManager em, Entity e)
        {
            em.AddComponents(e, TerminalComponents);
            em.SetComponentData<TileSize>(e, new float2(1, 1));
            em.SetComponentData<TerminalSize>(e, new int2(10, 10));

            var mesh = new Mesh();
            mesh.MarkDynamic();
            var mat = Resources.Load<Material>("Terminal8x8");
            var desc = new RenderMeshDescription(mesh, mat);

            RenderMeshUtility.AddComponents(e, em, desc);

            em.AddBuffer<TerminalMeshTileDataBuffer>(e);
            em.AddBuffer<TerminalMeshIndexBuffer>(e);
            em.AddBuffer<TerminalMeshVertsBuffer>(e);

#if UNITY_EDITOR
            em.SetName(e, "Terminal");
#endif
            return new TerminalBuilder(em, e);
        }

        public static int2 WorldToTileIndex(float3 worldPos,
            float3 terminalPos,
            int2 terminalSize,
            float2 tileSize = default)
        {
            if (tileSize.Equals(0))
                tileSize = new float2(1, 1);

            float3 local = (worldPos - terminalPos);
            local.xy /= tileSize;
            local.xy += terminalSize / 2;

            return (int2)math.floor(local.xy);
        }

        public static float3 PositionSnap(float3 worldPos,
            int2 terminalSize,
            float2 tileSize)
        {
            float2 size = terminalSize * tileSize;
            float2 halfSize = size / 2;
            worldPos.xy -= halfSize;
            worldPos.xy = MathUtil.roundedincrement(worldPos.xy, tileSize);
            worldPos.xy += halfSize;
            return worldPos;
        }

        public static readonly ComponentTypes TerminalComponents = 
            new ComponentTypes(new ComponentType[]
        {
            ComponentType.ReadOnly<Terminal>(),
            ComponentType.ReadOnly<TerminalSize>(),
            ComponentType.ReadOnly<TileSize>(),
            ComponentType.ReadOnly<TerminalTilesBuffer>(),
            ComponentType.ReadOnly<Translation>()
        });
    } 
}
