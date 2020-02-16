
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
    public class Part1_5ABootstrap : MonoBehaviour
    {
        void OnEnable()
        {
            Bootstrap.AddSystem<GenerateMapSystem>();

            Bootstrap.AddSystem<MapInputSystem>();

            Bootstrap.AddSystem<MoveSystem>();
            Bootstrap.AddSystem<VisibilitySystem>();
            Bootstrap.AddSystem<MonsterAISystem>();
            Bootstrap.AddSystem<PlayerInputSystem>();
            
            Bootstrap.AddSystem<RenderSystem>();

            Bootstrap.AddSystem<GameTurnSystem>();
        }
    }
}