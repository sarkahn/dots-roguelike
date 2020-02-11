
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_5
{
    [DisableAutoCreation]
    public class MoveSystem : JobComponentSystem
    {
        EntityQuery _mapQuery;
        EntityQuery _moveQuery;
        
        protected override void OnCreate()
        {
            _mapQuery = GetEntityQuery(
                ComponentType.ReadWrite<MapTiles>(),
                ComponentType.ReadOnly<MapData>()
                );

            _moveQuery = GetEntityQuery(
                ComponentType.ReadWrite<Position>(),
                ComponentType.ReadOnly<Movement>()
                );
            
            _moveQuery.AddChangedVersionFilter(typeof(Movement));

            RequireForUpdate(_mapQuery);
            RequireForUpdate(_moveQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_moveQuery.CalculateEntityCount() == 0)
                return inputDeps;

            var mapEntity = _mapQuery.GetSingletonEntity();
            var map = EntityManager.GetBuffer<MapTiles>(mapEntity);
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            inputDeps = Entities
                .WithReadOnly(map)
                .ForEach((ref Position p, ref Movement move) =>
            {
                int2 dest = p.value + move.value;
                int index = dest.y * mapData.width + dest.x;

                move = int2.zero;

                if (index < 0 || index >= map.Length)
                    return;

                if( map[index] != TileType.Wall )
                {
                    p = dest;
                }
            }).Schedule(inputDeps);

            return inputDeps;
        }
    }
}