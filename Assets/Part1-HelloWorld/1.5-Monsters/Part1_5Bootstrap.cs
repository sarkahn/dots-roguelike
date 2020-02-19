
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_5
{
    public class Part1_5Bootstrap : MonoBehaviour
    {
        void OnEnable()
        {
            Bootstrap.AddSimSystem<GenerateMapSystem>();

            Bootstrap.AddSimSystem<MapInputSystem>();

            Bootstrap.AddSimSystem<MoveSystem>();
            Bootstrap.AddSimSystem<VisibilitySystem>();
            Bootstrap.AddSimSystem<MonsterAISystem>();
            Bootstrap.AddSimSystem<PlayerInputSystem>();


            Bootstrap.AddSimSystem<RenderSystem>();
        }
    }
}