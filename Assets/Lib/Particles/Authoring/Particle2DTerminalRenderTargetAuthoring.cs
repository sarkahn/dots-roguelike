using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Sark.Particles2D.Authoring
{
    public class Particle2DTerminalRenderTargetAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public GameObject Target;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var tar = conversionSystem.GetPrimaryEntity(Target);
            new Particle2DEmitterBuilder(dstManager, entity).WithTerminalRenderTarget(tar);
        }
    } 
}
