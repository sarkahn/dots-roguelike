using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Represents which tiles an entity can currently see
/// </summary>
[System.Serializable]
public struct FOVTiles : IBufferElementData
{
    public int2 value;
    public static implicit operator int2(FOVTiles c) => c.value;
    public static implicit operator FOVTiles(int2 v) => new FOVTiles { value = v };
}