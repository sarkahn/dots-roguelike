using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_2
{
    [GenerateAuthoringComponent]
    public struct GenerateMap : IComponentData
    {
        public int iterationCount;
        public int2 playerPos;
        public int seed;
    }
}