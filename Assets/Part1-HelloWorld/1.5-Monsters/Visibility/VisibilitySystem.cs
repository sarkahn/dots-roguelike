using RLTK.FieldOfView.BeveledCorners;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_5
{
    [DisableAutoCreation]
    [UpdateAfter(typeof(GenerateMapSystem))]
    public class VisibilitySystem : JobComponentSystem
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

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var mapEntity = _mapQuery.GetSingletonEntity();
            var map = EntityManager.GetBuffer<MapTiles>(mapEntity);
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            if (map.Length == 0 )
                return inputDeps;

            bool mapRegenerated = EntityManager.HasComponent<GenerateMap>(mapEntity);
            
            inputDeps = ProcessViewMemoryEntities(mapRegenerated, map, mapData, inputDeps);

            inputDeps = ProcessViewEntities(mapRegenerated, map, mapData, inputDeps);
            
            return inputDeps;
        }

        JobHandle ProcessViewEntities(bool mapRegenerated, DynamicBuffer<MapTiles> map, MapData mapData, JobHandle inputDeps)
        {
            // Process Entities with only a view and no memory
            return Entities
                .WithReadOnly(map)
                .WithChangeFilter<Position>()
                .WithNone<TilesInMemory>()
                .ForEach((ref DynamicBuffer<TilesInView> view, in Position pos, in ViewRange range) =>
                {
                    if (view.Length != map.Length || mapRegenerated)
                        view.ResizeUninitialized(map.Length);

                    // Always reset view before rebuilding
                    for (int i = 0; i < view.Length; ++i)
                        view[i] = false;

                    var visibility = new VisibilityMap(
                        mapData.width,
                        mapData.height,
                        map.Reinterpret<TileType>().AsNativeArray(),
                        view.Reinterpret<bool>().AsNativeArray()
                        );

                    FOV.Compute(pos, range, visibility);
                }).Schedule(inputDeps);
        }

        JobHandle ProcessViewMemoryEntities(bool mapRegenerated, DynamicBuffer<MapTiles> map, MapData mapData, JobHandle inputDeps)
        {
            // Process Entities with both a view and a memory
            return Entities
                .WithChangeFilter<Position>()
                .WithReadOnly(map)
                .ForEach((ref DynamicBuffer<TilesInView> view, ref DynamicBuffer<TilesInMemory> memory,
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

                    // Always reset view before rebuilding
                    for (int i = 0; i < view.Length; ++i)
                        view[i] = false;

                    var visibility = new VisibilityMap(
                        mapData.width, mapData.height,
                        map.Reinterpret<TileType>().AsNativeArray(),
                        view.Reinterpret<bool>().AsNativeArray(),
                        memory.Reinterpret<bool>().AsNativeArray()
                        );

                    FOV.Compute(pos, range, visibility);
                }).Schedule(inputDeps);
        }
    }
}