using Unity.Entities;
using UnityEngine;

using Sark.RNG;

namespace DotsRogue
{
    public class ItemScrollMagicMissileAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public DiceValue DamageDice = new DiceValue(8, 2);
        public int Range = 5;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            Item.AddToEntity(dstManager, entity);
            dstManager.AddComponentData(entity, new InflictsDamage { damage = DamageDice });
            dstManager.AddComponentData(entity, new Ranged { range = Range });
            dstManager.AddComponent<Position>(entity);
            dstManager.AddComponentData(entity, new Consumable
            {
                uses = 1
            });
        }
    } 
}
