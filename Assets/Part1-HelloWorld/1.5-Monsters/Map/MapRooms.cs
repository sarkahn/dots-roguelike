
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using RLTK;


namespace RLTKTutorial.Part1_5
{
    [System.Serializable]
    [InternalBufferCapacity(30)]
    public struct MapRooms : IBufferElementData
    {
        public IntRect value;
        public static implicit operator IntRect(MapRooms b) => b.value;
        public static implicit operator MapRooms(IntRect v) => new MapRooms { value = v };
    }
}