
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
    public class MapInputSystem : SystemBase
    {
        TutorialControls _controls;

        BeginSimulationEntityCommandBufferSystem _barrierSystem;
        
        EntityQuery _mapQuery;

        EntityQuery _monstersQuery;

        InputAction _generateMapAction;
        InputAction _resizeMapAction;

        InputAction _changeMonsterCount;

        float2 _prevResize;

        protected override void OnCreate()
        {
            _barrierSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<MapInput>(),
                ComponentType.ReadWrite<MapData>()
                );

            _monstersQuery = GetEntityQuery(ComponentType.ReadOnly<Monster>());

            _controls = new TutorialControls();
            _controls.Enable();
            
            _generateMapAction = _controls.DefaultMapping.GenerateMap;
            _resizeMapAction = _controls.DefaultMapping.ResizeMap;
            _changeMonsterCount = _controls.DefaultMapping.ChangeMonsterCount;

            RequireForUpdate(_mapQuery);
        }

        protected override void OnUpdate()
        {
            var mapEntity = _mapQuery.GetSingletonEntity();

            // Early out if map is generating
            if (EntityManager.HasComponent<GenerateMap>(mapEntity) || EntityManager.HasComponent<ChangeMonsterCount>(mapEntity))
                return;

            var resize = _resizeMapAction.triggered ? (float2)_resizeMapAction.ReadValue<Vector2>() : float2.zero;
            var generateMap = _generateMapAction.triggered;

            // Ignore repeated inputs
            if ( generateMap || (resize.x != 0 || resize.y != 0) )
                Generate(mapEntity, resize);

            if(_changeMonsterCount.triggered )
            {
                float val = _changeMonsterCount.ReadValue<float>();
                if (val != 0)
                    ChangeMonsterCount(mapEntity, (int)(10 * val));
            }

        }

        void Generate(Entity mapEntity, float2 resize)
        {
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            var commandBuffer = _barrierSystem.CreateCommandBuffer();

            int monsterCount = _monstersQuery.CalculateEntityCount();

            Job.WithCode(() =>
            {
                var genData = GenerateMap.Default;
                genData.monsterCount = monsterCount;
                commandBuffer.AddComponent(mapEntity, genData);

                mapData.width += (int)resize.x;
                mapData.height += (int)resize.y;

                mapData.width = math.max(15, mapData.width);
                mapData.height = math.max(15, mapData.height);

                commandBuffer.SetComponent(mapEntity, mapData);
            }).Schedule();

            _barrierSystem.AddJobHandleForProducer(Dependency);
        }

        void ChangeMonsterCount(Entity mapEntity, int change)
        {
            var currentCount = _monstersQuery.CalculateEntityCount();
            var commandBuffer = _barrierSystem.CreateCommandBuffer();

            commandBuffer.AddComponent<ChangeMonsterCount>(mapEntity, new ChangeMonsterCount
            {
                count = math.max(0, currentCount + change)
            }); 

            _barrierSystem.AddJobHandleForProducer(Dependency);
        }
    }
}