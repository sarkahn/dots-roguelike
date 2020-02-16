using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{

    [System.Serializable]
    public struct PerformingAction : IComponentData
    {
        public int value;
        public static implicit operator int(PerformingAction c) => c.value;
        public static implicit operator PerformingAction(int v) => new PerformingAction { value = v };
    }


}
