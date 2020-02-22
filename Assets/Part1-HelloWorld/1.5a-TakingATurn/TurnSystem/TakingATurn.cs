using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace RLTKTutorial
{
    //public struct TakingATurn : IComponentData
    //{
    //}


    [System.Serializable]
    public struct TakingATurn : IComponentData
    {
        public bool value;
        public static implicit operator bool(TakingATurn c) => c.value;
        public static implicit operator TakingATurn(bool v) => new TakingATurn { value = v };
    }

}