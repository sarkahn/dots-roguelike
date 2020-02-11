using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_5
{
    [GenerateAuthoringComponent]
    public struct PlayerInput : IComponentData
    {
        public float2 movement;
    }
}