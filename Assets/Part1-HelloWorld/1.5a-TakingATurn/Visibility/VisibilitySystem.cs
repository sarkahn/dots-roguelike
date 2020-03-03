using RLTK.FieldOfView.BeveledCorners;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    [UpdateAfter(typeof(GenerateMapSystem))]
    public class VisibilitySystem : SystemBase
    {
        EntityQuery _mapQuery;

        protected override void OnCreate()
        {
            base.OnCreate();

            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<MapTiles>(),
                ComponentType.ReadOnly<MapData>()
                );
            
            RequireForUpdate(_mapQuery);
        }

        protected override void OnUpdate()
        {
            var mapEntity = _mapQuery.GetSingletonEntity();
            var map = EntityManager.GetBuffer<MapTiles>(mapEntity);
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            if (map.Length == 0 )
                return;

            bool mapRegenerated = EntityManager.HasComponent<GenerateMap>(mapEntity);
            
            ProcessViewMemoryEntities(mapRegenerated, map, mapData);

            ProcessViewEntities(mapRegenerated, map, mapData);
        }

        void ProcessViewEntities(bool mapRegenerated, DynamicBuffer<MapTiles> map, MapData mapData)
        {
            // Process Entities with only a view and no memory
            Entities
                .WithChangeFilter<Position>()
                .WithReadOnly(map)
                .WithNone<TilesInMemory>()
                .ForEach((ref DynamicBuffer<TilesInView> view, in Position pos, in ViewRange range) =>
                {
                    if (view.Length != map.Length || mapRegenerated)
                        view.ResizeUninitialized(map.Length);

                    var viewArray = view.Reinterpret<bool>().AsNativeArray();

                    // Always reset view before rebuilding
                    for (int i = 0; i < view.Length; ++i)
                        viewArray[i] = false;

                    var visibility = new VisibilityMap(
                        mapData.width,
                        mapData.height,
                        map.Reinterpret<TileType>().AsNativeArray(),
                        viewArray
                        );

                    FOV.Compute(pos, range, visibility);
                }).ScheduleParallel();
        }

        void ProcessViewMemoryEntities(bool mapRegenerated, DynamicBuffer<MapTiles> map, MapData mapData)
        {
            // Process Entities with both a view and a memory
            Entities
                .WithChangeFilter<Position>()
                .WithReadOnly(map)
                .ForEach((
                ref DynamicBuffer<TilesInView> view, ref DynamicBuffer<TilesInMemory> memory,
                in Position pos, in ViewRange range) =>
                {
                    if (memory.Length != map.Length || mapRegenerated)
                    {
                        memory.ResizeUninitialized(map.Length);
                        for (int i = 0; i < memory.Length; ++i)
                            memory[i] = false;
                    }

                    if (view.Length != map.Length || mapRegenerated)
                        view.ResizeUninitialized(map.Length);

                    var viewArray = view.Reinterpret<bool>().AsNativeArray();

                    // Always reset view before rebuilding
                    for (int i = 0; i < view.Length; ++i)
                        viewArray[i] = false;

                    var visibility = new VisibilityMap(
                        mapData.width, mapData.height,
                        map.Reinterpret<TileType>().AsNativeArray(),
                        viewArray,
                        memory.Reinterpret<bool>().AsNativeArray()
                        );

                    FOV.Compute(pos, range, visibility);
                }).ScheduleParallel();
        }
    }
}