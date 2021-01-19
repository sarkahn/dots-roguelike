using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    public class ItemPotionAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int HealAmount;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            Item.AddToEntity(dstManager, entity);
            dstManager.AddComponentData(entity, new HealsOnUse
            {
                healAmount = HealAmount
            });
            dstManager.AddComponent<Position>(entity);
            dstManager.AddComponentData(entity, new Consumable
            {
                uses = 1
            });
        }
    } 
}
