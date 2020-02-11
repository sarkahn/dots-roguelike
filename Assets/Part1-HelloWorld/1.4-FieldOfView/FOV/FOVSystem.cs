using RLTK.FieldOfView.BeveledCorners;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
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
                ComponentType.ReadOnly<MapTiles>()
                );

            _FOVQuery = GetEntityQuery(
                ComponentType.ReadWrite<TilesInView>(),
                ComponentType.ReadOnly<FOVRange>(),
                ComponentType.ReadOnly<Position>()
                );

            _FOVQuery.SetChangedVersionFilter(typeof(Position));

            RequireForUpdate(_mapQuery);
            RequireForUpdate(_FOVQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_FOVQuery.CalculateEntityCount() == 0)
                return inputDeps;

            var mapEntity = _mapQuery.GetSingletonEntity();
            var map = EntityManager.GetBuffer<MapTiles>(mapEntity);
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            var fovEntity = _FOVQuery.GetSingletonEntity();

            int2 origin = EntityManager.GetComponentData<Position>(fovEntity);
            int range = EntityManager.GetComponentData<FOVRange>(fovEntity);
            DynamicBuffer<TilesInView> fovTiles = EntityManager.GetBuffer<TilesInView>(fovEntity);

            var visibilityMap = new VisibilityMap(
               mapData.width, mapData.height, 
               map.Reinterpret<TileType>().AsNativeArray(), 
               Allocator.TempJob);

            inputDeps = Job.WithCode(() =>
            {
                FOV.Compute(origin, range, visibilityMap);

                fovTiles.Clear();

                var visibleTiles = visibilityMap.visibleTiles;

                for (int i = 0; i < visibleTiles.Length; ++i)
                    fovTiles.Add(visibleTiles[i]);
            }).Schedule(inputDeps);

            visibilityMap.Dispose(inputDeps);

            return inputDeps;
        }
    }
}