using Unity.Entities;
using UnityEngine;

namespace DotsRogue
{
    public class GameStateAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new GameState
            {
                currentState = ComponentType.ReadOnly<GameStateTakingTurns>()
            });
            dstManager.AddComponent<GameStateTakingTurns>(entity);
        }
    } 
}
