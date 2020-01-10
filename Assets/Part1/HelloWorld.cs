﻿
using RLTK.MonoBehaviours;
using UnityEngine;

public class HelloWorld : MonoBehaviour
{
    [SerializeField]
    SimpleConsoleProxy _console;

    private void Start()
    {
        _console.Print(5, 5, "Hello, world!");
    }
}
