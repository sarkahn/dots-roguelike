using System.Runtime.CompilerServices;
using  Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Sark.Common
{
    // http://www.chilliant.com/rgb2hsv.html
    public static class ColorUtil
    {
        const float Epsilon = 1e-10f;
        const float HCLgamma = 3;
        const float HCLy0 = 100;
        const float HCLmaxL = 0.530454533953517f; // == exp(HCLgamma / HCLy0) - 0.5

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 RGBtoHCV(in float3 RGB)
        {
            // Based on work by Sam Hocevar and Emil Persson
            float4 P = (RGB.y < RGB.z) ? float4(RGB.zy, -1.0f, 2.0f / 3.0f) : float4(RGB.yz, 0.0f, -1.0f / 3.0f);
            float4 Q = (RGB.x < P.x) ? float4(P.xyw, RGB.x) : float4(RGB.x, P.yzx);
            float C = Q.x - min(Q.w, Q.y);
            float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
            return float3(H, C, Q.x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 RGBtoHSV(in float3 RGB)
        {
            float3 HCV = RGBtoHCV(RGB);
            float S = HCV.y / (HCV.z + Epsilon);
            return float3(HCV.x, S, HCV.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 RGBtoHCL(in float3 RGB)
        {
            float3 HCL;
            float H = 0;
            float U = min(RGB.x, min(RGB.y, RGB.z));
            float V = max(RGB.x, max(RGB.y, RGB.z));
            float Q = HCLgamma / HCLy0;
            HCL.y = V - U;
            if (HCL.y != 0)
            {
                H = atan2(RGB.y - RGB.z, RGB.x - RGB.y) / PI;
                Q *= U / V;
            }
            Q = exp(Q);
            HCL.x = frac(H / 2 - min(frac(H), frac(-H)) / 6);
            HCL.y *= Q;
            HCL.z = lerp(-U, V, Q) / (HCLmaxL * 2);
            return HCL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 HSLtoRGB(float3 HSL)
        {
            float3 RGB = HUEtoRGB(HSL.x);
            float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
            float3 L = new float3(HSL.z, HSL.z, HSL.z);
            return (RGB - new float3(0.5f, 0.5f, 0.5f)) * C + L;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 RGBtoHSL(float3 RGB)
        {
            float3 HCV = RGBtoHCV(RGB);
            float L = HCV.z - HCV.y * 0.5f;
            float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
            return new float3(HCV.x, S, L);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 HUEtoRGB(in float H)
        {
            float R = abs(H * 6 - 3) - 1;
            float G = 2 - abs(H * 6 - 2);
            float B = 2 - abs(H * 6 - 4);
            return saturate(float3(R, G, B));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 HSVtoRGB(in float3 HSV)
        {
            float3 RGB = HUEtoRGB(HSV.x);
            return ((RGB - 1) * HSV.y + 1) * HSV.z;
        }

    } 
}
