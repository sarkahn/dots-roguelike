using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Sark.Particles2D.Authoring
{
    public class Particle2DEmitterAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int MaxParticles = 15;
        public float Lifetime = 5;
        public int EmissionRate = 5;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            Particle2DUtility.AddParticleEmitterComponents(dstManager, entity)
                .WithEmissionRate(EmissionRate)
                .WithParticleLifetime(Lifetime)
                .WithMaxParticles(MaxParticles);
        }
    } 
}
