using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

using Unity.Collections;


namespace RLTKTutorial.Part1_5A
{
    [System.Serializable]
    public struct Name : IComponentData
    {
        public FixedString32 value;
        public static implicit operator FixedString32(Name c) => c.value;
        public static implicit operator Name(FixedString32 v) => new Name { value = v };
        public override string ToString() => value.ToString();
    }
}