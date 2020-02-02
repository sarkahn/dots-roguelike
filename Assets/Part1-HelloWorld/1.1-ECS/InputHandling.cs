using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RLTKTutorial.Part1_1
{
    struct InputData : IComponentData
    {
        public float2 Value;
    }

    [DisableAutoCreation]
    [AlwaysSynchronizeSystem]
    public class ReadInputSystem : JobComponentSystem
    {
        TutorialControls _controls;
        InputAction _moveAction;
        InputAction _quitAction;

        protected override void OnCreate()
        {
            _controls = new TutorialControls();
            _controls.Enable();
            _moveAction = _controls.DefaultMapping.Move;
            _quitAction = _controls.DefaultMapping.QuitGame;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if(_quitAction.triggered )
            {
                Application.Quit();
                return inputDeps;
            }

            float2 move = _moveAction.triggered ? (float2)_moveAction.ReadValue<Vector2>() : float2.zero;

            Entities.ForEach((ref InputData input) =>
            {
                input.Value.x = move.x;
                input.Value.y = move.y;
            }).Run();

            return default;
        }
    }

    [DisableAutoCreation]
    [AlwaysSynchronizeSystem]
    public class MovePlayerSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Entities.ForEach((ref Position pos, in InputData input) =>
            {
                pos.Value += input.Value;
            }).Run();

            return default;
        }
    }
}