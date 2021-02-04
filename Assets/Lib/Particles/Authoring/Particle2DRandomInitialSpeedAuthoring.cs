using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Sark.Particles2D.Authoring
{
    public class Particle2DRandomInitialSpeedAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float Min = 5; 
        public float Max = 15;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            new Particle2DEmitterBuilder(dstManager, entity).WithRandomInitialSpeed(
                Min, Max);
        }
    } 
}
