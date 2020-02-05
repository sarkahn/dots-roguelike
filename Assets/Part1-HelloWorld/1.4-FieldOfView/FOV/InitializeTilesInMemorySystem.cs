using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace RLTKTutorial.Part1_4
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(UpdateTilesInMemorySystem))]
    [UpdateAfter(typeof(GenerateMapSystem))]
    public class InitializeTilesInMemorySystem : JobComponentSystem
    {
        EntityQuery _mapQuery;
        EntityQuery _memoryQuery;

        protected override void OnCreate()
        {
            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<GenerateMap>(),
                ComponentType.ReadOnly<MapData>()
                );

            _memoryQuery = GetEntityQuery(
                ComponentType.ReadWrite<TilesInMemory>()
                );

            RequireForUpdate(_mapQuery);
            RequireForUpdate(_memoryQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var mapEntity = _mapQuery.GetSingletonEntity();
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            var memoryEntity = _memoryQuery.GetSingletonEntity();
            var memory = EntityManager.GetBuffer<TilesInMemory>(memoryEntity);

            inputDeps = Job.WithCode(() =>
            {
                memory.ResizeUninitialized(mapData.width * mapData.height);
                for (int i = 0; i < memory.Length; ++i)
                    memory[i] = false;
            }).Schedule(inputDeps);

            return inputDeps;
        }
    }
}