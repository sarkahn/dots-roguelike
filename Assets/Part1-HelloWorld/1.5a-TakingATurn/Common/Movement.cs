using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
    [System.Serializable]
    public struct Movement : IComponentData
    {
        public int2 value;
        public static implicit operator int2(Movement c) => c.value;
        public static implicit operator Movement(int2 v) => new Movement { value = v };
    }
}