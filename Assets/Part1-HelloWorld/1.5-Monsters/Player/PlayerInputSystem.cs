using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RLTKTutorial.Part1_5
{
    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    [UpdateBefore(typeof(MoveSystem))]
    public class PlayerInputSystem : JobComponentSystem
    {
        TutorialControls _controls;
        InputAction _moveAction;
        InputAction _quitAction;

        float2 _previousMove;

        protected override void OnCreate()
        {
            base.OnCreate();

            _controls = new TutorialControls();
            _controls.Enable();
            _moveAction = _controls.DefaultMapping.Move;
            _quitAction = _controls.DefaultMapping.QuitGame;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var moveInput = _moveAction.triggered ? (float2)_moveAction.ReadValue<Vector2>() : float2.zero;
            
            if (moveInput.x == _previousMove.x && moveInput.y == _previousMove.y)
                return inputDeps;
            _previousMove = moveInput;

            inputDeps = Entities
                .WithAll<Player>()
                .ForEach((ref Movement move) =>
                {
                    move = (int2)moveInput;
                }).Schedule(inputDeps);

            if (_quitAction.triggered)
                Application.Quit();

            return inputDeps;
        }

    }
}