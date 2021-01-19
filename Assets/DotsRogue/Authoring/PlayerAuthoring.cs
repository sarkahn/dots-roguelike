using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    [ConverterVersion("Why", 5)]
    public class PlayerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Player>(entity);
        }
    } 
}
