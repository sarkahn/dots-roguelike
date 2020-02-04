
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

        protected override void OnCreate()
        {
            _barrierSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var playerEntity = GetSingletonEntity<Player>();
            var inputFromEntity = GetComponentDataFromEntity<PlayerInput>(true);

            var mapEntity = GetSingletonEntity<TileBuffer>();

            var commandBuffer = _barrierSystem.CreateCommandBuffer();

            inputDeps = Job
                .WithoutBurst()
                .WithReadOnly(inputFromEntity)
                .WithCode(() =>
            {
                var input = inputFromEntity[playerEntity];
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