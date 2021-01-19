using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

using Sark.Common.GridUtil;
using Sark.Common.NativeArrayExtensions;
using Sark.EntityUtils.DynamicBufferExtensions;

namespace DotsRogue
{
    public class Memory
    {
    }

    public class Visibility
    {
        public static void AddToEntity(EntityManager em, Entity e, int range)
        {
        }
    }

    public struct MapMemoryBuffer : IBufferElementData
    {
        public bool Value;
        public static implicit operator bool(MapMemoryBuffer b) => b.Value;
        public static implicit operator MapMemoryBuffer(bool v) =>
            new MapMemoryBuffer { Value = v };
    }

    public struct MapViewBuffer : IBufferElementData
    {
        public bool Value;
        public static implicit operator bool(MapViewBuffer b) => b.Value;
        public static implicit operator MapViewBuffer(bool v) =>
            new MapViewBuffer { Value = v };
    }

    public struct ViewRange : IComponentData
    {
        public int Value;
        public static implicit operator int(ViewRange b) => b.Value;
        public static implicit operator ViewRange(int v) =>
            new ViewRange { Value = v };
    }

    public struct VisibilityMap : IVisibilityMap
    {
        int width;
        int height;

        NativeArray<MapTile> tiles;

        NativeArray<bool> view;
        NativeArray<bool> memory;

        public int Width => width;
        public int Height => height;
        public int2 Size => new int2(width, height);

        public float Distance(int2 a, int2 b) => math.distance(a, b);

        public bool IsInBounds(int2 p) => Grid2D.InBounds(p, Size);

        public bool IsOpaque(int2 p)
        {
            if (!IsInBounds(p))
                return true;
            int i = Grid2D.PosToIndex(p, Width);
            return tiles[i] == MapTile.Wall;
        }

        public void SetVisible(int2 p)
        {
            if (!IsInBounds(p))
                return;
            int i = Grid2D.PosToIndex(p, Width);
            view[i] = true;

            if (memory.IsCreated)
                memory[i] = true;
        }

        public VisibilityMap(int width, int height,
            NativeArray<MapTile> map,
            NativeArray<bool> view,
            NativeArray<bool> memory = default)
        {
            this.tiles = map;
            this.width = width;
            this.height = height;
            this.view = view;
            this.memory = memory;
        }
    }

    public struct VisibilityMapGrid : IVisibilityMap
    {
        GridData2D<MapTile> map;

        NativeArray<bool> view;
        NativeArray<bool> memory;

        public int Width => map.Width;
        public int Height => map.Height;
        public int2 Size => map.Size;

        public float Distance(int2 a, int2 b) => math.distance(a, b);

        public bool IsInBounds(int2 p) => Grid2D.InBounds(p, Size);

        public bool IsOpaque(int2 p)
        {
            if (!IsInBounds(p))
                return true;
            int i = Grid2D.PosToIndex(p, Width);
            return map[i] == MapTile.Wall;
        }

        public void SetVisible(int2 p)
        {
            if (!IsInBounds(p))
                return;
            int i = Grid2D.PosToIndex(p, Width);
            view[i] = true;

            if (memory.IsCreated)
                memory[i] = true;
        }

        public VisibilityMapGrid(GridData2D<MapTile> map,
            NativeArray<bool> view,
            NativeArray<bool> memory = default)
        {
            this.map = map;
            this.view = view;
            this.memory = memory;
        }
    }

    public struct MapViewJobContext
    {
        public Entity entity;

        BufferFromEntity<MapViewBuffer> viewFromEntity;
        ComponentDataFromEntity<Position> posFromEntity;

        public int2 Position
        {
            get => posFromEntity[entity];
            set => posFromEntity[entity] = value;
        }

        public MapViewJobContext(SystemBase sys, Entity e, bool readOnly = false)
        {
            entity = e;
            viewFromEntity = sys.GetBufferFromEntity<MapViewBuffer>(readOnly);
            posFromEntity = sys.GetComponentDataFromEntity<Position>(readOnly);
        }

        public GridData2D<bool> GetView(int2 mapSize)
        {
            return new GridData2D<bool>(
            viewFromEntity[entity].Reinterpret<bool>().AsNativeArray(),
            mapSize);
        }
    }

    public class VisibilitySystem : SystemBase
    {
        EntityQuery _viewQuery;
        EntityQuery _viewMemoryQuery;

