
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_3
{
    public class Part1_3Bootstrap : Bootstrap
    {
        protected override void AddSystems()
        {
            AddSystem<PlayerInputSystem>();
            AddSystem<GenerateMapSystem>();
            AddSystem<GenerateMapInputSystem>();
            AddSystem<RenderSystem>();
            AddSystem<ResizeMapInputSystem>();
        }
    }
}