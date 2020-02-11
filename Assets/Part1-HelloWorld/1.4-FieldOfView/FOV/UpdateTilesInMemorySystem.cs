using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;


namespace RLTKTutorial.Part1_4
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(RenderSystem))]
    public class UpdateTilesInMemorySystem : JobComponentSystem
    {
        EntityQuery _memoryQuery;
        EntityQuery _mapQuery;

        protected override void OnCreate()
        {
            _memoryQuery = GetEntityQuery(
                ComponentType.ReadOnly<TilesInView>(),
                ComponentType.ReadWrite<TilesInMemory>()
                );

            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<MapData>(),
                ComponentType.Exclude<GenerateMap>()
                );

            _memoryQuery.AddChangedVersionFilter(
                ComponentType.ReadOnly<TilesInView>()
                );
            
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_memoryQuery.CalculateEntityCount() == 0 || _mapQuery.CalculateEntityCount() == 0)
                return inputDeps;
            
            var mapEntity = _mapQuery.GetSingletonEntity();
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            // Early out during map generation
            if ( EntityManager.HasComponent<GenerateMap>(mapEntity))
                return inputDeps;

            var memoryEntity = _memoryQuery.GetSingletonEntity();
            var fovTiles = EntityManager.GetBuffer<TilesInView>(memoryEntity);
            var memory = EntityManager.GetBuffer<TilesInMemory>(memoryEntity);
            
            inputDeps = Job
                .WithReadOnly(fovTiles)
                .WithCode(() =>
            {
                for( int i = 0; i < fovTiles.Length; ++i )
                {
                    var p = fovTiles[i].value;
                    int idx = p.y * mapData.width + p.x;
                    memory[idx] = true;
                }
            }).Schedule(inputDeps);

            return inputDeps;
        }
    }
}