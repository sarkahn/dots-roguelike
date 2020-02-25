using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;


namespace RLTKTutorial.Part1_5A
{
    /// <summary>
    /// A base class for systems which perform an action during an <see cref="Actor"/>'s turn.
    /// Must be added via <see cref="GameTurnSystem.AddTurnActionSystem{T}"/>.
    /// </summary>
    public abstract class TurnActionSystem : SystemBase
    {
        /// <summary>
        /// Determines which system will be called by the <see cref="GameTurnSystem"/> when
        /// an <see cref="Actor"/>'s turn comes around.
        /// </summary>
        public abstract ActorType ActorType { get; }

        /// <summary>
        /// Returns true when entity is done taking their turn
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool ProcessEntityTurn(Entity e)
        {
            int cost = OnTakeTurn(e);

            if (cost <= 0)
                return false;

            var energy = EntityManager.GetComponentData<Energy>(e);
            energy -= cost;
            EntityManager.SetComponentData(e, energy);

            return energy < Energy.ActionThreshold;
        }

        /// <summary>
        /// Called by <see cref="GameTurnSystem"/> when an actor's turn comes
        /// around. Should return the energy cost of any action taken on their turn.
        /// </summary>
        /// <returns>The energy cost of any action taken on their turn.</returns>
        protected abstract int OnTakeTurn(Entity actor);

        /// <summary>
        /// Called once for each <see cref="TurnActionSystem"/> at the beginning of 
        /// <see cref="GameTurnSystem.OnUpdate"/>.
        /// </summary>
        public virtual void OnFrameBegin() { }

        protected override void OnUpdate()
        {}
    }

}
