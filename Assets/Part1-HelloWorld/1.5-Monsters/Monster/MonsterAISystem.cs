using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace RLTKTutorial.Part1_5
{
    [DisableAutoCreation]
    [UpdateAfter(typeof(VisibilitySystem))]
    public class MonsterAISystem : JobComponentSystem
    {
        EntityQuery _playerQuery;
        EntityQuery _mapQuery;

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
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var mapEntity = _mapQuery.GetSingletonEntity();
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            // Early out if the map is regenerating.
            if (EntityManager.HasComponent<GenerateMap>(mapEntity))
                return inputDeps;

            var playerEntity = _playerQuery.GetSingletonEntity();
            var playerPos = (int2)EntityManager.GetComponentData<Position>(playerEntity);
            var playerName = EntityManager.GetComponentData<Name>(playerEntity);
            
            int playerIndex = playerPos.y * mapData.width + playerPos.x;

            inputDeps = Entities
                .WithoutBurst()
                .WithAll<Monster>()
                .ForEach((in DynamicBuffer<TilesInView> view, in Name name) =>
                {
                    if(view[playerIndex] )
                    {
                        Debug.Log($"{name} shouts angrily at {playerName}");
                    }
                }).Schedule(inputDeps);


            return inputDeps;
        }
    }
}