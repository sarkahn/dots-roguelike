using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Color = UnityEngine.Color;

namespace Sark.Terminals
{
    [System.Serializable]
    public struct TerminalTile
    {
        public ushort Glyph;
        public Color FGColor;
        public Color BGColor;

        public static TerminalTile Default => new TerminalTile
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
            return new TerminalAccessor(Tiles, Size);
        }

        public void SetDirty()
        {
            termFromEntity[entity] = new Terminal();
        }
    }

    [InternalBufferCapacity(0)]
    public struct TerminalTilesBuffer : IBufferElementData
    {
        public TerminalTile Value;
        public static implicit operator TerminalTile(TerminalTilesBuffer b) => b.Value;
        public static implicit operator TerminalTilesBuffer(TerminalTile v) =>
            new TerminalTilesBuffer { Value = v };
    }

    public struct UpdateTerminal : IComponentData
    { }
}