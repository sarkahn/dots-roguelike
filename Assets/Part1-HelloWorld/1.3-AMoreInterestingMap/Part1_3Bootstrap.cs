
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_3
{
    public class Part1_3Bootstrap : MonoBehaviour
    {
        void OnEnable()
        {
            Bootstrap.AddSystem<PlayerInputSystem>();
            Bootstrap.AddSystem<GenerateMapSystem>();
            Bootstrap.AddSystem<GenerateMapInputSystem>();
            Bootstrap.AddSystem<RenderSystem>();
            Bootstrap.AddSystem<ResizeMapInputSystem>();
        }
    }
}