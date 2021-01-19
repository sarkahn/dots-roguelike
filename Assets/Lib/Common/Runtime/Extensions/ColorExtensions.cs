using Unity.Mathematics;
using UnityEngine;

using static Sark.Common.ColorUtil;

namespace Sark.Common.ColorExtensions
{
    public static class ColorExtensions 
    {
        public static float4 ToFloat4(this Color col)
        {
            return new float4(col.r, col.g, col.b, col.a);
        }

        public static Color ToColor(this float4 v)
        {
            return new Color(v.x, v.y, v.z, v.w);
        }

        public static Color WithLightness(this Color col, float lightness)
        {
            float4 f4 = col.ToFloat4();
            float3 hsl = RGBtoHSL(f4.xyz);
            hsl.z = lightness;
            f4.xyz = HSLtoRGB(hsl);
            return f4.ToColor();
        }

        public static Color WithSaturation(this Color col, float saturation)
        {
            float4 v = col.ToFloat4();
            float3 hsv = RGBtoHSV(v.xyz);
            hsv.y = saturation;
            v.xyz = HSVtoRGB(hsv);
            return v.ToColor();
        }
    } 
}
