
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_2
{
    public class Part1_2Bootstrap : MonoBehaviour
    {
        void OnEnable()
        {
            Bootstrap.AddSimSystem<PlayerInputSystem>();
            Bootstrap.AddSimSystem<GenerateMapSystem>();
            Bootstrap.AddSimSystem<RenderSystem>();
            Bootstrap.AddSimSystem<ResizeMapInputSystem>();
        }
    }
}