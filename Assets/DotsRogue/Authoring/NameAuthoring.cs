using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    public class NameAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        string _name = "Jornathon";

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData<Name>(entity, _name);
        }
    } 
}
