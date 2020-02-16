
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;

namespace RLTKTutorial.Part1_5A
{

    /// <summary>
    /// <para>Distributes energy to entities in a loop based on their speed until any entity has enough energy to act. Faster units will always go first.</para>
    /// <para>When an entity is able to act it will be given a <see cref="TakingATurn"/> component. During it's turn it should add an <see cref="ActionPerformed"/> component to itself so actions can be properly processed by the turn system.</para>
    /// <para>The turn system will automatically end the entity's turn once it performs enough actions to sufficiently drain it's energy.</para>
    /// </summary>
    [DisableAutoCreation]
    [AlwaysSynchronizeSystem]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class GameTurnSystem : JobComponentSystem
    {
        const int EnergyActionThreshold = 100;

        EndSimulationEntityCommandBufferSystem _barrier;

        EntityQuery _actorsWaitingForEnergy;

        EntityQuery _actorsTakingTurns;

        // Buffer of actors who are ready to take their turn. As actors gain enough energy
        // to act they'll be added to this buffer
        NativeList<Entity> _turnBuffer;

        public NativeArray<Entity> CopyTurnBuffer(Allocator allocator) => 
            new NativeArray<Entity>(_turnBuffer, allocator);
        

        protected override void OnCreate()
        {
            base.OnCreate();

            _actorsTakingTurns = GetEntityQuery(
                ComponentType.ReadOnly<Actor>(),
                ComponentType.ReadOnly<TakingATurn>()
                );

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _turnBuffer = new NativeList<Entity>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _turnBuffer.Dispose();
        }

        public void AddTurnSystem<T>() where T : ComponentSystemBase
        {
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var commandBuffer = _barrier.CreateCommandBuffer();

            bool takingATurn = !_actorsTakingTurns.IsEmptyIgnoreFilter;
            bool nextTurn = true;

            if (takingATurn)
                nextTurn = ProcessTurnActions(commandBuffer);
            
            // Distribute energy to actors as long as no one is acting and there's no one in queue to act
            if (!takingATurn && _turnBuffer.Length == 0 && !_actorsWaitingForEnergy.IsEmptyIgnoreFilter )
                DistributeEnergy();

            // Add any actors to the turn queue if their energy is above the threshold
            PopulateTurnBuffer();

            // Hand out a turn to the next actor
            if (_turnBuffer.Length > 0 && nextTurn)
                DistributeTurn(commandBuffer);
             
            return default;
        }

        /// <summary>
        /// Process actions for any entities currently taking their turn. Returns true if the current turn is complete.
        /// </summary>
        bool ProcessTurnActions(EntityCommandBuffer buffer)
        {
            bool finishedActing = false;

            Entities
                .WithAll<Actor>()
                .WithAll<TakingATurn>()
                .ForEach((int entityInQueryIndex, Entity e, ref Energy energy, in ActionPerformed action) =>
                {
                    if (action.cost > 0)
                        energy -= action.cost;
                    else
                        energy = 0;

                    buffer.RemoveComponent<ActionPerformed>(e);

                    if (energy < EnergyActionThreshold)
                    {
                        buffer.RemoveComponent<TakingATurn>(e);
                        finishedActing = true;
                    }
                }).Run();

            return finishedActing;
        }
        
        /// <summary>
        /// Add energy to waiting entities in a loop until at least one entity is ready to act.
        /// </summary>
        void DistributeEnergy()
        {
            bool canAct = false;
            while (!canAct)
            {
                Entities
                    .WithStoreEntityQueryInField(ref _actorsWaitingForEnergy)
                    .WithAll<Actor>()
                    .WithNone<TakingATurn>()
                    .ForEach((int entityInQueryIndex, Entity e, ref Energy energy, in Speed speed) =>
                    {
                        if (speed.value > 0)
                            energy += speed;
                        else
                            energy += Speed.Default.value;

                        if (energy >= EnergyActionThreshold)
                            canAct = true;
                    }).Run();
            }
        }
        
        /// <summary>
        /// Add any entities above the energy threshold to the turn buffer.
        /// </summary>
        void PopulateTurnBuffer()
        {
            var turnBuffer = _turnBuffer;
            Entities
                .WithAll<Actor>()
                .WithNone<TakingATurn>()
                .ForEach((int entityInQueryIndex, Entity e, in Energy energy) =>
                {
                    if ( energy >= EnergyActionThreshold )
                        if( !turnBuffer.Contains(e))
                            turnBuffer.Add(e);
                }).Run();
        }

        /// <summary>
        /// Distribute a turn to the next actor in the turn buffer.
        /// Actors with higher speeds will always go first.
        /// </summary>
        // TODO : Might make more sense for the actor with the highest energy to go first?
        void DistributeTurn(EntityCommandBuffer commandBuffer)
        {
            var energyFromEntity = GetComponentDataFromEntity<Energy>(true);
            var speedSort = new SpeedSort(GetComponentDataFromEntity<Speed>(true));
            var turnBuffer = _turnBuffer;
            Job.WithCode(() =>
            {
                // Remove anyone that's lost the ability to act
                for (int i = 0; i < turnBuffer.Length; ++i)
                    if (!ActorCanAct(turnBuffer[i], energyFromEntity))
                        turnBuffer.RemoveAtSwapBack(i);

                if (turnBuffer.Length == 0)
                    return;

                // Always sort before distributing a turn in case actor speeds changed between turns
                turnBuffer.Sort(speedSort);

                commandBuffer.AddComponent<TakingATurn>(turnBuffer[0]);
                turnBuffer.RemoveAtSwapBack(0);
            }).Run();
        }
        
        static bool ActorCanAct(Entity e, ComponentDataFromEntity<Energy> energyFromEntity)
        {
            return e != Entity.Null
                && energyFromEntity.Exists(e)
                && energyFromEntity[e].value >= EnergyActionThreshold;
        }
        
        struct SpeedSort : IComparer<Entity>
        {
            ComponentDataFromEntity<Speed> speedFromEntity;

            public SpeedSort(ComponentDataFromEntity<Speed> speedFromEntity)
            {
                this.speedFromEntity = speedFromEntity;
            }

            public int Compare(Entity a, Entity b) => -speedFromEntity[a].value.CompareTo(speedFromEntity[b].value);
        }
        
    }
}