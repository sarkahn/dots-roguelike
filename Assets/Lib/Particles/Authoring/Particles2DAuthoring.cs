using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Sark.Particles2D.Authoring
{
    public class Particles2DAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //var buffer = dstManager.AddBuffer<Particle2DBuffer>(entity);
        }
    } 
}
