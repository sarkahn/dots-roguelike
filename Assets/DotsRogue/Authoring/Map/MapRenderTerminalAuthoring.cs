using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    [ConverterVersion("RenderTerminal", 3)]
    public class MapRenderTerminalAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<MapRenderTerminal>(entity);
        }
    } 
}
