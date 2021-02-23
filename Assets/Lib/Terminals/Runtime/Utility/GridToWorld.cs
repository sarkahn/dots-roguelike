using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Sark.Terminals
{
    public struct GridToWorld : IComponentData
    {
        // Assumes world position is in center, grid position [0,0] is bottom left
        public float3 worldPosition;
        public int2 gridSize;
        public float2 cellSize;
        public float2 HalfSize => (float2)gridSize / 2f;
        public float3 WorldCenter => worldPosition;
        public int2 Center => gridSize / 2;

        public int2 WorldToGridPosition(float3 worldPos)
        {
            float2 xy = worldPos.xy - worldPosition.xy;
            xy += HalfSize;
            return (int2)math.floor(xy);
        }

        public float3 GridToWorldPosition(int2 xy)
        {
            float3 p = worldPosition;
            float2 gridPos = (float2)xy - HalfSize;
            p.xy += gridPos;
            return p;
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(TerminalResizeSystem))]
    public class TerminalGridToWorldSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithChangeFilter<TerminalSize>()
                .WithChangeFilter<Translation>()
                .ForEach((
                    ref GridToWorld gridToWorld,
                    in TerminalSize size,
                    in Translation translation,
                    in TileSize tileSize) =>
                {
                    gridToWorld.gridSize = size;
                    gridToWorld.worldPosition = translation.Value;
                    gridToWorld.cellSize = tileSize;
                }).Schedule();
        }
    }
}
