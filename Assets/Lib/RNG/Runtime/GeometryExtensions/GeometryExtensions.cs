using Unity.Mathematics;
using Sark.Common.Geometry;

namespace Sark.RNG.GeometryExtensions
{
    public static class Rect2DExtensions
    {
        public static int2 GetRandomPoint(this Rect2D rect, Random rand)
        {
            return rand.NextInt2(rect.Min, rect.Max + 1);
        }
    }
}

