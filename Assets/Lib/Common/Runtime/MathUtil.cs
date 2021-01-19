using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Sark.Common
{
    public static class MathUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int mod(int x, int period)
        {
            return ((x % period) + period) % period;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 mod(int2 xy, int2 period)
        {
            return ((xy % period) + period) % period;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 mod(int3 xyz, int3 period)
        {
            return ((xyz % period) + period) % period;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float roundedincrement(float value, float increments)
        {
            increments = 1f / increments;
            return math.round(value * increments) / increments;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 roundedincrement(float2 v, float2 increments)
        {
            increments = new float2(1, 1) / increments;
            return math.round(v * increments) / increments;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 roundedincrement(float3 value, float3 increments)
        {
            increments = new float3(1) / increments;
            return math.round(value * increments) / increments;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float flooredincrement(float value, float increments)
        {
            increments = 1f / increments;
            return math.floor(value * increments) / increments;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 flooredincrement(float2 v, float2 increments)
        {
            increments = new float2(1) / increments;
            return math.floor(v * increments) / increments;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 flooredincrement(float3 value, float3 increments)
        {
            increments = new float3(1) / increments;
            return math.floor(value * increments) / increments;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int convertrange(
            int value,
            int originalStart, int originalEnd,
            int newStart, int newEnd )
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (int)(newStart + ((value - originalStart) * scale));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float convertrange(
            float value,
            float originalStart, float originalEnd, // original range
            float newStart, float newEnd) // value to convert
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (float)(newStart + ((value - originalStart) * scale));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float repeat(float t, float len)
        {
            return math.clamp(t - math.floor(t / len) * len, 0, len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 repeat(float2 t, float2 len)
        {
            return math.clamp(t - math.floor(t / len) * len, 0, len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 repeat(float3 t, float3 len)
        {
            return math.clamp(t - math.floor(t / len) * len, 0, len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float pingpong(float t, float len)
        {
            t = repeat(t, len * 2);
            return len - math.abs(t - len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 pingpong(float2 t, float2 len)
        {
            t = repeat(t, len * 2);
            return len - math.abs(t - len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 pingpong(float3 t, float3 len)
        {
            t = repeat(t, len * 2);
            return len - math.abs(t - len);
        }
    }
}