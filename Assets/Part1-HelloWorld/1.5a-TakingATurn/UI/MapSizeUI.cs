using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace RLTKTutorial.Part1_5A
{
    public class MapSizeUI : MonoBehaviour
    {
        [SerializeField]
        Text _text = default;

        private void Update()
        {
            var size = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MapSizeSystem>().MapSize;
            _text.text = $"{size.x}, {size.y}";
        }

        [DisableAutoCreation]
        internal class MapSizeSystem : SystemBase
        {
            EntityQuery _mapQuery = default;

            public int2 MapSize => GetSingleton<MapData>().Size;

            protected override void OnCreate() => _mapQuery = GetEntityQuery(ComponentType.ReadOnly<MapData>());

            protected override void OnUpdate()
            {}
        }
    } 
}
