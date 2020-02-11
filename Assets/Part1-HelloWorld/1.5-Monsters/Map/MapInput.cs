using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_5
{
    public struct MapInput : IComponentData
    {
        public float2 resizeMap;
        public bool generateNewMap;
    }
}