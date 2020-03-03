
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
    [UpdateBefore(typeof(GenerateMapSystem))]
    public class MapInputSystem : SystemBase
    {
        EntityQuery _mapQuery;
        EntityQuery _monstersQuery;

        EndSimulationEntityCommandBufferSystem _endSimBarrier;

        TutorialControls _controls;
        InputAction _generateMapAction;
        InputAction _resizeMapAction;
        InputAction _changeMonsterCount;
        InputAction _changeMonsterCountLarge;

        protected override void OnCreate()
        {
            _endSimBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _mapQuery = GetEntityQuery(
                ComponentType.ReadWrite<MapData>(),
                // Don't update if the map is generating
                ComponentType.Exclude<GenerateMap>(),
                ComponentType.Exclude<ChangeMonsterCount>()
                );
            RequireForUpdate(_mapQuery);

            _monstersQuery = GetEntityQuery(ComponentType.ReadOnly<Monster>());

            _controls = new TutorialControls();
            _controls.Enable();
            
            _generateMapAction = _controls.DefaultMapping.GenerateMap;
            _resizeMapAction = _controls.DefaultMapping.ResizeMap;
            _changeMonsterCount = _controls.DefaultMapping.ChangeMonsterCount;
            _changeMonsterCountLarge = _controls.DefaultMapping.ChangeMonsterCountLarge;
        }

        protected override void OnUpdate()
        {
            var resize = _resizeMapAction.triggered ? (float2)_resizeMapAction.ReadValue<Vector2>() : float2.zero;

            int changeMonsterCount = 0;
            if( _changeMonsterCount.triggered )
                changeMonsterCount = (int)_changeMonsterCount.ReadValue<float>() * 10;

            if (_changeMonsterCountLarge.triggered)
                changeMonsterCount = (int)_changeMonsterCountLarge.ReadValue<float>() * 100;
           
            var mapEntity = _mapQuery.GetSingletonEntity();

            if( _resizeMapAction.triggered)
                Generate(mapEntity, resize);

            if( changeMonsterCount != 0 )
                ChangeMonsterCount(mapEntity, changeMonsterCount);
        }

        void Generate(Entity mapEntity, float2 resize)
        {
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

            int monsterCount = _monstersQuery.CalculateEntityCount();
            var commandBuffer = _endSimBarrier.CreateCommandBuffer();

            var genData = GenerateMap.Default;
            genData.monsterCount = monsterCount;
            commandBuffer.AddComponent(mapEntity, genData);

            mapData.width += (int)resize.x;
            mapData.height += (int)resize.y;

            mapData.width = math.max(15, mapData.width);
            mapData.height = math.max(15, mapData.height);

            commandBuffer.SetComponent(mapEntity, mapData);
        }

        void ChangeMonsterCount(Entity mapEntity, int change)
        {
            var monsterCount = _monstersQuery.CalculateEntityCount();
            var commandBuffer = _endSimBarrier.CreateCommandBuffer();

            commandBuffer.AddComponent(mapEntity, new ChangeMonsterCount
            {
                value = math.max(0, monsterCount + change)
            }); 
        }
    }
}