using Sark.Terminals.Rendering;
using Unity.Entities;
using UnityEngine;

namespace Sark.Terminals.Authoring
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class TerminalGameObjectHybridAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var renderer = GetComponent<MeshRenderer>();
            var filter = GetComponent<MeshFilter>();

            conversionSystem.AddHybridComponent(renderer);
            conversionSystem.AddHybridComponent(filter);

            var mesh = new Mesh();
            mesh.MarkDynamic();

            filter.sharedMesh = mesh;

            TerminalRendererUtility.AddMeshDataComponents(dstManager, entity);
        }

        private void OnValidate()
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer.sharedMaterial == null)
            {
                renderer.sharedMaterial = Resources.Load<Material>("Terminal8x8");
            }
        }
    } 
}
