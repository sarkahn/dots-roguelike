
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
    public class Part1_5ABootstrap : MonoBehaviour
    {
        void OnEnable()
        {
            Bootstrap.AddSimSystem<GenerateMapSystem>();

            Bootstrap.AddSimSystem<MapInputSystem>();
            Bootstrap.AddSimSystem<VisibilitySystem>();
            Bootstrap.AddSimSystem<GameTurnSystem>();

            Bootstrap.AddSimSystem<PlayerInputSystem>();

            Bootstrap.AddRenderSystem<RenderSystem>();


        }
    }
}