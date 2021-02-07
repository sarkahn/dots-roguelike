using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;

namespace Sark.Terminals
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public class TerminalResizeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
            .WithChangeFilter<TerminalSize>()
            .WithName("ResizeTerminal")
            .ForEach((Entity e,
            ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer,
            in TerminalSize termSize) =>
            {
                int len = termSize.Length;
                Debug.Log("Terminal Resized");
                tilesBuffer.ResizeUninitialized(len);
                var tiles = tilesBuffer.Reinterpret<TerminalTile>().AsNativeArray();
                for (int i = 0; i < tiles.Length; ++i)
                {
                    tiles[i] = new TerminalTile
                    {
                        FGColor = Color.white,
                        BGColor = Color.black,
                        Glyph = 1
                    };
                }
            }).Schedule();
        }
    }
}
