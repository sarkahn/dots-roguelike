using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    //[UpdateAfter(typeof(VisibilitySystem))]
    public class MonsterAISystem : SystemBase
    {
        EntityQuery _playerQuery;
        EntityQuery _mapQuery;

        Random _rand;

        EndSimulationEntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _playerQuery = GetEntityQuery(
                ComponentType.ReadOnly<Player>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<Name>()
                );

            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<MapData>()
                );

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _rand = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        }

        protected override void OnUpdate()
        {
            var mapEntity = _mapQuery.GetSingletonEntity();
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            // Early out if the map is regenerating.
            if (EntityManager.HasComponent<GenerateMap>(mapEntity))
                return;

            if (_playerQuery.IsEmptyIgnoreFilter)
                return;
            
            var playerEntity = _playerQuery.GetSingletonEntity();
            var playerPos = (int2)EntityManager.GetComponentData<Position>(playerEntity);
            var playerName = EntityManager.GetComponentData<Name>(playerEntity);
            
            int playerIndex = playerPos.y * mapData.width + playerPos.x;

            var buffer = new EntityCommandBuffer(Allocator.Temp); //_barrier.CreateCommandBuffer().ToConcurrent();

            //StandAndDeliver(playerIndex, buffer);
            Wander(buffer);

            buffer.Playback(EntityManager);
            
        }

        void StandAndDeliver(int playerIndex, EntityCommandBuffer buffer)
        {
            Entities
                .WithoutBurst()
                .WithAll<Monster>()
                .WithAll<TakingATurn>()
                .WithNone<ActionPerformed>()
                .ForEach((int entityInQueryIndex, Entity e, in DynamicBuffer<TilesInView> view, in Name name) =>
                {
                    var action = new ActionPerformed();

                    if (view[playerIndex])
                    {
                                    //Debug.Log($"{name} shouts angrily at {playerName}");
                                    action.cost = 100;
                    }
                    else
                    {
                                    //Debug.Log($"{name} stands around");
                                    action.cost = 50;
                    }

                    buffer.AddComponent(e, action);
                }).Run();
        }

        void Wander(EntityCommandBuffer buffer)
        {
            var rand = _rand;
            Entities
                .WithAll<Monster>()
                .WithAll<TakingATurn>()
                .WithNone<ActionPerformed>()
                .ForEach((int entityInQueryIndex, Entity e, in DynamicBuffer<TilesInView> view, in Name name) =>
                {
                    var action = new ActionPerformed();
                    action.cost = 100;

                    var dir = GetRandomDirection(ref rand);

                    buffer.SetComponent<Movement>(e, dir);

                    buffer.AddComponent(e, action);
                }).Run();
            
            _rand = rand;
        }

        static int2 GetRandomDirection(ref Random rand)
        {
            int i = rand.NextInt(0, 5);
            switch (i)
            {
                case 0: return new int2(-1, 0);
                case 1: return new int2(1, 0);
                case 2: return new int2(0, -1);
                case 3: return new int2(0, 1);
            }
            return default;
        }
    }
}