using Sark.Terminals.Rendering;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Sark.Terminals.Authoring
{
    public class TerminalRenderMeshAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Material Material;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            TerminalRendererUtility.AddRenderMeshRenderer(dstManager, entity, Material);
        }

        private void OnValidate()
        {
            if (Material == null)
            {
                Material = Resources.Load<Material>("Terminal8x8");
            }
        }
    } 
}
