using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

using Sark.EntityUtils.DynamicBufferExtensions;

namespace DotsRogue
{
    [RequireComponent(typeof(MapAuthoring))]
    public class MapStateAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            int2 size = GetComponent<MapAuthoring>().Size;

            int len = size.x * size.y;
            var blockedBuffer = dstManager.AddBuffer<MapObstaclesBuffer>(entity);
            blockedBuffer.ResizeUninitialized(len);
            blockedBuffer.MemClear();

            var entityBuffer = dstManager.AddBuffer<MapEntitiesBuffer>(entity);
            entityBuffer.ResizeUninitialized(len);
            entityBuffer.MemClear();
        }
    } 
}
