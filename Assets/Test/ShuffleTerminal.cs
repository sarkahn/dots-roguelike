using Unity.Entities;
using UnityEngine;

using Sark.Terminals;

public struct ShuffleTerminalOnClick : IComponentData
{}

public class ShuffleTerminal : SystemBase
{
    protected override void OnUpdate()
    {
        if (!Input.GetMouseButtonDown(0))
            return;
        var rand = new Unity.Mathematics.Random((uint)Random.Range(1, int.MaxValue));
        Entities
            .WithAll<ShuffleTerminalOnClick>()
            .ForEach((ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer) =>
            {
                var tiles = tilesBuffer.Reinterpret<TerminalTile>().AsNativeArray();
                for(int i = 0; i < tiles.Length; ++i)
                {
                    var t = tiles[i];
                    t.Glyph = (byte)rand.NextInt(0, 256);
                    tiles[i] = t;
                }
            }).Schedule();
    }
}
