using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    [ConverterVersion("MemoryConverter", 3)]
    public class MemoryAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<MapMemoryBuffer>(entity);
        }
    } 
}
