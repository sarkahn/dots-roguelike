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
            WithSize(3, 3).WithPosition(1,1).WithAlignment(-1,-1).WithBorder();
    }
}
