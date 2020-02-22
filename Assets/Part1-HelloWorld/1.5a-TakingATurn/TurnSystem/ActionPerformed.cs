using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace RLTKTutorial.Part1_5A
{

    [System.Serializable]
    public struct ActionPerformed : IComponentData
    {
        public bool value;

        public int cost;
    }
}