using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Scenes;
using UnityEngine;

using Sark.Terminals;

public class SerializeTest : SystemBase
{

    const string FileName = "SerializedWorld.txt";
    string FullPath = $"{Application.persistentDataPath}/{FileName}";

    ReferencedUnityObjects _refs;

    protected override void OnUpdate()
    {
        if(Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log($"Saving world...");
            using (var writer = new StreamBinaryWriter(FullPath))
            {
                EntityManager.CompleteAllJobs();
                var q = EntityManager.CreateEntityQuery(typeof(Terminal));
                EntityManager.DestroyEntity(q);
                SerializeUtilityHybrid.Serialize(EntityManager, writer, out _refs);
                int refCount = _refs == null ? 0 : _refs.Array.Length;
                Debug.Log($"RefCount: {refCount}");
            }
        }

        if(Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Loading World...");

            var loadWorld = new World("Loading World");
            var em = loadWorld.EntityManager;
            using(var reader = new StreamBinaryReader(FullPath))
            {
                SerializeUtilityHybrid.Deserialize(em, reader, _refs);
            }

            EntityManager.CompleteAllJobs();
            EntityManager.DestroyEntity(EntityManager.UniversalQuery);

            EntityManager.MoveEntitiesFrom(em);
            loadWorld.Dispose();
        }
    }
}
