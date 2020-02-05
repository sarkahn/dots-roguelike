using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace RLTKTutorial.Part1_4
{
    /// <summary>
    /// Represent the tiles an entity has seen, presently and in the past.
    /// </summary>
    [System.Serializable]
    public struct TilesInMemory : IBufferElementData
    {
        public bool value;
        public static implicit operator bool(TilesInMemory c) => c.value;
        public static implicit operator TilesInMemory(bool v) => new TilesInMemory { value = v };
    }
}