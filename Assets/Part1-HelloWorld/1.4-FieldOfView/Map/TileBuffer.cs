
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

using RLTK;

namespace RLTKTutorial.Part1_4
{
    // A buffer of TileType which will represent the walkable map.
    [System.Serializable]
    public struct TileBuffer : IBufferElementData
    {
        public TileType value;
        public static implicit operator TileType(TileBuffer b) => b.value;
        public static implicit operator TileBuffer(TileType v) => new TileBuffer { value = v };
    }
}