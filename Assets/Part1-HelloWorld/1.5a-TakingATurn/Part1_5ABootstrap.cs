
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

            Bootstrap.AddRenderSystem<RenderSystem>();

            Bootstrap.AddSimSystem<MoveSystem>();

            Bootstrap.AddSimSystem<ChangeMonsterCountSystem>();

            var turnSystem = Bootstrap.AddSimSystem<GameTurnSystem>();
            turnSystem.AddTurnActionSystem<PlayerTurnSystem>();
            turnSystem.AddTurnActionSystem<MonsterTurnSystem>();
        }
    }
}