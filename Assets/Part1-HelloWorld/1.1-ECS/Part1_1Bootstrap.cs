using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_1
{
    public class Part1_1Bootstrap : MonoBehaviour
    {
        private void OnEnable()
        {
            Bootstrap.AddSystem<RenderSystem>();
            Bootstrap.AddSystem<MoveLeftSystem>();
            Bootstrap.AddSystem<MovePlayerSystem>();
            Bootstrap.AddSystem<ReadInputSystem>();
        }
        
    }
}