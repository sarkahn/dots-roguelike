using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
    [System.Serializable]
    public struct GenerateMap : IComponentData
    {
        public int iterationCount;
        public int minRoomSize;
        public int maxRoomSize;
        public int seed;
        public int monsterCount;
        public static GenerateMap Default =>
            new GenerateMap
            {
                iterationCount = 30,
                minRoomSize = 6,
                maxRoomSize = 10,
                seed = 0,
                monsterCount = 15
            };
    }

    public class GenerateMapAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        GenerateMap _properties = GenerateMap.Default;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, _properties);
        }
    }


}