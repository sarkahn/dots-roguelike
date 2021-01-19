using Unity.Entities;
using UnityEngine;

using Sark.RNG;

namespace DotsRogue
{
    public class AttackerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public DiceValue AttackDice;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            // TODO: This should be based on equipped weapon and stats obviously
            dstManager.AddComponentData<AttackPower>(entity, AttackDice);
        }
    } 
}
