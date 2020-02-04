using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RLTKTutorial.Part1_4
{
    [DisableAutoCreation]
    public class PlayerInputSystem : JobComponentSystem
    {
        TutorialControls _controls;
        InputAction _moveAction;
        InputAction _generateMapAction;
        InputAction _quitAction;

        Queue<Vector2> _inputQueue = new Queue<Vector2>();

        protected override void OnCreate()
        {
            _controls = new TutorialControls();
            _controls.Enable();
            _moveAction = _controls.DefaultMapping.Move;
            _generateMapAction = _controls.DefaultMapping.GenerateMap;
            _quitAction = _controls.DefaultMapping.QuitGame;
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if( _quitAction.triggered )
            {
                Application.Quit();
                return inputDeps;
            }

            float2 movement = _moveAction.triggered ? (float2)_moveAction.ReadValue<Vector2>() : float2.zero;
            bool generateMap = _generateMapAction.triggered;
            
            inputDeps = Entities.ForEach((ref PlayerInput input) =>
            {
                input.movement = movement;
                input.generateNewMap = generateMap;
            }).Schedule(inputDeps);

        return inputDeps;
        }
    }
}