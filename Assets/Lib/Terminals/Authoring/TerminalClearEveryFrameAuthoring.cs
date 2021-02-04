using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

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
