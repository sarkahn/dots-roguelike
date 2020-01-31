using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part3
{
    [GenerateAuthoringComponent]
    public struct GenerateMapProxy : IComponentData
    {
        public int iterationCount;
        public int2 playerPos;
        public int seed;
    }
}