        protected override void OnCreate()
        {
            _viewQuery = GetEntityQuery(
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<ViewRange>(),
                ComponentType.ReadWrite<MapViewBuffer>()
                );
            _viewQuery.AddChangedVersionFilter(typeof(Position));

            _viewMemoryQuery = GetEntityQuery(
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<ViewRange>(),
                ComponentType.ReadWrite<MapViewBuffer>(),
                ComponentType.ReadWrite<MapMemoryBuffer>()
                );
            _viewMemoryQuery.AddChangedVersionFilter(typeof(Position));

            RequireSingletonForUpdate<Map>();
        }

        protected override void OnUpdate()
        {
            var mapEntity = GetSingletonEntity<Map>();
            var mapData = new MapJobContext(this, mapEntity, true);

            Dependency = new ProcessViewJob
            {
                MapData = mapData,
                PosHandle = GetComponentTypeHandle<Position>(true),
                ViewRangeHandle = GetComponentTypeHandle<ViewRange>(true),
                ViewBufferHandle = GetBufferTypeHandle<MapViewBuffer>(false)
            }.ScheduleParallel(_viewQuery, Dependency);

            Dependency = new ProcessViewMemoryJob
            {
                MapData = mapData,
                PosHandle = GetComponentTypeHandle<Position>(true),
                ViewRangeHandle = GetComponentTypeHandle<ViewRange>(true),
                ViewBufferHandle = GetBufferTypeHandle<MapViewBuffer>(false),
                MemoryBufferHandle = GetBufferTypeHandle<MapMemoryBuffer>(false)
            }.ScheduleParallel(_viewMemoryQuery, Dependency);
        }

        [BurstCompile]
        struct ProcessViewJob : IJobChunk
        {
            [ReadOnly]
            public MapJobContext MapData;

            [ReadOnly]
            public ComponentTypeHandle<Position> PosHandle;
            [ReadOnly]
            public ComponentTypeHandle<ViewRange> ViewRangeHandle;

            public BufferTypeHandle<MapViewBuffer> ViewBufferHandle;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var map = MapData.Grid;

                var posArr = chunk.GetNativeArray(PosHandle);
                var viewRangeArr = chunk.GetNativeArray(ViewRangeHandle);

                var bufferAccessor = chunk.GetBufferAccessor(ViewBufferHandle);

                for (int i = 0; i < posArr.Length; ++i)
                {
                    var viewBuffer = bufferAccessor[i];

                    if (viewBuffer.Length != map.Length)
                        viewBuffer.ResizeUninitialized(map.Length);

                    var view = viewBuffer.Reinterpret<bool>().AsNativeArray();

                    view.MemClear();

                    var fovMap = new VisibilityMapGrid(map, view);

                    int2 pos = posArr[i];
                    int range = viewRangeArr[i];

                    FOV.Compute(pos, range, fovMap);
                }
            }
        }

        [BurstCompile]
        struct ProcessViewMemoryJob : IJobChunk
        {
            [ReadOnly]
            public MapJobContext MapData;

            [ReadOnly]
            public ComponentTypeHandle<Position> PosHandle;
            [ReadOnly]
            public ComponentTypeHandle<ViewRange> ViewRangeHandle;

            public BufferTypeHandle<MapViewBuffer> ViewBufferHandle;
            public BufferTypeHandle<MapMemoryBuffer> MemoryBufferHandle;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var map = MapData.Grid;

                var posArr = chunk.GetNativeArray(PosHandle);
                var viewRangeArr = chunk.GetNativeArray(ViewRangeHandle);

                var viewAccessor = chunk.GetBufferAccessor(ViewBufferHandle);
                var memoryAccessor = chunk.GetBufferAccessor(MemoryBufferHandle);

                for (int i = 0; i < posArr.Length; ++i)
                {
                    var viewBuffer = viewAccessor[i];
                    var memoryBuffer = memoryAccessor[i];

                    if (viewBuffer.Length != map.Length)
                        viewBuffer.ResizeUninitialized(map.Length);
                    if(memoryBuffer.Length != map.Length)
                    {
                        memoryBuffer.ResizeUninitialized(map.Length);
                        memoryBuffer.MemClear();
                    }

                    var view = viewBuffer.Reinterpret<bool>().AsNativeArray();
                    var memory = memoryBuffer.Reinterpret<bool>().AsNativeArray();

                    view.MemClear();

                    var fovMap = new VisibilityMapGrid(map, view, memory);

                    int2 pos = posArr[i];
                    int range = viewRangeArr[i];

                    FOV.Compute(pos, range, fovMap);
                }
            }
        }
    }
}
