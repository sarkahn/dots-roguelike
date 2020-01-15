using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Renderable : IComponentData
{
    public Color FGColor;
    public Color BGColor;
    public byte Glyph;
}
