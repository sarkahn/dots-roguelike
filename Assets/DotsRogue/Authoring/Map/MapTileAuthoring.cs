using UnityEngine;

using Sark.Terminals;

namespace DotsRogue.Authoring
{
    public class MapTileAuthoring : MonoBehaviour
    {
        public Color ForegroundColor = Color.white;
        public Color BackgroundColor = Color.black;
        public char Glyph = '@';
        public byte GlyphIndex => CodePage437.ToCP437(Glyph);

        public TerminalTile ToTerminalTile()
        {
            return new TerminalTile
            {
                FGColor = ForegroundColor,
                BGColor = BackgroundColor,
                Glyph = Glyph
            };
        }

        //byte SpriteToGlyphIndex(Sprite sprite)
        //{
        //    var rect = sprite.rect;
        //    float2 uvPos = new float2(rect.x / rect.width, rect.y / rect.height);
        //    uvPos.y = 15 - uvPos.y;
        //    return (byte)Grid2D.PosToIndex((int2)uvPos, 16);
        //}
    }
}