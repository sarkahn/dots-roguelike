using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_5
{
    public struct GenerateMap : IComponentData
    {
        public int iterationCount;
        public int minRoomSize;
        public int maxRoomSize;
        public int seed;
        public static GenerateMap Default =>
            new GenerateMap
            {
                iterationCount = 30,
                minRoomSize = 6,
                maxRoomSize = 10,
                seed = 0
            };
    }

    public class GenerateMapAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int _iterationCount = 30;
        public int _minRoomSize = 6;
        public int _maxRoomSize = 10;
        public int _seed = 0;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new GenerateMap
            {
                iterationCount = _iterationCount,
                minRoomSize = _minRoomSize,
                maxRoomSize = _maxRoomSize,
                seed = _seed,
            });
        }
    }


}