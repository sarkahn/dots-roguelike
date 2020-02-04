using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_4
{
    public class FOVSystem : JobComponentSystem
    {
        EntityQuery _mapQuery;
        EntityQuery _FOVQuery;

        protected override void OnCreate()
        {
            _mapQuery = GetEntityQuery(
                ComponentType.ReadWrite<TileBuffer>()
                );

            _FOVQuery = GetEntityQuery(
                ComponentType.ReadWrite<FOVTiles>(),
                ComponentType.ReadOnly<FOVRange>()
                );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var mapEntity = _mapQuery.GetSingletonEntity();
            var map = EntityManager.GetBuffer<TileBuffer>(mapEntity);

            inputDeps = Entities.ForEach((ref DynamicBuffer<FOVTiles> fovTiles, in FOVRange range) =>
            {

            }).Schedule(inputDeps);

            return inputDeps;
        }
    }
}