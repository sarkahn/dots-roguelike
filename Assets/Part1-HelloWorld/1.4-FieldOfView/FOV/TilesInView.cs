using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace RLTKTutorial.Part1_4
{
    /// <summary>
    /// Represents which tiles an entity can currently see
    /// </summary>
    [System.Serializable]
    public struct TilesInView : IBufferElementData
    {
        public int2 value;
        public static implicit operator int2(TilesInView c) => c.value;
        public static implicit operator TilesInView(int2 v) => new TilesInView { value = v };
    }
}