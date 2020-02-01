
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
        EntityQuery _playerQuery;

        protected override void OnCreate()
        {
            _mapQuery = GetEntityQuery(
                ComponentType.ReadWrite<TileBuffer>(),
                ComponentType.ReadOnly<MapData>()
                );

            _playerQuery = GetEntityQuery(
                ComponentType.ReadWrite<Position>(),
                ComponentType.ReadOnly<PlayerInput>(),
                ComponentType.ReadOnly<Player>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            var playerEntity = _playerQuery.GetSingletonEntity();
            var inputFromEntity = GetComponentDataFromEntity<PlayerInput>(false);
            var posFromEntity = GetComponentDataFromEntity<Position>(false);

            var mapEntity = GetSingletonEntity<TileBuffer>();
            var mapFromEntity = GetBufferFromEntity<TileBuffer>(false);
            var mapDataFromEntity = GetComponentDataFromEntity<MapData>();

            inputDeps = Job.WithCode(() =>
            {
                var input = inputFromEntity[playerEntity];
                var hor = input.movement.x;
                var ver = input.movement.y;

                var pos = posFromEntity[playerEntity];
                int2 p = pos;

                p.x += (int)hor;
                p.y += (int)ver;

                var map = mapFromEntity[mapEntity];
                var mapData = mapDataFromEntity[mapEntity];

                int idx = p.y * mapData.width + p.x;

                if (map[idx].value != TileType.Wall)
                {
                    posFromEntity[playerEntity] = p;
                }
            }).Schedule(inputDeps);
            
            //inputDeps = Entities
            //    .WithNativeDisableParallelForRestriction(posFromEntity)
            //    .WithNativeDisableParallelForRestriction(inputFromEntity)
            //    .ForEach((ref DynamicBuffer<TileBuffer> map, in MapData mapData) =>
            //    {
            //        var input = inputFromEntity[playerEntity];
            //        var hor = input.movement.x;
            //        var ver = input.movement.y;

            //        var pos = posFromEntity[playerEntity];
            //        int2 p = pos;

            //        p.x += (int)hor;
            //        p.y += (int)ver;

            //        int idx = p.y * mapData.width + p.x;

            //        if (map[idx].value != TileType.Wall)
            //        {
            //            posFromEntity[playerEntity] = p;
            //        }
            //        // Reset input
            //        inputFromEntity[playerEntity] = default;
            //    }).Schedule(inputDeps);
    
            return inputDeps;
        }
    }
}