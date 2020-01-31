using RLTK;
using RLTKTutorial.Game;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part3
{
    [System.Serializable]
    public struct TileBuffer : IBufferElementData
    {
        public TileType value;
        public static implicit operator TileType(TileBuffer b) => b.value;
        public static implicit operator TileBuffer(TileType v) => new TileBuffer { value = v };
    }
}