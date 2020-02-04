using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;




[System.Serializable]
public struct FOVRange : IComponentData
{
    public int value;
    public static implicit operator int(FOVRange c) => c.value;
    public static implicit operator FOVRange(int v) => new FOVRange { value = v };
    public static FOVRange Default => 8;
}



