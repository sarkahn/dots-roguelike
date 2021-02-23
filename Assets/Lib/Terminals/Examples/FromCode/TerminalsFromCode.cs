using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sark.Terminals;
using Unity.Entities;
using Unity.Mathematics;
using Sark.Terminals.Rendering;

public class TerminalsFromCode : MonoBehaviour
{
    public int2 Size;
    [Range(-1f, 1f)]
    public float AlignmentX = 0;
    [Range(-1f, 1f)]
    public float AlignmentY = 0;

    private void Start()
    {
        TerminalUtility.MakeTerminal()
            .WithSize(32, 3)
            .WithPosition(transform.position)
            .WithAlignment(AlignmentX, AlignmentY)
            .WithRenderMeshRenderer()
            .WithBorder()
            .WithText("Left click to build terminals", 1,1);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;
            TerminalUtility.MakeTerminal()
                .WithSize(Size)
                .WithPosition(pos)
                .WithAlignment(AlignmentX, AlignmentY)
                .WithNoiseOnce()
                .WithRenderMeshRenderer()
                .WithBorder();
        }
    }
}
