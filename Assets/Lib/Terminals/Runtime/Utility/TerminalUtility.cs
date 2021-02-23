using Unity.Mathematics;

using Sark.Common;
using Unity.Entities;
using Unity.Transforms;

namespace Sark.Terminals
{
    public static class TerminalUtility
    {
        public static TerminalBuilder MakeTerminal()
        {
            return MakeTerminal(World.DefaultGameObjectInjectionWorld.EntityManager);
        }
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

        public static TerminalAccessor GetTerminalAccessor(EntityManager em, Entity termEntity)
        {
            var size = em.GetComponentData<TerminalSize>(termEntity);
            var tiles = em.GetBuffer<TerminalTilesBuffer>(termEntity);
            return new TerminalAccessor(tiles, size);
        }

        public static readonly ComponentTypes TerminalComponents = 
            new ComponentTypes(new ComponentType[]
        {
            ComponentType.ReadOnly<Terminal>(),
            ComponentType.ReadOnly<TerminalSize>(),
            ComponentType.ReadOnly<TileSize>(),
            ComponentType.ReadOnly<TerminalTilesBuffer>(),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<GridToWorld>()
        });
    } 
}
