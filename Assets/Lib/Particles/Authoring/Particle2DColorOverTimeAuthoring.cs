using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Sark.Particles2D.Authoring
{
    public class Particle2DColorOverTimeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Color BeginColor = Color.red;
        public Color EndColor = Color.blue;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            new Particle2DEmitterBuilder(dstManager, entity).
                WithColorOverTime(BeginColor, EndColor);
        }
    } 
}
