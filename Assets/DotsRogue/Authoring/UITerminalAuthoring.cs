using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    public class UITerminalAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<UI>(entity);
            dstManager.AddComponent<HitPointsUI>(entity);
            dstManager.AddBuffer<UILogBuffer>(entity);
        }
    } 
}
