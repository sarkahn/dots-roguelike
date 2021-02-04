using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Sark.Particles2D.Authoring
{
    public class Particle2DRandomInitialVelocityAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float2 MinVelocity;
        public float2 MaxVelocity;
        public float2 MinAcceleration;
        public float2 MaxAcceleration;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            new Particle2DEmitterBuilder(dstManager, entity).WithRandomInitialVelocity(
                MinVelocity,
                MaxVelocity,
                MinAcceleration,
                MaxAcceleration);
        }
    }
}
