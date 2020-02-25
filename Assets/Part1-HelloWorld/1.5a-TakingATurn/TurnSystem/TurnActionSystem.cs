using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;


namespace RLTKTutorial.Part1_5A
{
    public abstract class TurnActionSystem : SystemBase
    {
        /// <summary>
        /// Determines which system will be called by the <see cref="GameTurnSystem"/> when
        /// an <see cref="Actor"/> is processed.
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

            return energy < 100;
        }

        protected abstract int OnTakeTurn(Entity e);

        /// <summary>
        /// Called once for each system at the beginning of that turn's frame.
        /// </summary>
        public virtual void OnFrameBegin() { }

        protected override void OnUpdate()
        {
        }
    }

}
