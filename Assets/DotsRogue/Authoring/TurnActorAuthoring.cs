using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    [ConverterVersion("TurnActorAuthoring", 3)]
    public class TurnActorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int Speed = 20;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Actor>(entity);
            dstManager.AddComponent<Energy>(entity);
            dstManager.AddComponentData<Speed>(entity, Speed);
        }
    } 
}
