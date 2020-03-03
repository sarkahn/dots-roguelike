
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
    public class Part1_5ABootstrap : MonoBehaviour
    {
        void OnEnable()
        {
            Bootstrap.AddInitSystem<TurnBeginSystem>();

            Bootstrap.AddSimSystem<PlayerTurnSystem>();
            Bootstrap.AddSimSystem<MonsterTurnSystem>();

            Bootstrap.AddSimSystem<MapInputSystem>();
            Bootstrap.AddSimSystem<GenerateMapSystem>();
            Bootstrap.AddSimSystem<UpdateMapStateSystem>();
            Bootstrap.AddSimSystem<VisibilitySystem>();

            Bootstrap.AddSimSystem<ChangeMonsterCountSystem>();

            Bootstrap.AddLateSimSystem<CombatSystem>();
            Bootstrap.AddLateSimSystem<DeathSystem>();
            Bootstrap.AddLateSimSystem<TurnEndSystem>();

            Bootstrap.AddRenderSystem<RenderSystem>();
        }
    }
}