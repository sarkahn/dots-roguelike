using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
    public struct MapState : IBufferElementData
    {
        public bool blocked;
        public Entity content;
    } 
}
