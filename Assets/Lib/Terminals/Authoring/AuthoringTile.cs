using Sark.Common.GridUtil;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Sark.Terminals
{
    public class AuthoringTile : MonoBehaviour
    {
        public Color ForegroundColor = Color.white;
        public Color BackgroundColor = Color.black;
        public char Glyph = '@';
        public ushort GlyphIndex => CodePage437.ToCP437(Glyph);

        public Tile ToTerminalTile()
        {
            return new Tile
            {
                BGColor = BackgroundColor,
                FGColor = ForegroundColor,
                Glyph = Glyph
            };
        }

        byte SpriteToGlyphIndex(Sprite sprite)
        {
            var rect = sprite.rect;
            float2 uvPos = new float2(rect.x / rect.width, rect.y / rect.height);
            uvPos.y = 15 - uvPos.y;
            return (byte)Grid2D.PosToIndex((int2)uvPos, 16);
        }
    } 
}
