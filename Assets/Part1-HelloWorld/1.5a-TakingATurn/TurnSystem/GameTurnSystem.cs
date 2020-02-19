
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

// We need some mechanism by which to add systems to our GameTurnSystemGroup
// During GameTurnSystem.OnUpdate we will loop through these systems and update them.


// Query for turnaction
// Add performaction when action is complete
// These systems should do the turnaction query/add performaction steps during their update.
// They should NOT use command buffers. All structural changes should be done via entity manager.


namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    public class GameTurnSystemGroup : ComponentSystemGroup
    { }

    /// <summary>
    /// <para>Distributes energy to entities in a loop based on their speed until any entity has enough energy to act. Faster units will always go first.</para>
    /// <para>When an entity is able to act it will be given a <see cref="TakingATurn"/> component. During it's turn it should add an <see cref="ActionPerformed"/> component to itself so actions can be properly processed by the turn system.</para>
    /// <para>The turn system will automatically end the entity's turn once it performs enough actions to sufficiently drain it's energy.</para>
    /// </summary>
    [DisableAutoCreation]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class GameTurnSystem : SystemBase
    {
        const int EnergyActionThreshold = 100;

        EndSimulationEntityCommandBufferSystem _barrier;

        EntityQuery _actorsWaitingForEnergy;

        EntityQuery _actorsTakingTurns;

        EntityQuery _actors;

        // Buffer of actors who are ready to take their turn. As actors gain enough energy
        // to act they'll be added to this buffer
        NativeList<Entity> _turnBuffer;

        public NativeArray<Entity> CopyTurnBuffer(Allocator allocator) => 
            new NativeArray<Entity>(_turnBuffer, allocator);
        

        protected override void OnCreate()
        {
            base.OnCreate();

            _actors = GetEntityQuery(
                ComponentType.ReadOnly<Actor>()
                );

            _actorsTakingTurns = GetEntityQuery(
                ComponentType.ReadOnly<Actor>(),
                ComponentType.ReadOnly<TakingATurn>()
                );

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _turnBuffer = new NativeList<Entity>(Allocator.Persistent);
            
            AddTurnActionSystem<MoveSystem>();
            AddTurnActionSystem<MonsterAISystem>();
            
            RequireForUpdate(_actors);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _turnBuffer.Dispose();
            
        }

        /// <summary>
        /// <para>A TurnActionSystem is a system which performs some action when the entities it's querying for 
        /// are given a <see cref="TakingATurn"/> component by the <see cref="GameTurnSystem"/>.</para>
        /// 
        /// <para>It should include the <see cref="TakingATurn"/> component in it's entity queries and add an 
        /// <see cref="ActionPerformed"/> component to it's entities whenever they finish an action. The 
        /// <see cref="GameTurnSystem"/> will automatically process and remove <see cref="ActionPerformed"/>
        /// components.</para>
        /// 
        /// <para>The <see cref="GameTurnSystem"/> will not move to the next entity's turn until the currently
        /// acting entity has performed enough actions to fall below the <seealso cref="EnergyActionThreshold"/>.</para>
        /// </summary>
        public void AddTurnActionSystem<T>() where T : ComponentSystemBase
        {
            var group = World.GetOrCreateSystem<GameTurnSystemGroup>();
            group.AddSystemToUpdateList(World.GetOrCreateSystem<T>());
        }

        protected override void OnUpdate()
        {
            while (true)
            {
                // Loop until somone has enough energy to take a turn
                if ( _actorsTakingTurns.IsEmptyIgnoreFilter )
                {
                    if( _turnBuffer.Length > 0 )
                    {
                        DistributeTurn();
                        continue;
                    }
                    
                    if( !_actorsWaitingForEnergy.IsEmptyIgnoreFilter )
                        DistributeEnergy();

                    PopulateTurnBuffer();

                    continue;
                }

                // Process actions. We break from the loop
                // if an actor is taking their turn but does not
                // perform any actions (generally this will only happen
                // when waiting for player input).
                if (!ProcessTurnActions())
                    break;

                // If no actor is taking their turn and no actor is waiting for energy then there's nothing
                // left to process and we should bail out to avoid an infinite loop
                if (_actorsTakingTurns.IsEmptyIgnoreFilter && _actorsWaitingForEnergy.IsEmptyIgnoreFilter)
                    break;
            }

            //Debug.Log("Waiting for action");
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
                    //.WithoutBurst()
                    .ForEach((int entityInQueryIndex, Entity e, ref Energy energy, in Speed speed) =>
                    {
                        int value = speed.value == 0 ? Speed.Default.value : speed.value;

                        //Debug.Log($"Distributing {value} energy to {EntityManager.GetComponentData<Name>(e).value}");

                        energy += value;

                        if (energy >= EnergyActionThreshold)
                            canAct = true;
                    }).Run();
            }
        }


        /// <summary>
        /// Updates any systems in GameTurnSystem group, giving systems an opportunity to act.
        /// Returns false if no action was performed.
        /// </summary>
        bool ProcessTurnActions()
        {
            bool actionPerformed = false;

            // Update "turn systems". This allows any entities with a 'TakingATurn' component to perform an
            // action via a system. Systems that perform turn actions must be explicitly added to
            // GameTurnSystemGroup via GameTurnSystemGroup.AddToSystemUpdateList.
            // TODO: Make this less arcane
            World.GetOrCreateSystem<GameTurnSystemGroup>().Update();

            var buffer = new EntityCommandBuffer(Allocator.Temp);

            Entities
                //.WithoutBurst()
                .WithAll<Actor>()
                .WithAll<TakingATurn>()
                .ForEach((int entityInQueryIndex, Entity e, ref Energy energy, in ActionPerformed action) =>
                {
                    //Debug.Log($"{EntityManager.GetComponentData<Name>(e).value.ToString()} is performing an action of cost {action.cost}");
                    
                    actionPerformed = true;

                    if (action.cost > 0)
                        energy -= action.cost;
                    else
                        energy = 0;

                    buffer.RemoveComponent<ActionPerformed>(e);

                    if (energy < EnergyActionThreshold)
                    {
                        buffer.RemoveComponent<TakingATurn>(e);
                    }
                }).Run();

            buffer.Playback(EntityManager); 

            return actionPerformed;
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
        void DistributeTurn()
        {
            var energyFromEntity = GetComponentDataFromEntity<Energy>(true);
            var speedSort = new SpeedSort(GetComponentDataFromEntity<Speed>(true));
            var turnBuffer = _turnBuffer;
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

            Job
                //.WithoutBurst()
                .WithCode(() =>
            {
                // Remove anyone that's lost the ability to act
                for (int i = 0; i < turnBuffer.Length; ++i)
                    if (!ActorCanAct(turnBuffer[i], energyFromEntity))
                    {
                        turnBuffer.RemoveAtSwapBack(i);
                        i--;
                    }

                if (turnBuffer.Length == 0)
                    return;


                //Debug.Log($"Distributing turn to {EntityManager.GetComponentData<Name>(turnBuffer[0]).value.ToString()}");

                // Always sort before distributing a turn in case actor speeds changed between turns
                turnBuffer.Sort(speedSort);

                commandBuffer.AddComponent<TakingATurn>(turnBuffer[0]);
                turnBuffer.RemoveAtSwapBack(0);

            }).Run();

            commandBuffer.Playback(EntityManager);
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