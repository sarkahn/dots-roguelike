using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Sark.Terminals.Utility;
using Unity.Rendering;
using UnityEngine;

namespace Sark.Terminals
{
    public struct TerminalBuilder
    {
        public EntityManager em;
        public Entity e;

        public TerminalBuilder(EntityManager em, Entity e)
        {
            Assert.IsTrue(em.HasComponent<Terminal>(e));
            this.em = em;
            this.e = e;
        }

        public TerminalBuilder WithSize(int width, int height) => 
            WithSize(new int2(width, height));
        public TerminalBuilder WithSize(int2 size)
        {
            em.AddComponentData<TerminalSize>(e, size);
            return this;
        }

        public TerminalBuilder WithTileSize(float width, float height) =>
            WithTileSize(new float2(width, height));
        public TerminalBuilder WithTileSize(float2 size)
        {
            em.AddComponentData<TileSize>(e, size);
            return this;
        }

        public TerminalBuilder WithBorder()
        {
            em.AddComponent<TerminalAddBorder>(e);
            return this;
        }

        public TerminalBuilder WithPosition(float x, float y, float z = 0) =>
            WithPosition(new float3(x, y, z));
        public TerminalBuilder WithPosition(float3 pos)
        {
            em.SetComponentData<Translation>(e, new Translation
            {
                Value = pos
            });

            return this;
        }

        public TerminalBuilder WithAlignment(float xAlign, float yAlign) =>
            WithAlignment(new float2(xAlign, yAlign));
        public TerminalBuilder WithAlignment(float2 align)
        {
            float3 p = em.GetComponentData<Translation>(e).Value;
            float2 size = em.GetComponentData<TerminalSize>(e).Value;
            float2 tileSize = em.GetComponentData<TileSize>(e).Value;

            float2 halfSize = size * tileSize / 2;
            p.xy += halfSize * align;
            em.SetComponentData<Translation>(e, new Translation
            {
                Value = p
            });
            return this;
        }

        public TerminalBuilder WithClearEveryFrame()
        {
            em.AddComponent<TerminalClearEveryFrame>(e);
            return this;
        }
    } 
}
