using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_4
{
    [GenerateAuthoringComponent]
    public struct GenerateMap : IComponentData
    {
        public int iterationCount;
        public int seed;
        public int minRoomSize;
        public int maxRoomSize;

        public static GenerateMap Default => new GenerateMap
        {
            iterationCount = 30,
            minRoomSize = 6,
            maxRoomSize = 10,
            seed = 0
        };
    }


}