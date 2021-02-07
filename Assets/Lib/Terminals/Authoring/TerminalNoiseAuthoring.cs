using Unity.Entities;
using UnityEngine;

using Sark.Terminals.Utility;

namespace Sark.Terminals.Authoring
{
    public class TerminalNoiseAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public bool Once = false;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (Once)
                dstManager.AddComponent<TerminalNoiseOnce>(entity);
            else
                dstManager.AddComponent<TerminalNoiseEveryFrame>(entity);
        }
    } 
}
