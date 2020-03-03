using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace RLTKTutorial.Part1_5A
{
    public struct Player : IComponentData
    {
        public bool takingTurn;
    }

    public class PlayerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        bool _fovEnabled = true;

        [SerializeField]
        public int _speed = 25;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var p = transform.position;
            int2 pos = new int2((int)p.x, (int)p.y);

            dstManager.AddComponent<Player>(entity);
            dstManager.AddComponentData<Position>(entity, pos);
            dstManager.AddComponentData<Name>(entity, new FixedString32("Player"));
            dstManager.AddComponent<Energy>(entity);
            dstManager.AddComponent<Actor>(entity);

            dstManager.AddComponentData<Speed>(entity, _speed);

            dstManager.AddComponent<Collidable>(entity);

            if( _fovEnabled )
            {
                dstManager.AddBuffer<TilesInView>(entity);
                dstManager.AddBuffer<TilesInMemory>(entity);
                dstManager.AddComponentData<ViewRange>(entity, ViewRange.Default);
            }
        }

        private void OnDrawGizmos()
        {
            var p = transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(p, Vector3.one);
        }
    }
}