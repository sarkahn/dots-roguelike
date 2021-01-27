using Unity.Entities;
using UnityEngine;

using static Sark.Terminals.CodePage437;
using UnityColor = UnityEngine.Color;
using TinyColor = UnityEngine.Color;

namespace DotsRogue.Authoring
{
    [ConverterVersion("AAAAHAHAHHAH", 3)]
    public class RenderableAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public char Glyph;
        public UnityColor FGColor = UnityColor.white;
        public UnityColor BGColor = UnityColor.black;

        [Tooltip("An optional authoring tile to use for rendering. If set, this will override inspector settings.")]
        public MapTileAuthoring Tile = null;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if(Tile != null)
            {
                dstManager.AddComponentData(entity, new Renderable
                {
                    Glyph = ToCP437(Glyph),
                    FGColor = ConvertColor(FGColor),
                    BGColor = ConvertColor(BGColor)
                });
            }

            dstManager.AddComponentData(entity, new Renderable
            {
                Glyph = ToCP437(Glyph),
                FGColor = ConvertColor(FGColor),
                BGColor = ConvertColor(BGColor)
            });
        }


        TinyColor ConvertColor(UnityColor c)
        {
            return new TinyColor(c.r, c.g, c.b, c.a);
        }
    } 
}
