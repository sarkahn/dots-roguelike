using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

using Sark.Common.GridUtil;

using Debug = UnityEngine.Debug;

namespace DotsRogue
{
    public struct MapState
    {
    }

    public struct PathBlocker : IComponentData
    {
    }

    public struct MapObstaclesBuffer : IBufferElementData
    {
        public bool Value;
        public static implicit operator bool(MapObstaclesBuffer b) => b.Value;
        public static implicit operator MapObstaclesBuffer(bool v) =>
            new MapObstaclesBuffer { Value = v };
    }

    public struct MapEntitiesBuffer : IBufferElementData
    {
        public Entity Value;
        public static implicit operator Entity(MapEntitiesBuffer b) => b.Value;
        public static implicit operator MapEntitiesBuffer(Entity v) =>
            new MapEntitiesBuffer { Value = v };
    }

    public struct MapStateJobContext
    {
        //public BufferJobData<MapStateBuffer> BufferData;
        BufferFromEntity<MapObstaclesBuffer> obstaclesFromEntity;
        BufferFromEntity<MapEntitiesBuffer> entitiesFromEntity;
        ComponentDataFromEntity<MapSize> sizeFromEntity;

        public Entity entity;

        public int2 Size => sizeFromEntity[entity];

        public GridData2D<bool> Obstacles =>
            new GridData2D<bool>(
                obstaclesFromEntity[entity].Reinterpret<bool>().AsNativeArray(),
                Size);

        public GridData2D<Entity> Entities =>
            new GridData2D<Entity>(
                entitiesFromEntity[entity].Reinterpret<Entity>().AsNativeArray(),
                Size);

        public MapStateJobContext(SystemBase sys, Entity e, bool readOnly = false)
        {
            entity = e;
            obstaclesFromEntity = sys.GetBufferFromEntity<MapObstaclesBuffer>(readOnly);
            entitiesFromEntity = sys.GetBufferFromEntity<MapEntitiesBuffer>(readOnly);
            sizeFromEntity = sys.GetComponentDataFromEntity<MapSize>(readOnly);
        }

        public MapStateJobContext(SystemBase sys, bool readOnly = false)
            : this(sys, sys.GetSingletonEntity<MapObstaclesBuffer>(), readOnly)
        {}
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class MapStateSystem : SystemBase
    {
        //public struct OnMap : ISystemStateComponentData
        //{ }

        EntityQuery _movedActors;
        EntityQuery _mapChanged;
        EntityQuery _actorsOnMap;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<MapObstaclesBuffer>();
            RequireSingletonForUpdate<MapEntitiesBuffer>();

            _movedActors = GetEntityQuery(
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<PathBlocker>());
            _movedActors.AddChangedVersionFilter(typeof(Position));

            _mapChanged = GetEntityQuery(
                ComponentType.ReadOnly<MapTilesBuffer>(),
                ComponentType.ReadOnly<MapSize>());
            _mapChanged.AddChangedVersionFilter(typeof(MapTilesBuffer));

            _actorsOnMap = EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<PathBlocker>()
                );
        }

        protected override void OnUpdate()
        {
            bool changed =
                //_addedActors.CalculateEntityCount() != 0 ||
                //_removedActors.CalculateEntityCount() != 0 ||
                !(_movedActors.IsEmpty &&
                _mapChanged.IsEmpty);

            if (!changed)
                return;

            var mapEntity = GetSingletonEntity<Map>();
            var mapData = new MapJobContext(this, mapEntity, true);
            var stateData = new MapStateJobContext(this,
                mapEntity, false);

            // Update from map state
            Entities
                .WithReadOnly(mapData)
                .ForEach((ref DynamicBuffer<MapObstaclesBuffer> obstaclesBuffer,
                ref DynamicBuffer<MapEntitiesBuffer> entitiesBuffer) =>
                {
                    //Debug.Log("Clearing all map state, adding walls");
                    var map = mapData.Grid;

                    var obstacles = obstaclesBuffer.Reinterpret<bool>().AsNativeArray();

                    for (int i = 0; i < obstacles.Length; ++i)
                    {
                        obstacles[i] = map[i] == MapTileType.Wall;
                    }

                    //Clear entities too since we're rebuilding entities state
                    // from path blocker entities next
                    unsafe
                    {
                        int stride = UnsafeUtility.SizeOf<Entity>();
                        UnsafeUtility.MemClear(entitiesBuffer.GetUnsafePtr(), entitiesBuffer.Length * stride);
                    }
                }).Schedule();

            //Debug.Log($"Actors on map count: {_actorsOnMap.CalculateEntityCount()}");

            // Update from actor state
            Dependency = new UpdateFromPathBlockers
            {
                StateData = stateData,
                EntityHandle = GetEntityTypeHandle(),
                PosHandle = GetComponentTypeHandle<Position>(true)
            }.ScheduleSingle(_actorsOnMap, Dependency);

        }

        [BurstCompile]
        struct UpdateFromPathBlockers : IJobChunk
        {
            [ReadOnly]
            public ComponentTypeHandle<Position> PosHandle;

            [ReadOnly]
            public EntityTypeHandle EntityHandle;

            //[ReadOnly]
            //public MapJobData MapData;

            public MapStateJobContext StateData;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var positions = chunk.GetNativeArray(PosHandle);
                var entities = chunk.GetNativeArray(EntityHandle);

                var obstacles = StateData.Obstacles;
                var mapEntities = StateData.Entities;


                // Update from entities
                for (int entityIndex = 0; entityIndex < chunk.Count; ++entityIndex)
                {
                    Entity entity = entities[entityIndex];
                    int2 pos = positions[entityIndex];

                    int posIndex = mapEntities.PosToIndex(pos);

                    mapEntities[posIndex] = entity;
                    obstacles[posIndex] = true;

                    //Debug.Log($"Adding entity obstacle at {pos}");
                }
            }
        }
    }
}
