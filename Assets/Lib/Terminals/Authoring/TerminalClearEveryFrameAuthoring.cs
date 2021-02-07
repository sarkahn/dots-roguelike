using Unity.Entities;
using UnityEngine;

using Sark.Terminals.Utility;

namespace Sark.Terminals.Authoring
{
    public class TerminalClearEveryFrameAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<TerminalClearEveryFrame>(entity);
        }
    }
}
