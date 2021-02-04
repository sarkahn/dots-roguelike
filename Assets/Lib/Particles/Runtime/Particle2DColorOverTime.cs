using Unity.Entities;
using Unity.Mathematics;
using Color32 = UnityEngine.Color32;

namespace Sark.Particles2D
{
    public struct Particle2DColorOverTime : IComponentData
    {
        public Color32 beginColor;
        public Color32 endColor;
    }

    public class Particle2DColorOverTimeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((
                ref DynamicBuffer<Particle2DColor> colBuffer,
                in DynamicBuffer<Particle2DLifetime> lifetimeBuffer,
                in Particle2DColorOverTime colorOverTime) =>
            {
                var colArr = colBuffer.Reinterpret<Color32>().AsNativeArray();
                var lifeArr = lifetimeBuffer.AsNativeArray();

                for(int i = 0; i < colBuffer.Length;++i)
                {
                    colArr[i] = Color32.Lerp(
                        colorOverTime.beginColor, colorOverTime.endColor,
                        lifeArr[i].RemainingNormalized);
                }
            }).ScheduleParallel();
        }
    }

    public static class BuilderExtension
    {
        public static Particle2DEmitterBuilder WithColorOverTime(
            this Particle2DEmitterBuilder b, 
            Color32 beginColor,
            Color32 endColor)
        {
            b.em.AddComponentData(b.e, new Particle2DColorOverTime
            {
                beginColor = beginColor,
                endColor = endColor
            });

            return b;
        }
    }
}