using UnityEngine;
using System.Collections;
using Unity.Entities;
using UnityEngine.InputSystem;
using Unity.Mathematics;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    public class PlayerTurnSystem : TurnActionSystem
    {
        public override ActorType ActorType => ActorType.Player;

        TutorialControls _controls;
        InputAction _moveAction;
        InputAction _quitAction;

        int2 _previousMove;

        MoveSystem _moveSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            _controls = new TutorialControls();
            _controls.Enable();
            _moveAction = _controls.DefaultMapping.Move;
            _quitAction = _controls.DefaultMapping.QuitGame;

            _moveSystem = World.GetOrCreateSystem<MoveSystem>();

        }

        protected override int OnTakeTurn(Entity actor)
        {
            int2 move = (int2)(_moveAction.triggered ? (float2)_moveAction.ReadValue<Vector2>() : float2.zero);

            if (move.x == _previousMove.x && move.y == _previousMove.y)
                return 0;

            _previousMove = move;

            EntityManager.SetComponentData<Movement>(actor, move);

            if (move.x == 0 && move.y == 0)
                return 0;

            if (_quitAction.triggered)
                Application.Quit();

            return 100;
        }
    }
}