using Unity.Entities;
using UnityEngine;

using Sark.Terminals.Utility;

namespace Sark.Terminals.Authoring
{
    public class TerminalBorderOnCreateAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new TerminalAddBorder { });
        }
    } 
}
