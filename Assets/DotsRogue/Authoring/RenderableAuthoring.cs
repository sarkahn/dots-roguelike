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

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            Renderable.AddToEntity(dstManager, entity, Glyph, ConvertColor(FGColor), ConvertColor(BGColor));
        }


        TinyColor ConvertColor(UnityColor c)
        {
            return new TinyColor(c.r, c.g, c.b, c.a);
        }
    } 
}
