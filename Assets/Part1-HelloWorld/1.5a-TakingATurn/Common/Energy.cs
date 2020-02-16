using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{

    [System.Serializable]
    public struct Energy : IComponentData
    {
        public int value;
        public static implicit operator int(Energy c) => c.value;
        public static implicit operator Energy(int v) => new Energy { value = v };
    }
}