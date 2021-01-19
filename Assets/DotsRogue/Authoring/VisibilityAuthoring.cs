using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    [ConverterVersion("VisibilityConverter", 5)]
    public class VisibilityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int Range = 6;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<MapViewBuffer>(entity);
            dstManager.AddComponentData<ViewRange>(entity, Range);
        }
    } 
}
