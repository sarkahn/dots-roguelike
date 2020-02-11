using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_5
{
    
    public struct Renderable : IComponentData
    {
        public byte glyph;
        public Color fgColor;
        public Color bgColor;

        public Renderable( char c, Color fg = default, Color bg = default )
        {
            fgColor = fg == default ? Color.white : fg;
            bgColor = bg == default ? Color.black : bg;
            glyph = RLTK.CodePage437.ToCP437(c);
        }
    }

    public class RenderableAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public char _glyph;
        public Color _fgColor = Color.white;
        public Color _bgColor = Color.black;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Renderable
            {
                glyph = RLTK.CodePage437.ToCP437(_glyph),
                fgColor = _fgColor,
                bgColor = _bgColor,
            });
        }
    }
}