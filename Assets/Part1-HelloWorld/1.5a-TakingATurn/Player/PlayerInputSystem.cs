using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    public class PlayerInputSystem : SystemBase
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

        protected override void OnUpdate()
        {
            var moveInput = _moveAction.triggered ? (float2)_moveAction.ReadValue<Vector2>() : float2.zero;
            
            if (moveInput.x == _previousMove.x && moveInput.y == _previousMove.y)
                return;
            _previousMove = moveInput;

            Entities
                .WithAll<Player>()
                .ForEach((ref Movement move) =>
                {
                    move = (int2)moveInput;
                }).Run();

            if (_quitAction.triggered)
                Application.Quit();
        }

    }
}