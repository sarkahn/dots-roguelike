using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DotsRogue
{
    [ConverterVersion("DebugMapAuthoring", 22)]
    public class DebugMapAuthoring : MonoBehaviour, IConvertGameObjectToEntity,
        IDeclareReferencedPrefabs
    {
        public int2 Size = new int2(20, 20);

        [System.Serializable]
        public class Spawn
        {
            public GameObject Prefab;
            public int2 Position;
        }

        public List<Spawn> Spawns = new List<Spawn>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Map>(entity);
            dstManager.AddComponentData<MapSize>(entity, Size);
            DebugMap.AddToEntity(dstManager, entity);

            var spawns = dstManager.AddBuffer<DebugMapSpawns>(entity);
            foreach(var spawn in Spawns)
            {
                spawns.Add(new DebugMapSpawns
                {
                    Position = spawn.Position,
                    Prefab = conversionSystem.GetPrimaryEntity(spawn.Prefab)
                });
            }

            int len = Size.x * Size.y;
            var blockedBuffer = dstManager.AddBuffer<MapObstaclesBuffer>(entity);
            blockedBuffer.ResizeUninitialized(len);
            unsafe
            {
                UnsafeUtility.MemClear(blockedBuffer.GetUnsafePtr(), len * UnsafeUtility.SizeOf<MapObstaclesBuffer>());
            }

            var entityBuffer = dstManager.AddBuffer<MapEntitiesBuffer>(entity);
            entityBuffer.ResizeUninitialized(len);
            unsafe
            {
                UnsafeUtility.MemClear(entityBuffer.GetUnsafePtr(), len * UnsafeUtility.SizeOf<MapEntitiesBuffer>());
            }
        } 

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            foreach (var spawn in Spawns)
                referencedPrefabs.Add(spawn.Prefab);
        }
    }
}
