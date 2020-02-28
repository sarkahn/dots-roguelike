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

            World.GetOrCreateSystem<MoveSystem>().OnFrameBegin();

            while (!_actors.IsEmptyIgnoreFilter)
            {
                while( _actingEntity == Entity.Null )
                {
                    if (_turnBuffer.Length == 0)
                        PopulateTurnBuffer();

                    _actingEntity = GetNextActor();
                }

                // Allows the currently acting entity to take a turn. Will return false
                // if the actor has not finished their turn this frame. Generally this will
                // only happen when waiting for player input.
                if (!PerformTurnActions(_actingEntity))
                    break;
                else
                    _actingEntity = Entity.Null;
            }
        }

        /// <summary>
        /// Distributes energy in a loop and populates the turn buffer.
        /// </summary>
        void PopulateTurnBuffer()
        {
            var turnBuffer = _turnBuffer;
            while (turnBuffer.Length == 0)
            {
                Entities
                    .WithAll<Actor>()
                    .ForEach((Entity e, ref Energy energy, in Speed speed) =>
                    {
                        int value = speed.value == 0 ? Speed.Default.value : speed.value;

                        energy += value;

                        if (energy >= Energy.ActionThreshold)
                            turnBuffer.Add(e);
                    }).Run();
            }
        }

        /// <summary>
        /// Passes the actor to the appropriate system to take a turn. The system is determined by
        /// the actor's <seealso cref="ActorType"/>
        /// </summary>
        /// <returns></returns>
        bool PerformTurnActions(Entity e)
        {
            var actorType = EntityManager.GetComponentData<Actor>(e).actorType;
            var actionSystem = _dispatchMap[(int)actorType];

            return actionSystem.ProcessEntityTurn(e);
        }
        

        /// <summary>
        /// Retrieve the next entity from the turn buffer. Returns <see cref="Entity.Null"/>
        /// once the buffer is empty.
        /// </summary>
        // TODO : Might make more sense for the actor with the highest energy to go first?
        Entity GetNextActor()
        {
            var energyFromEntity = GetComponentDataFromEntity<Energy>(true);
            var turnBuffer = _turnBuffer;
            var speedFromEntity = GetComponentDataFromEntity<Speed>(true);
            Entity e = Entity.Null;
            int index = -1;

            Job
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

            if (index != -1)
            {
                e = _turnBuffer[index];
                _turnBuffer.RemoveAtSwapBack(index);
            }

            return e;
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