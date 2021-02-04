using Sark.Terminals;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Color32 = UnityEngine.Color32;

namespace Sark.Particles2D
{
    public struct Particle2DTerminalRenderTarget : IComponentData
    {
        public Entity value;
        public static implicit operator Entity(Particle2DTerminalRenderTarget b) => b.value;
        public static implicit operator Particle2DTerminalRenderTarget(Entity v) =>
            new Particle2DTerminalRenderTarget { value = v };
    }

    //[UpdateInGroup(typeof(PresentationSystemGroup))]
    //[UpdateBefore(typeof(TerminalRenderSystem))]
    public class Particle2DTerminalRendererSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var termFromEntity = GetBufferFromEntity<TerminalTilesBuffer>(false);
            var termSizeFromEntity = GetComponentDataFromEntity<TerminalSize>(true);
            var posFromEntity = GetComponentDataFromEntity<Translation>(true);

            Entities
                .WithReadOnly(termSizeFromEntity)
                .WithReadOnly(posFromEntity)
                .ForEach((
                in DynamicBuffer<Particle2DPosition> posBuffer,
                in DynamicBuffer<Particle2DColor> colBuffer,
                in DynamicBuffer<Particle2DGlyph> glyphBuffer,
                in Particle2DCountAlive alive,
                in Particle2DTerminalRenderTarget target) =>
            {
                int2 size = termSizeFromEntity[target];
                var tileBuffer = termFromEntity[target];
                var term = new TerminalAccessor(tileBuffer, size, posFromEntity[target].Value);
                if (term.Length == 0)
                    return;

                var positions = posBuffer.Reinterpret<float2>().AsNativeArray();
                var colors = colBuffer.Reinterpret<Color32>().AsNativeArray();
                var glyphs = glyphBuffer.Reinterpret<byte>().AsNativeArray();

                for(int i = 0; i < alive; ++i)
                {
                    float2 worldPos = positions[i];
                    int2 xy = term.WorldToLocalIndex2D(worldPos);

                    if (!term.InBounds(xy))
                        continue;

                    var tile = term[xy];
                    tile.BGColor = colors[i];
                    term[xy] = tile;
                }
            }).Schedule();
        }
    }

    public static class AAAA
    {
        public static Particle2DEmitterBuilder WithTerminalRenderTarget(
            this Particle2DEmitterBuilder b,
            Entity target)
        {
            b.em.AddComponentData<Particle2DTerminalRenderTarget>(b.e, target);
            return b;
        }
    }
}
