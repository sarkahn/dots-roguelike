
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_2
{
    public class Part3Bootstrap : Bootstrap
    {
        protected override void AddSystems()
        {
            AddSystem<PlayerInputSystem>();
            AddSystem<GenerateMapSystem>();
            AddSystem<RenderSystem>();
        }
    }
}