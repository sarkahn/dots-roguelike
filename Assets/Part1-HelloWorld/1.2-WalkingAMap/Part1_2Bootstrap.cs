
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_2
{
    public class Part1_2Bootstrap : Bootstrap
    {
        protected override void AddSystems()
        {
            AddSystem<PlayerInputSystem>();
            AddSystem<GenerateMapSystem>();
            AddSystem<RenderSystem>();
            AddSystem<ResizeMapInputSystem>();
        }
    }
}