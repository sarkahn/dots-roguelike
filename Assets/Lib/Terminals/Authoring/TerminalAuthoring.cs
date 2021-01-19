using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Sark.Terminals.Authoring
{
    [ExecuteAlways]
    [SelectionBase]
    [ConverterVersion("TerminalAuthoring", 1)]
    public class TerminalAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int2 Size = new int2(48, 25);

        public float2 TileSize = 1;

        [SerializeField]
        bool _snapToGrid;

        public enum Snap
        {
            None,
            Pixel,
            Grid
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (_snapToGrid)
            {
                float3 pos = transform.position;
                pos = TerminalUtility.PositionSnap(pos, Size, TileSize);
                dstManager.AddComponentData<Translation>(entity, new Translation
                {
                    Value = pos
                });
            }
            Terminal.AddToEntity(dstManager, entity, Size.x, Size.y, TileSize.x, TileSize.y);
        }
        static Color ColorFromName(string name)
        {
            var hash = CalculateHash(name);
            UnityEngine.Random.InitState((int)hash);
            return UnityEngine.Random.ColorHSV(0, 1, .15f, .85f);
        }

        static ulong CalculateHash(string read)
        {
            ulong hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }

        private void OnDrawGizmos()
        {
            float3 center = transform.position;
            float3 size = new float3(Size * TileSize, .1f);
            var col = ColorFromName(name);

            Gizmos.color = col;
            Gizmos.DrawCube(center, size);
        }
    } 
}
