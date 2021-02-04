using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sark.Particles2D.Authoring
{
    public class Particle2DEmitterCircleAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Range(0, 360f)]
        public float Angle;
        [Range(0, 360f)]
        public float Radius;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            new Particle2DEmitterBuilder(dstManager, entity)
                .WithCircleEmitter(math.radians(Angle), math.radians(Radius));
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            float3 p = transform.position;

            float halfRadius = Radius / 2;

            float r = math.radians(Angle + halfRadius);

            float3 dir = new float3(
                math.cos(r), math.sin(r), 0);

            Handles.DrawSolidArc(p, Vector3.back, dir, Radius, HandleUtility.GetHandleSize(p));
        }
#endif
    }
}
