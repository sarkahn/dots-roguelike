using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Sark.Terminals.Rendering
{
    public static class TerminalRendererUtility
    {
        public static void AddRenderMeshRenderer(EntityManager em, Entity e, Material mat = default)
        {
            mat = mat == default ? Resources.Load<Material>("Terminal8x8") : mat;
            em.AddComponents(e, _meshDataTypes);

            var mesh = new Mesh();
            mesh.MarkDynamic();
            RenderMeshUtility.AddComponents(e, em, new RenderMeshDescription
            {

                RenderMesh = new RenderMesh
                {
                    mesh = mesh,
                    material = mat
                }
            });
        }

        public static void AddMeshDataComponents(EntityManager em, Entity e)
        {
            em.AddComponents(e, _meshDataTypes);
        }

        static readonly ComponentTypes _meshDataTypes = new ComponentTypes(new ComponentType[]
        {
            ComponentType.ReadOnly<TerminalMeshDataTiles>(),
            ComponentType.ReadOnly<TerminalMeshDataIndices>(),
            ComponentType.ReadOnly<TerminalMeshDataVertices>()
        });
    }

    public static class TerminalRendererBuilderExtensions
    {
        public static TerminalBuilder WithRenderMeshRenderer(this TerminalBuilder builder, Material mat = default)
        {
            TerminalRendererUtility.AddRenderMeshRenderer(builder.em, builder.Entity, mat);
            return builder;
        }
    } 
}
