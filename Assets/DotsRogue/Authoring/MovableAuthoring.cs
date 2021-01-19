using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsRogue
{
    [ConverterVersion("MoveableAuthoring", 3)]
    public class MovableAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            float3 xyz = transform.position;
            int2 xy = (int2)math.floor(xyz.xy);
            dstManager.AddComponent<Movement>(entity);
            dstManager.AddComponentData<Position>(entity, xy);
        }
    } 
}
