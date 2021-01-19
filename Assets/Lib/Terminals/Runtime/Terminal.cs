using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;

namespace Sark.Terminals
{
    public struct Tile
    {
        public ushort Glyph;
        public Color FGColor;
        public Color BGColor;

        public static Tile Default => new Tile
        {
            Glyph = 0,
            FGColor = Color.white,
            BGColor = Color.black
        };
    }

    public struct Terminal : IComponentData
    {
        // Dummy data - so we can use this component for ChangeFiltering to force render updates
        bool dirty;

        public static void AddToEntity(EntityManager em, Entity e, int2 size, float2 tileSize)
        {
            em.AddComponent<Terminal>(e);
            em.AddBuffer<TerminalTilesBuffer>(e);
            em.AddComponentData<TerminalSize>(e, size);
            em.AddComponentData<TileSize>(e, tileSize);
        }

        public static void AddToEntity(EntityManager em, Entity e, int w, int h, float tileWidth = 1, float tileHeight = 1)
        {
            AddToEntity(em, e, new int2(w, h), new float2(tileWidth, tileHeight));
        }
    }


    public struct TerminalSize : IComponentData
    {
        public int2 Value;
        public static implicit operator int2(TerminalSize b) => b.Value;
        public static implicit operator TerminalSize(int2 v) =>
            new TerminalSize { Value = v };
        public int Width => Value.x;
        public int Height => Value.y;
        public int Length => Value.x * Value.y;
    }

    public struct TileSize : IComponentData
    {
        public float2 Value;
        public static implicit operator float2(TileSize b) => b.Value;
        public static implicit operator TileSize(float2 v) =>
            new TileSize { Value = v };
        public float Width => Value.x;
        public float Height => Value.y;
    }

    public struct TerminalJobContext
    {
        public BufferFromEntity<TerminalTilesBuffer> tilesFromEntity;
        ComponentDataFromEntity<TerminalSize> sizeFromEntity;
        ComponentDataFromEntity<Terminal> termFromEntity;
        ComponentDataFromEntity<TileSize> tileSizeFromEntity;
        ComponentDataFromEntity<Translation> posFromEntity;
        public Entity entity;

        public int2 Size => sizeFromEntity[entity];
        public DynamicBuffer<TerminalTilesBuffer> Tiles => tilesFromEntity[entity];
        public float3 WorldPos => posFromEntity[entity].Value;
        public float2 TileSize => tileSizeFromEntity[entity].Value;

        public TerminalJobContext(SystemBase sys, Entity e, bool readOnly = false)
        {
            tilesFromEntity = sys.GetBufferFromEntity<TerminalTilesBuffer>(readOnly);
            sizeFromEntity = sys.GetComponentDataFromEntity<TerminalSize>(readOnly);
            tileSizeFromEntity = sys.GetComponentDataFromEntity<TileSize>(readOnly);
            posFromEntity = sys.GetComponentDataFromEntity<Translation>(readOnly);
            termFromEntity = sys.GetComponentDataFromEntity<Terminal>(readOnly);
            entity = e;
        }

        public TerminalAccessor GetAccessor()
        {
            return new TerminalAccessor(tilesFromEntity[entity], Size);
        }

        public void SetDirty()
        {
            termFromEntity[entity] = new Terminal();
        }
    }

    [InternalBufferCapacity(0)]
    public struct TerminalTilesBuffer : IBufferElementData
    {
        public Tile Value;
        public static implicit operator Tile(TerminalTilesBuffer b) => b.Value;
        public static implicit operator TerminalTilesBuffer(Tile v) =>
            new TerminalTilesBuffer { Value = v };
    }

    public struct UpdateTerminal : IComponentData
    { }

    public struct TerminalBorderOnCreate : IComponentData
    {}

    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    public class TerminalInitSystem : SystemBase
    {
        void CheckComponent<T>(Entity e) where T : unmanaged, IComponentData
        {
            bool exists = HasComponent<T>(e);
            Debug.Log($"Has {typeof(T)} : {exists}");
        }

        static void CheckBuffer<T>(Entity e, BufferFromEntity<T> bfe) where T : unmanaged, IBufferElementData
        {
            bool exists = bfe.HasComponent(e);
            Debug.Log($"Has {typeof(T)} : {exists}");
        }

        static void PrintHasComponent<T>(bool hasComponent)
        {
            Debug.Log($"Has {typeof(T)} : {hasComponent}");
        }

        protected override void OnUpdate()
        {
            Entities
                .WithChangeFilter<TerminalSize>()
                .WithName("ResizeTerminal")
                .ForEach((Entity e,
                ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer,
                in TerminalSize termSize) =>
                {
                    int len = termSize.Length;
                    Debug.Log("Terminal Resized");
                    tilesBuffer.ResizeUninitialized(len);
                    var tiles = tilesBuffer.Reinterpret<Tile>().AsNativeArray();
                    for (int i = 0; i < tiles.Length; ++i)
                    {
                        tiles[i] = new Tile
                        {
                            FGColor = Color.white,
                            BGColor = Color.black,
                            Glyph = 3
                        };
                    }
                }).Schedule();
        }


    }
}
