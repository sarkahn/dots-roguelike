using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace DotsRogue
{
    public interface IGameState
    {}

    public struct GameStateTakingTurns : IComponentData, IGameState
    {}

    public struct GameStateShowInventory : IComponentData, IGameState
    {}

    public struct GameStateShowTargeting : IComponentData, IGameState
    {
        public int range;
        public Entity source;
        public Entity target;
    }

    public struct GameState : IComponentData
    {
        public ComponentType currentState;
    }

    public struct GameStateJobContext
    {
        public Entity entity;
        ComponentDataFromEntity<GameState> stateFromEntity;

        public GameState GameState
        {
            get => stateFromEntity[entity];
            set => stateFromEntity[entity] = value;
        }

        public GameStateJobContext(SystemBase sys, bool readOnly = false)
        {
            entity = sys.GetSingletonEntity<GameState>();
            stateFromEntity = sys.GetComponentDataFromEntity<GameState>(readOnly);
        }

        public void ChangeGameState<T>(EntityCommandBuffer ecb, T t = default)
            where T : unmanaged, IComponentData, IGameState
        {
            var state = GameState;
            ecb.RemoveComponent(entity, state.currentState);
            ecb.AddComponent(entity, t);
            state.currentState = ComponentType.ReadOnly<T>();
            GameState = state;
        }

        public void ChangeGameState<T>(EntityManager em, T t = default)
            where T : unmanaged, IComponentData, IGameState
        {
            var state = GameState;
            em.RemoveComponent(entity, state.currentState);
            em.AddComponentData(entity, t);
            state.currentState = ComponentType.ReadOnly<T>();
            GameState = state;
        }
    }


}
