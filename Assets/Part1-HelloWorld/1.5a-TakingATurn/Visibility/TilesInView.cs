using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace RLTKTutorial.Part1_5A
{
    /// <summary>
    /// Represents which tiles an entity can currently see
    /// </summary>
    [System.Serializable]
    public struct TilesInView : IBufferElementData
    {
        public bool value;
        public static implicit operator bool(TilesInView c) => c.value;
        public static implicit operator TilesInView(bool v) => new TilesInView { value = v };
    }
}