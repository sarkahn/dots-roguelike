using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RLTKTutorial.Part1_3
{
    [DisableAutoCreation]
    public class ResizeMapInputSystem : JobComponentSystem
    {
        TutorialControls _controls;
        InputAction _resizeMapAction;

        EntityQuery _mapQuery;
        EntityQuery _inputQuery;

        EndSimulationEntityCommandBufferSystem _barrierSystem;

        protected override void OnCreate()
        {
            _controls = new TutorialControls();
            _controls.Enable();

            _resizeMapAction = _controls.DefaultMapping.ResizeMap;

            _mapQuery = GetEntityQuery(
                ComponentType.ReadWrite<MapData>(),
                ComponentType.ReadOnly<TileBuffer>()
                );

            _inputQuery = GetEntityQuery(
                ComponentType.ReadOnly<PlayerInput>()
                );

            _barrierSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float2 resize = _resizeMapAction.triggered ?
                (float2)_resizeMapAction.ReadValue<Vector2>() : float2.zero;

            if (math.lengthsq(resize) == 0)
                return inputDeps;

            var mapEntity = _mapQuery.GetSingletonEntity();
            var mapDataFromEntity = GetComponentDataFromEntity<MapData>(false);

            var commandBuffer = _barrierSystem.CreateCommandBuffer();

            inputDeps = Job.WithCode(() =>
            {
                var mapData = mapDataFromEntity[mapEntity];
                mapData.width += (int)resize.x;
                mapData.height += (int)resize.y;

                mapData.width = math.max(15, mapData.width);
                mapData.height = math.max(15, mapData.height);
                mapDataFromEntity[mapEntity] = mapData;

                var genData = GenerateMap.Default;
                commandBuffer.AddComponent(mapEntity, genData);
            }).Schedule(inputDeps);

            _barrierSystem.AddJobHandleForProducer(inputDeps);
  
            return inputDeps;
        }
    }
}