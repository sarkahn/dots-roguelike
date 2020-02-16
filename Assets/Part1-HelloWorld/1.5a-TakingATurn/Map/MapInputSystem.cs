
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    public class MapInputSystem : JobComponentSystem
    {
        TutorialControls _controls;

        EndSimulationEntityCommandBufferSystem _barrierSystem;
        
        EntityQuery _mapQuery;

        InputAction _generateMapAction;
        InputAction _resizeMapAction;

        float2 _prevResize;

        protected override void OnCreate()
        {
            _barrierSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<MapInput>(),
                ComponentType.ReadWrite<MapData>()
                );

            _controls = new TutorialControls();
            _controls.Enable();
            
            _generateMapAction = _controls.DefaultMapping.GenerateMap;
            _resizeMapAction = _controls.DefaultMapping.ResizeMap;

            RequireForUpdate(_mapQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var resize = _resizeMapAction.triggered ? (float2)_resizeMapAction.ReadValue<Vector2>() : float2.zero;
            var generateMap = _generateMapAction.triggered;

            // Early out on repeated inputs
            if (resize.x == _prevResize.x && resize.y == _prevResize.y &&
                generateMap == false)
                return inputDeps;
            _prevResize = resize;

            var mapEntity = _mapQuery.GetSingletonEntity();
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            var commandBuffer = _barrierSystem.CreateCommandBuffer();

            inputDeps = Job
                .WithCode(() =>
            {
                var genData = GenerateMap.Default;
                commandBuffer.AddComponent(mapEntity, genData);
                
                mapData.width += (int)resize.x;
                mapData.height += (int)resize.y;

                mapData.width = math.max(15, mapData.width);
                mapData.height = math.max(15, mapData.height);

                commandBuffer.SetComponent(mapEntity, mapData);
            }).Schedule(inputDeps);

            _barrierSystem.AddJobHandleForProducer(inputDeps);
            
            return inputDeps;
        }
    }
}