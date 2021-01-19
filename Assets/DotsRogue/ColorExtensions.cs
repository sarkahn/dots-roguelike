using Color = UnityEngine.Color;

namespace DotsRogue.ColorExtensions
{
    public static class ColorExtension
    {
        public static Color ToGreyscale(this Color c)
        {
            float grey = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
            return new Color(grey, grey, grey, c.a);
        }
    }
}