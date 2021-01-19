using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsRogue
{
    [ConverterVersion("Ahhh", 12)]
    public class MapAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [SerializeField]
        int2 _size = new int2(48, 25);

        public int2 Size => _size;

        [SerializeField]
        GameObject _playerPrefab;

        [SerializeField]
        List<GameObject> _monsterPrefabs;

        [SerializeField]
        List<GameObject> _itemPrefabs;

        [SerializeField]
        GenMapSettings _genSettings = GenMapSettings.Default;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            int len = _size.x * _size.y;
            var buffer = dstManager.AddBuffer<MapTilesBuffer>(entity);
            buffer.ResizeUninitialized(len);
            unsafe
            {
                UnsafeUtility.MemClear(buffer.GetUnsafePtr(), buffer.Length);// *
                //UnsafeUtility.SizeOf<MapTilesBuffer>());
            }

            dstManager.AddComponent<Map>(entity);
            dstManager.AddComponentData<MapSize>(entity, _size);
            //Map.AddToEntity(dstManager, entity, _size);

            var playerPrefab = _playerPrefab == null ? Entity.Null : 
                conversionSystem.GetPrimaryEntity(_playerPrefab);

            var monsterPrefabs = new NativeList<Entity>(Allocator.Temp);
            var itemPrefabs = new NativeList<Entity>(Allocator.Temp);

            foreach (var goPrefab in _monsterPrefabs)
                monsterPrefabs.Add(
                    conversionSystem.GetPrimaryEntity(goPrefab));
            foreach (var item in _itemPrefabs)
                itemPrefabs.Add(conversionSystem.GetPrimaryEntity(item));

            if (_genSettings.Seed == 0)
                _genSettings.Seed = (uint)UnityEngine.Random.Range(1, int.MaxValue);

            GenerateMap.AddToEntity(dstManager, entity,
                _genSettings,
                playerPrefab,
                monsterPrefabs,
                itemPrefabs);
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(_playerPrefab);
            referencedPrefabs.AddRange(_monsterPrefabs);
            referencedPrefabs.AddRange(_itemPrefabs);
        }

        private void OnDrawGizmosSelected()
        {
            int w = _size.x;
            int h = _size.y;
            Rect r = new Rect(0, 0, w, h);
            r.position = -new Vector2(w, h) * .5f;

            Vector3 pos = transform.position + (Vector3)r.center;
            Vector3 size = new Vector3(w, h, 1);

            Gizmos.color = Color.blue;
            Gizmos.DrawCube(pos, size);
        }

        private void Reset()
        {
            _playerPrefab = Resources.Load<GameObject>("Actors/PlayerPrefab");
        }
    }
}
