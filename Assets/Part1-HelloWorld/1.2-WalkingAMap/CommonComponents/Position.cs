using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
[GenerateAuthoringComponent]
public struct Position : IComponentData
{
    public int2 value;
    public static implicit operator int2(Position c) => c.value;
    public static implicit operator Position(int2 v) => new Position { value = v };
}


