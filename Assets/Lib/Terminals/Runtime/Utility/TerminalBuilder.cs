using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Sark.Terminals.Utility;

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

        public TerminalBuilder WithPosition(float3 pos)
        {
            em.SetComponentData<Translation>(e, new Translation
            {
                Value = pos
            });

            return this;
        }

        public TerminalBuilder WithCenteredPosition(float x, float y, float z = 0)
            => WithCenteredPosition(new float3(x, y, z));
        public TerminalBuilder WithCenteredPosition(float3 pos)
        {
            float2 size = em.GetComponentData<TerminalSize>(e).Value;
            float2 tileSize = em.GetComponentData<TileSize>(e).Value;
            pos.xy -= (size * tileSize / 2);
            em.SetComponentData<Translation>(e, new Translation
            {
                Value = pos
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
