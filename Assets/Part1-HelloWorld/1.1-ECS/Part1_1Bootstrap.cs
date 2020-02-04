using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_1
{
    public class Part1_1Bootstrap : Bootstrap
    {
        protected override void AddSystems()
        {
            AddSystem<RenderSystem>();
            AddSystem<MoveLeftSystem>();
            AddSystem<MovePlayerSystem>();
            AddSystem<ReadInputSystem>();
        }
    }
}