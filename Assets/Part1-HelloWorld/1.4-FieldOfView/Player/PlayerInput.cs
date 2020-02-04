using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_4
{
    [GenerateAuthoringComponent]
    public struct PlayerInput : IComponentData
    {
        public float2 movement;
        public bool generateNewMap;
    }
}