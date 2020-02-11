using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_5
{
    [System.Serializable]
    public struct ViewRange : IComponentData
    {
        public int value;
        public static implicit operator int(ViewRange c) => c.value;
        public static implicit operator ViewRange(int v) => new ViewRange { value = v };
        public static ViewRange Default => 8;
    }

}

