using RLTKTutorial.Part3.Map;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part3
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