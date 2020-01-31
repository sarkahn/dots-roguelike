
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace RLTKTutorial.Part1_2
{
    public class MapSizeControls : MonoBehaviour
    {
        [SerializeField]
        Text _widthText = null;

        [SerializeField]
        Text _heightText = null;

        [SerializeField]
        Text _iterationsText = null;

        [SerializeField]
        Slider _widthSlider = null;

        [SerializeField]
        Slider _heightSlider = null;

        [SerializeField]
        Slider _iterationsSlider = null;


        Entity _mapEntity = default;

        private void Start()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var q = em.CreateEntityQuery(ComponentType.ReadOnly<MapData>());
            _mapEntity = q.GetSingletonEntity();

            var mapSize = GetMapSize();
            _widthSlider.value = mapSize.x;
            _heightSlider.value = mapSize.y;
            _iterationsSlider.value = 150;
            _iterationsSlider.onValueChanged.AddListener((f) => GenerateMap());
            UpdateText(mapSize);
        }

        int2 GetMapSize()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var mapData = em.GetComponentData<MapData>(_mapEntity);
            return new int2(mapData.width, mapData.height);
        }

        void SetMapSize(int2 value)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var mapData = em.GetComponentData<MapData>(_mapEntity);
            mapData.width = value.x;
            mapData.height = value.y;
            em.SetComponentData(_mapEntity, mapData);
        }

        private void Update()
        {
            var mapSize = GetMapSize();

            int2 newSize = new int2((int)_widthSlider.value, (int)_heightSlider.value);

            if (mapSize.x != newSize.x || mapSize.y != newSize.y)
            {
                GenerateMap(newSize);
            }

            UpdateText(newSize);
        }

        void GenerateMap()
        {
            GenerateMap(new int2((int)_widthSlider.value, (int)_heightSlider.value));
        }

        void GenerateMap(int2 size)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var gen = new GenerateMap
            {
                iterationCount = (int)_iterationsSlider.value,
                playerPos = new int2(
                    UnityEngine.Random.Range(1, size.x - 1),
                    UnityEngine.Random.Range(1, size.y - 1)),
                seed = UnityEngine.Random.Range(1, int.MaxValue)
            };
            em.AddComponentData(_mapEntity, gen);
            _widthSlider.value = size.x;
            _heightSlider.value = size.y;

            SetMapSize(size);
        }

        void UpdateText(int2 size)
        {
            _widthText.text = size.x.ToString();
            _heightText.text = size.y.ToString();
            _iterationsText.text = _iterationsSlider.value.ToString();
        }
    }
}