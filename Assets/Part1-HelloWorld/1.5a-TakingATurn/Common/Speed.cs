using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace RLTKTutorial.Part1_5A
{

    [System.Serializable]
    public struct Speed : IComponentData
    {
        public int value;
        public static implicit operator int(Speed c) => c.value;
        public static implicit operator Speed(int v) => new Speed { value = v };

        public static Speed Default => new Speed { value = 25 };
    }

}
