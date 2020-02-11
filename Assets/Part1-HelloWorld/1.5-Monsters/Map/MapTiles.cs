
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

using RLTK;

namespace RLTKTutorial.Part1_5
{
    // A buffer of TileType which will represent the walkable map.
    [System.Serializable]
    public struct MapTiles : IBufferElementData
    {
        public TileType value;
        public static implicit operator TileType(MapTiles b) => b.value;
        public static implicit operator MapTiles(TileType v) => new MapTiles { value = v };
    }
}