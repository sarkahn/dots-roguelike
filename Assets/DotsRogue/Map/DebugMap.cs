using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using Sark.Common.GridUtil;

namespace DotsRogue
{ 
    public struct DebugMap : IComponentData
    {
        public static void AddToEntity(EntityManager em, Entity e)
        {
            em.AddComponent<DebugMap>(e);
        }
    }

    public struct DebugMapSpawns : IBufferElementData
    {
        public Entity Prefab;
        public int2 Position;
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializeDebugMapSystem : SystemBase
    {
        EntityQuery _debugMapQuery;

        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            Entities
                .WithStoreEntityQueryInField(ref _debugMapQuery)
                .WithStructuralChanges()
                .ForEach((ref DynamicBuffer<MapTilesBuffer> tilesBuffer,
                ref MapSize sizeC,
                in DynamicBuffer<DebugMapSpawns> spawnsBuffer,
                in DebugMap debugMap) =>
            {
                int2 size = new int2(20, 20);
                sizeC = size;

                tilesBuffer.ResizeUninitialized(size.x * size.y);
                
                var map = new GridData2D<MapTile>(tilesBuffer.
                    Reinterpret<MapTile>().AsNativeArray(), size);

                for (int i = 0; i < map.Length; ++i)
                    map[i] = MapTile.Floor;

                foreach(var spawn in spawnsBuffer)
                {
                    var spawned = ecb.Instantiate(spawn.Prefab);
                    ecb.SetComponent<Position>(spawned, spawn.Position);
                }
            }).Run();

            ecb.Playback(EntityManager);
            ecb.Dispose();

            EntityManager.RemoveComponent<DebugMap>(_debugMapQuery);
        }
    }
}
