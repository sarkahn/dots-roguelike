using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sark.Terminals;
using Unity.Entities;

public class TerminalsFromCode : MonoBehaviour
{
    private void Start()
    {
        TerminalUtility.MakeTerminal(World.DefaultGameObjectInjectionWorld.EntityManager).
            WithBorder().WithSize(15, 15).WithCenteredPosition(3,3);
    }
}
