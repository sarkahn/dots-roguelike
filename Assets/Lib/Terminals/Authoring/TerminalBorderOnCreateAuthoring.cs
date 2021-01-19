using Unity.Entities;
using UnityEngine;

namespace Sark.Terminals.Authoring
{
    public class TerminalBorderOnCreateAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new TerminalBorderOnCreate { });
        }
    } 
}
