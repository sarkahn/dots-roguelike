using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace RLTKTutorial.Part1_4
{
    public struct Player : IComponentData
    { }

    public class PlayerProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var p = transform.position;
            int2 pos = new int2((int)p.x, (int)p.y);

            dstManager.AddComponent<Player>(entity);
            dstManager.AddComponent<PlayerInput>(entity);
            dstManager.AddComponentData<Position>(entity, pos);
            dstManager.AddComponentData<FOVRange>(entity, FOVRange.Default);
            dstManager.AddBuffer<TilesInView>(entity);
            dstManager.AddBuffer<TilesInMemory>(entity);
        }

        private void OnDrawGizmos()
        {
            var p = transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(p, Vector3.one);
        }
    }
}