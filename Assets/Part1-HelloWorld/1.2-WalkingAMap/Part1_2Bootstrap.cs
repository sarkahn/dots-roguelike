
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_2
{
    public class Part1_2Bootstrap : MonoBehaviour
    {
        void OnEnable()
        {
            Bootstrap.AddSystem<PlayerInputSystem>();
            Bootstrap.AddSystem<GenerateMapSystem>();
            Bootstrap.AddSystem<RenderSystem>();
            Bootstrap.AddSystem<ResizeMapInputSystem>();
        }
    }
}