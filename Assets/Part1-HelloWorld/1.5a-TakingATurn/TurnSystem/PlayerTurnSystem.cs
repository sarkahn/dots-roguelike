using UnityEngine;
using System.Collections;
using Unity.Entities;
using UnityEngine.InputSystem;
using Unity.Mathematics;

namespace RLTKTutorial.Part1_5A
{
    public class PlayerTurnSystem : TurnActionSystem
    {
        public override ActorType ActorType => ActorType.Player;

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

            Debug.Log("Player turn OnCreate");
        }

        protected override int OnTakeTurn(Entity e)
        {
            //Debug.Log("Taking player turn");

            var moveInput = _moveAction.triggered ? (float2)_moveAction.ReadValue<Vector2>() : float2.zero;

            if (moveInput.x == _previousMove.x && moveInput.y == _previousMove.y)
                return 0;

            Debug.Log("MOVING PLAYER?");

            _previousMove = moveInput;

            EntityManager.SetComponentData<Movement>(e, (int2)moveInput);

            if (_quitAction.triggered)
                Application.Quit();

            return 100;
        }
    }
}