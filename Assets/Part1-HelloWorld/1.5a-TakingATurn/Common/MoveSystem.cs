
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
    public class MoveSystem : SystemBase
    {
        EntityQuery _mapQuery;
        EntityQuery _moveQuery;

        Entity _mapEntity;
        DynamicBuffer<MapTiles> _map;
        MapData _mapData;

        protected override void OnCreate()
        {
            _mapQuery = GetEntityQuery(
                ComponentType.ReadWrite<MapTiles>(),
                ComponentType.ReadOnly<MapData>()
                );

            _moveQuery = GetEntityQuery(
                ComponentType.ReadWrite<Position>(),
                ComponentType.ReadOnly<Movement>()
                );
            
            _moveQuery.AddChangedVersionFilter(typeof(Movement));
        }

        public void OnFrameBegin()
        {
            _mapEntity = _mapQuery.GetSingletonEntity();
            _map = EntityManager.GetBuffer<MapTiles>(_mapEntity);
            _mapData = EntityManager.GetComponentData<MapData>(_mapEntity);
        }

        public void TryMove(Entity e, int2 move)
        {
            if (move.x == 0 && move.y == 0)
                return;

            int2 p = EntityManager.GetComponentData<Position>(e);

            int2 dest = p + move;
            int index = dest.y * _mapData.width + dest.x;

            if (index < 0 || index >= _map.Length)
                return;

            if (_map[index] != TileType.Wall)
                p = dest;

            EntityManager.SetComponentData<Position>(e, p);
        }

        protected override void OnUpdate()
        {
        }
    }
}