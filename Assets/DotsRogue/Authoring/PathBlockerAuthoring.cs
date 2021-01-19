using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    [ConverterVersion("PathBlockerAuthoring", 6)]
    public class PathBlockerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<PathBlocker>(entity);
        }
    } 
}
