using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    public class AttackableAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int HitPoints = 30;
        public int Defense = 0;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData<HitPoints>(entity, HitPoints);
            dstManager.AddComponentData<MaxHitPoints>(entity, HitPoints);

            dstManager.AddComponentData<Defense>(entity, Defense);
            dstManager.AddBuffer<CombatDefendBuffer>(entity);
        }
    } 
}
