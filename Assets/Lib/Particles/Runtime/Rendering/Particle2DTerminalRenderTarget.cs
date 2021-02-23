using Sark.Terminals;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
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
            var gridToWorldFromEntity = GetComponentDataFromEntity<GridToWorld>(true);

            Entities
                .WithReadOnly(termSizeFromEntity)
                .WithReadOnly(gridToWorldFromEntity)
                .ForEach((
                in DynamicBuffer<Particle2DPosition> posBuffer,
                in DynamicBuffer<Particle2DColor> colBuffer,
                in DynamicBuffer<Particle2DGlyph> glyphBuffer,
                in Particle2DCountAlive alive,
                in Particle2DTerminalRenderTarget target) =>
            {
                int2 size = termSizeFromEntity[target];
                var tileBuffer = termFromEntity[target];
                var term = new TerminalAccessor(tileBuffer, size);
                if (term.Length == 0)
                    return;

                var gridToWorld = gridToWorldFromEntity[target];

                var positions = posBuffer.Reinterpret<float2>().AsNativeArray();
                var colors = colBuffer.Reinterpret<Color32>().AsNativeArray();
                var glyphs = glyphBuffer.Reinterpret<byte>().AsNativeArray();
                //Debug.Log("Drawing particles");
                for(int i = 0; i < alive; ++i)
                {
                    float3 worldPos = new float3(positions[i], 0);
                    int2 xy = gridToWorld.WorldToGridPosition(worldPos);//term.WorldToLocalIndex2D(worldPos);

                    if (!term.InBounds(xy))
                        continue;

                    var tile = term[xy];
                    tile.BGColor = colors[i];
                    tile.Glyph = CodePage437.ToCP437('*');
                    tile.FGColor = Color.blue;
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
