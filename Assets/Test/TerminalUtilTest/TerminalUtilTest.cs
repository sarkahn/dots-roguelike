using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

using Sark.Terminals;

public class TerminalUtilTest : MonoBehaviour
{
    EntityQuery eq;
    EntityManager em;

    private void OnEnable()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        eq = em.CreateEntityQuery(
            typeof(TerminalSize), 
            typeof(TileSize), 
            typeof(Translation));
    }

    private void OnGUI()
    {
        float3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        var pos = eq.GetSingleton<Translation>().Value;
        var tileSize = eq.GetSingleton<TileSize>().Value;
        var size = eq.GetSingleton<TerminalSize>().Value;

        int2 index = TerminalUtility.WorldToTileIndex(mousePos, pos, size, tileSize);

        GUILayout.Label($"Terminal Index: {index.x}, {index.y}");
    }
}
