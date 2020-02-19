
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_4
{
    public class Part1_4Bootstrap : MonoBehaviour
    {
        void OnEnable()
        {
            Bootstrap.AddSimSystem<PlayerInputSystem>();
            Bootstrap.AddSimSystem<GenerateMapSystem>();
            Bootstrap.AddSimSystem<GenerateMapInputSystem>();
            Bootstrap.AddSimSystem<ResizeMapInputSystem>();
            Bootstrap.AddSimSystem<FOVSystem>();
            Bootstrap.AddSimSystem<InitializeTilesInMemorySystem>();
            Bootstrap.AddSimSystem<UpdateTilesInMemorySystem>();
            Bootstrap.AddSimSystem<RenderSystem>();
        }
    }
}