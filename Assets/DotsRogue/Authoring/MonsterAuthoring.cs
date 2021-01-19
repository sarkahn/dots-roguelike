using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    [ConverterVersion("MonsterAuthoring", 1)]
    public class MonsterAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Monster>(entity);
        }
    } 
}
