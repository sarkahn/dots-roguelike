using UnityEngine;
using System.Collections;
using Unity.Entities;
using UnityEngine.InputSystem;
using Unity.Mathematics;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    public class PlayerTurnSystem : SystemBase
    {
        EntityQuery _mapQuery;

        EndSimulationEntityCommandBufferSystem _barrier;

        TutorialControls _controls;
        InputAction _moveAction;
        InputAction _quitAction;

        protected override void OnCreate()
        {
            base.OnCreate();

            _controls = new TutorialControls();
            _controls.Enable();
            _moveAction = _controls.DefaultMapping.Move;
            _quitAction = _controls.DefaultMapping.QuitGame;

            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<MapState>(),
                ComponentType.ReadOnly<MapData>(),
                // Don't run if the map is generating
                ComponentType.Exclude<GenerateMap>(),
                ComponentType.Exclude<ChangeMonsterCount>()
                );

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            RequireForUpdate(_mapQuery);
        }

        protected override void OnUpdate()
        {
            if (_quitAction.triggered)
            {
                Application.Quit();
                return;
            }

            int2 dir = (int2)(_moveAction.triggered ? (float2)_moveAction.ReadValue<Vector2>() : float2.zero);

            if (dir.x == 0 && dir.y == 0)
                return;

            var mapEntity = _mapQuery.GetSingletonEntity();
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);
            var mapState = EntityManager.GetBuffer<MapState>(mapEntity);

            var monsterFromEntity = GetComponentDataFromEntity<Monster>(true);

            var buffer = _barrier.CreateCommandBuffer();

            Entities
                .WithReadOnly(monsterFromEntity)
                .WithAll<Player>()
                .WithAll<TakingATurn>()
                .ForEach((Entity e, ref Energy energy, ref Position pos) =>
                {
                    int2 dest = pos + dir;
                    int destIndex = dest.y * mapData.width + dest.x;

                    // Don't use up an action if we don't do anything meaningful
                    int cost = 0;

                    if ( MapUtility.CellIsUnblocked(destIndex, mapState) )
                    {
                        int oldIndex = pos.value.y * mapData.width + pos.value.x;
                        pos = dest;

                        // Immediately update map state so the next actors have up to date state.
                        MapUtility.MoveActor(e, oldIndex, destIndex, mapState);

                        cost = Energy.ActionThreshold;
                    }
                    else
                    {
                        var cell = mapState[destIndex];

                        if (monsterFromEntity.Exists(cell.content))
                        {
                            buffer.AddComponent(e, new Attacking
                            {
                                target = cell.content
                            });

                            cost = Energy.ActionThreshold;
                        }
                    }

                    energy -= cost;
                }).Run();
        }
    }
}