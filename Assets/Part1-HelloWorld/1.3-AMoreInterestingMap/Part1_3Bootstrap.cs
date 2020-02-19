
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_3
{
    public class Part1_3Bootstrap : MonoBehaviour
    {
        void OnEnable()
        {
            Bootstrap.AddSimSystem<PlayerInputSystem>();
            Bootstrap.AddSimSystem<GenerateMapSystem>();
            Bootstrap.AddSimSystem<GenerateMapInputSystem>();
            Bootstrap.AddSimSystem<RenderSystem>();
            Bootstrap.AddSimSystem<ResizeMapInputSystem>();
        }
    }
}