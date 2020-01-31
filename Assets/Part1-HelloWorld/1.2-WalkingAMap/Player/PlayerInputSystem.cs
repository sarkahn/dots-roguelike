using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RLTKTutorial.Part3
{
    [DisableAutoCreation]
    public class PlayerInputSystem : JobComponentSystem
    {
        MapNavigationInput _controls;
        InputAction _moveInput;

        Queue<Vector2> _inputQueue = new Queue<Vector2>();

        protected override void OnCreate()
        {
            _controls = new MapNavigationInput();
            _controls.Enable();
            _moveInput = _controls.MapNavigation.Move;
            _moveInput.performed += OnPerformed;
        }

        private void OnPerformed(InputAction.CallbackContext ctx)
        {
            _inputQueue.Enqueue(ctx.ReadValue<Vector2>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //float h = Input.GetAxisRaw("Horizontal");
            //float v = Input.GetAxisRaw("Vertical");

            //float2 move = new float2(h, v);
            
            
            if( _inputQueue.Count > 0 )
            {
                float2 move = _inputQueue.Dequeue();
                inputDeps = Entities.ForEach((ref PlayerInput input) =>
                {
                    input.movement = move;
                }).Schedule(inputDeps);
            }

        return inputDeps;
        }
    }
}