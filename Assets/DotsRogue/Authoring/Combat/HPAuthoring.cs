using Unity.Entities;
using UnityEngine;


namespace DotsRogue
{
    [DisallowMultipleComponent]
    public class HPAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int HitPoints = 30;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData<HitPoints>(entity, HitPoints);
            dstManager.AddComponentData<MaxHitPoints>(entity, HitPoints);
        }
    } 
}
