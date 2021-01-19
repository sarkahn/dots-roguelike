using DotsRogue;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TESTS
{

    public class NameStringTest : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject PlayerPrefab;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var p = conversionSystem.GetPrimaryEntity(PlayerPrefab);
            dstManager.AddComponentData(entity, new PlayerSpawner
            {
                prefab = p
            });
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(PlayerPrefab);
        }
    }

    struct PlayerSpawner : IComponentData
    {
        public Entity prefab;
    }

    public class NAmeStringTestSpawnPlayerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
            .WithStructuralChanges()
            .ForEach((in PlayerSpawner spawner) =>
            {
                EntityManager.Instantiate(spawner.prefab);
            }).Run();
            Enabled = false;
        }
    }

    [DisableAutoCreation]
    public class PrintNameSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach((in Player player, in Name name) =>
                {
                    FixedString128 str = $"Len {name.Value.Length}, Player name {name.Value}. WHOA!!!!";
                    Debug.Log(str);
                }).Schedule();
        }
    }
}
