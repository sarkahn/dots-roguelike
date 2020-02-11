
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_2
{
    public class PlayerMoveSystem : JobComponentSystem
    {
        EntityQuery _mapQuery;

        protected override void OnCreate()
        {
            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<MapTiles>(),
                ComponentType.ReadOnly<MapData>()
                );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var mapEntity = GetSingletonEntity<MapTiles>();
            var map = EntityManager.GetBuffer<MapTiles>(mapEntity);
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            inputDeps = Entities
                .WithReadOnly(map)
                .ForEach((ref Position pos, in PlayerInput input) =>
            {
                if (input.movement.x == 0 && input.movement.y == 0)
                    return;

                int2 p = pos;

                p += (int2)input.movement;

                int idx = p.y * mapData.width + p.x;

                if (map[idx].value != TileType.Wall)
                {
                    pos = p;
                }
            }).Schedule(inputDeps);
            
            return inputDeps;
        }
    }
}