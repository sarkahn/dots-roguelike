using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Unity.Entities;
using Unity.Mathematics;

public struct Position : IComponentData 
{
    public float2 Value;
    public static implicit operator float2(Position c) => c.Value;
    public static implicit operator Position(float2 v) => new Position { Value = v };
}
