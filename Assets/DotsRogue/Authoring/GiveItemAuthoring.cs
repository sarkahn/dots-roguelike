using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    public class GiveItemAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [System.Serializable]
        public struct Give
        {
            public GameObject prefab;
            public int amount;
        }

        public List<Give> GiveItems;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            foreach (var giveItem in GiveItems)
            {
                var item = conversionSystem.GetPrimaryEntity(giveItem.prefab);
                var give = conversionSystem.CreateAdditionalEntity(gameObject);
                dstManager.AddComponentData(give, new GiveItem
                {
                    item = item,
                    amount = giveItem.amount,
                    receiver = entity
                });

#if UNITY_EDITOR
                var name = dstManager.GetName(item);
                dstManager.SetName(give, $"Give {name}");
#endif
            }
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            foreach (var pair in GiveItems)
                referencedPrefabs.Add(pair.prefab);
        }
    } 
}
