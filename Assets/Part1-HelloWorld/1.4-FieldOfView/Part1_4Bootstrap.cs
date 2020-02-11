
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_4
{
    public class Part1_4Bootstrap : MonoBehaviour
    {
        void OnEnable()
        {
            Bootstrap.AddSystem<PlayerInputSystem>();
            Bootstrap.AddSystem<GenerateMapSystem>();
            Bootstrap.AddSystem<GenerateMapInputSystem>();
            Bootstrap.AddSystem<ResizeMapInputSystem>();
            Bootstrap.AddSystem<FOVSystem>();
            Bootstrap.AddSystem<InitializeTilesInMemorySystem>();
            Bootstrap.AddSystem<UpdateTilesInMemorySystem>();
            Bootstrap.AddSystem<RenderSystem>();
        }
    }
}