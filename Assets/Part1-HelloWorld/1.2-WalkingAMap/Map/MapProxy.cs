
using RLTKTutorial.Game;
using System.Runtime.CompilerServices;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RLTKTutorial.Part3.Map
{
    public struct MapData : IComponentData
    {
        public int width;
        public int height;
        public uint seed;
    }

    public class MapProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        int _width = 80;
        [SerializeField]
        int _height = 50;

        [SerializeField]
        uint _seed = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int At(int x, int y) => y * _width + x;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new MapData
            {
                width = _width,
                height = _height,
                seed = _seed == 0 ? (uint)Random.Range(1, int.MaxValue) : _seed
            });

            var buffer = dstManager.AddBuffer<TileBuffer>(entity);

        }

        private void OnDrawGizmos()
        {
            Rect r = new Rect(0, 0, _width, _height);
            r.position = -new Vector2(_width, _height) * .5f;

            Vector3 pos = r.center;
            Vector3 size = new Vector3(_width, _height, 1);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(pos, size);
        }
    }

}