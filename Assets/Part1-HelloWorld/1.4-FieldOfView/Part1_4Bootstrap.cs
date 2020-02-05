
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_4
{
    public class Part1_4Bootstrap : Bootstrap
    {
        protected override void AddSystems()
        {
            AddSystem<PlayerInputSystem>();
            AddSystem<GenerateMapSystem>();
            AddSystem<GenerateMapInputSystem>();
            AddSystem<ResizeMapInputSystem>();
            AddSystem<FOVSystem>();
            AddSystem<InitializeTilesInMemorySystem>();
            AddSystem<UpdateTilesInMemorySystem>();
            AddSystem<RenderSystem>();
        }
    }
}