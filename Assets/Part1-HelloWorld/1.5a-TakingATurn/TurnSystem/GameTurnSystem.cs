using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace RLTKTutorial.Part1_5A
{
    /// <summary>
    /// <para>Distributes energy to entities in a loop based on their speed until any entity has enough energy to act. Faster units will always go first.</para>
    /// <para><see cref="Actor"/>s perform actions via their matching <seealso cref="TurnActionSystem"/>, based on their <seealso cref="ActorType"/>.</para>
    /// </summary>
    [DisableAutoCreation]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateBefore(typeof(MoveSystem))]
    public class GameTurnSystem : SystemBase
    {

        EntityQuery _actors;

        // Buffer of actors who are ready to take their turn. As actors gain enough energy
        // to act they'll be added to this buffer
        NativeList<Entity> _turnBuffer;

        /// <summary>
        /// Matches <see cref="Actor"/>s to their <see cref="TurnActionSystem"/> based on their <seealso cref="ActorType"/>.
        /// </summary>
        Dictionary<int, TurnActionSystem> _dispatchMap = new Dictionary<int, TurnActionSystem>();

        /// <summary>
        /// Currently acting entity.
        /// </summary>
        Entity _actingEntity;

        public NativeArray<Entity> CopyTurnBuffer(Allocator allocator) => 
            new NativeArray<Entity>(_turnBuffer, allocator);
        
        protected override void OnCreate()
        {
            base.OnCreate();

            _actors = GetEntityQuery(
                ComponentType.ReadOnly<Actor>()
                );

            _turnBuffer = new NativeList<Entity>(50, Allocator.Persistent);

            _actingEntity = Entity.Null;
            
            RequireForUpdate(_actors);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _turnBuffer.Dispose();
            
        }

        public void AddTurnActionSystem<T>() where T : TurnActionSystem
        {
            var system = World.GetOrCreateSystem<T>();
            _dispatchMap.Add((int)system.ActorType, system);
        }

        protected override void OnUpdate()
        {
            foreach (var pair in _dispatchMap)
                pair.Value.OnFrameBegin();

            while (!_actors.IsEmptyIgnoreFilter)
            {
                if( _actingEntity == Entity.Null )
                {
                    // Loop until somone has enough energy to take a turn
                    if (_turnBuffer.Length == 0)
                    {
                        DistributeEnergy();

                        PopulateTurnBuffer();

                        continue;
                    }

                    int i = GetNextActorIndex();

                    if (i == -1)
                        break;

                    _actingEntity = _turnBuffer[i];
                    _turnBuffer.RemoveAtSwapBack(i);
                }

                // Process actions. We break from the loop
                // if an actor is taking their turn but does not
                // perform any actions (generally this will only happen
                // when waiting for player input).
                if (!ProcessTurnActions())
                    break;
            }
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
                    .WithAll<Actor>()
                    //.WithoutBurst()
                    .ForEach((int entityInQueryIndex, Entity e, ref Energy energy, in Speed speed) =>
                    {
                        int value = speed.value == 0 ? Speed.Default.value : speed.value;

                        //Debug.Log($"Distributing {value} energy to {EntityManager.GetComponentData<Name>(e).value}");

                        energy += value;

                        if (energy >= Energy.ActionThreshold)
                            canAct = true;
                    }).Run();
            }
        }

        /// <summary>
        /// Passes an actor to the appropriate system to take it's turn based on it's
        /// <seealso cref="ActorType"/>
        /// </summary>
        /// <returns></returns>
        bool ProcessTurnActions()
        {
            var curr = _actingEntity;
            var actorType = EntityManager.GetComponentData<Actor>(curr).actorType;
            var actionSystem = _dispatchMap[(int)actorType];

            bool done = actionSystem.ProcessEntityTurn(curr);

            if(done)
            {
                _actingEntity = Entity.Null;
                return true;
            }

            return false;
        }
        

        /// <summary>
        /// Add any entities above the energy threshold to the turn buffer.
        /// </summary>
        void PopulateTurnBuffer()
        {
            var turnBuffer = _turnBuffer;
            Entities
                .WithAll<Actor>()
                .ForEach((int entityInQueryIndex, Entity e, in Energy energy) =>
                {
                    if ( energy >= Energy.ActionThreshold )
                        if( !turnBuffer.Contains(e))
                            turnBuffer.Add(e);
                }).Run();
        }

        /// <summary>
        /// Clear any invalid entities from the turn buffer and sort it in case
        /// any relevant actor state changed between turns.
        /// </summary>
        // TODO : Might make more sense for the actor with the highest energy to go first?
        int GetNextActorIndex()
        {
            var energyFromEntity = GetComponentDataFromEntity<Energy>(true);
            var speedSort = new SpeedSort(GetComponentDataFromEntity<Speed>(true));
            var turnBuffer = _turnBuffer;
            var speedFromEntity = GetComponentDataFromEntity<Speed>(true);
            int index = -1;

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

                int highestSpeed = int.MinValue;

                for( int i = 0; i < turnBuffer.Length; ++i )
                {
                    int speed = speedFromEntity[turnBuffer[i]];
                    if (speed > highestSpeed)
                    {
                        highestSpeed = speed;
                        index = i;
                    }
                }
            }).Run();

            return index;
        }
        
        static bool ActorCanAct(Entity e, ComponentDataFromEntity<Energy> energyFromEntity)
        {
            return e != Entity.Null
                && energyFromEntity.Exists(e)
                && energyFromEntity[e].value >= Energy.ActionThreshold;
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