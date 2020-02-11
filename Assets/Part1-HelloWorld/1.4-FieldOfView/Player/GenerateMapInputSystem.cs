
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace RLTKTutorial.Part1_4
{
    [DisableAutoCreation]
    public class GenerateMapInputSystem : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem _barrierSystem;

        EntityQuery _inputQuery;
        EntityQuery _mapQuery;

        protected override void OnCreate()
        {
            _barrierSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _inputQuery = GetEntityQuery(
                ComponentType.ReadOnly<PlayerInput>()
                );

            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<MapTiles>()
                );

            RequireForUpdate(_inputQuery);
            RequireForUpdate(_mapQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var input = _inputQuery.GetSingleton<PlayerInput>();

            var mapEntity = _mapQuery.GetSingletonEntity();

            var commandBuffer = _barrierSystem.CreateCommandBuffer();

            inputDeps = Job
                .WithoutBurst()
                .WithCode(() =>
            {
                if(input.generateNewMap)
                {
                    var genData = GenerateMap.Default;
                    commandBuffer.AddComponent(mapEntity, genData);
                }
            }).Schedule(inputDeps);

            _barrierSystem.AddJobHandleForProducer(inputDeps);
            
            return inputDeps;
        }
    }
}