using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using Sark.Terminals;
using Sark.Common.GridUtil;
using Sark.EntityUtils;
using DotsRogue.ColorExtensions;

using Color = UnityEngine.Color;
using static Sark.Terminals.CodePage437;

namespace DotsRogue
{
    public struct MapRenderTerminal : IComponentData
    { }

    public struct ControlStringsBuffer : IBufferElementData
    {
        public FixedString128 Value;
        public static implicit operator FixedString128(ControlStringsBuffer b) => b.Value;
        public static implicit operator ControlStringsBuffer(FixedString128 v) =>
            new ControlStringsBuffer { Value = v };
    }

    public struct Renderable : IComponentData
    {
        public byte Glyph;
        public Color FGColor;
        public Color BGColor;

        public Renderable(char c, Color fg = default, Color bg = default)
        {
            FGColor = fg == default ? Color.white : fg;
            BGColor = bg == default ? Color.black : bg;
            Glyph = ToCP437(c);
        }

        public Renderable(TerminalTile t)
        {
            FGColor = t.FGColor;
            BGColor = t.BGColor;
            Glyph = (byte)t.Glyph;
        }

        public TerminalTile ToTile() =>
            new TerminalTile
            {
                FGColor = this.FGColor,
                BGColor = this.BGColor,
                Glyph = this.Glyph
            };
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RenderSystem : SystemBase
    {
        public static readonly Color FloorColor =
            new Color(0, .5f, .5f);

        public static readonly Color WallColor =
            Color.green;

        public static readonly byte FloorGlyph =
            ToCP437('.');

        public static readonly byte WallGlyph =
            ToCP437('#');

        EntityQuery _renderableItems;
        EntityQuery _renderableActors;
        EntityQuery _actorsMovedQuery;
        EntityQuery _playerDiedQuery;
        EntityQuery _gameStateChanged;
        EntityQuery _terminalChangedQuery;
        EntityQuery _terminalDirtyQuery;

        int _lastRenderableCount;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<MapRenderTerminal>();
            RequireSingletonForUpdate<Map>();

            _renderableActors = GetEntityQuery(
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<Renderable>(),
                ComponentType.ReadOnly<Actor>());

            _renderableItems = GetEntityQuery(
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<Renderable>(),
                ComponentType.ReadOnly<Item>());

            _actorsMovedQuery = GetEntityQuery(
                ComponentType.ReadOnly<Position>()
                );
            _actorsMovedQuery.AddChangedVersionFilter(ComponentType.ReadOnly<Position>());

            _gameStateChanged = GetEntityQuery(
                ComponentType.ReadOnly<GameState>());
            _gameStateChanged.AddChangedVersionFilter(typeof(GameState));

            _terminalChangedQuery = GetEntityQuery(
                ComponentType.ReadOnly<MapRenderTerminal>(),
                ComponentType.ReadOnly<TerminalTilesBuffer>());
            _terminalChangedQuery.AddChangedVersionFilter(typeof(TerminalTilesBuffer));

            _terminalDirtyQuery = GetEntityQuery(ComponentType.ReadOnly<MapRenderTerminal>(),
                ComponentType.ReadOnly<Terminal>());
            _terminalDirtyQuery.AddChangedVersionFilter(typeof(Terminal));

            _lastRenderableCount = _renderableActors.CalculateEntityCount();
        }

        protected override void OnUpdate()
        {
            // A bit ugly but prevents the terminal from having to redraw constantly
            bool doUpdate = false;
            int renderableCount = _renderableActors.CalculateEntityCount();
            doUpdate |= renderableCount != _lastRenderableCount;
            doUpdate |= !_gameStateChanged.IsEmpty;
            doUpdate |= !_actorsMovedQuery.IsEmpty;
            doUpdate |= !_terminalDirtyQuery.IsEmpty;
            //doUpdate |= !_terminalChangedQuery.IsEmpty;
            if (!doUpdate)
                return;

            _lastRenderableCount = renderableCount;

#if UNITY_EDITOR
            //Debug.Log("Rendering");
#endif
            var termEntity = GetSingletonEntity<MapRenderTerminal>();
            var terminalCTX = new TerminalJobContext(this, termEntity, false);

            if (!HasSingleton<Player>())
            {
                RenderEverything(terminalCTX);
                return;
            }

            ClearScreen(terminalCTX);

            var playerEntity = GetSingletonEntity<Player>();
            bool hasView = EntityManager.HasComponent<MapViewBuffer>(playerEntity);
            bool hasMemory = EntityManager.HasComponent<MapMemoryBuffer>(playerEntity);

            if (!hasView && !hasMemory)
            {
                RenderEverything(terminalCTX);
            }
            else
            {
                if (hasMemory)
                    RenderMemory(playerEntity, terminalCTX);

                if (hasView)
                    RenderView(playerEntity, terminalCTX);
            }
        }

        void ClearScreen(TerminalJobContext terminalData)
        {
            Job
                //.WithoutBurst()
                .WithCode(() =>
                {
                    var term = terminalData.GetAccessor();
                    term.ClearScreen();
                }).Schedule();
        }


        void RenderView(Entity viewEntity, TerminalJobContext termData)
        {
            var viewData = new BufferJobData<MapViewBuffer>(this, viewEntity, true);

            RenderMapInView(viewData, termData);
            RenderItemsInview(viewData, termData);
            RenderActorsInView(viewData, termData);
        }

        void RenderMapInView(BufferJobData<MapViewBuffer> viewData, TerminalJobContext termData)
        {
            var mapData = new MapJobContext(this, GetSingletonEntity<Map>(), true);
            var tileAssetsCTX = new MapTileAssetsBufferJobContext(this, true);

            Job.WithReadOnly(mapData)
               .WithReadOnly(viewData)
               .WithCode(() =>
               {
                   var map = mapData.Grid;
                   var term = termData.GetAccessor();
                   var view = viewData.GetArray<bool>();

                   var tileAssets = tileAssetsCTX.Tiles;

                   term.DrawBorder();
                   term.Print(5, 5, "Hello world");

                   if (view.Length == 0)
                       return;

                   TerminalTile t = TerminalTile.Default;

                   for (int i = 0; i < view.Length; ++i)
                   {
                       if (view[i])
                       {
                           int tileType = (int)map[i];
                           t = tileAssets[tileType];
                           term[i] = t;
                       }
                   }
               }).Schedule();
        }

        void RenderActorsInView(BufferJobData<MapViewBuffer> viewData, TerminalJobContext termData)
        {
            Entities
                //.WithoutBurst()
                .WithAll<Actor>()
                .ForEach((in Position pos, in Renderable renderable) =>
                {
                    var view = viewData.GetArray<bool>();
                    var term = termData.GetAccessor();

                    int i = Grid2D.PosToIndex(pos, term.Width);
                    if (view[i])
                    {
                        term[i] = renderable.ToTile();
                    }
                }).Schedule();
        }

        void RenderItemsInview(BufferJobData<MapViewBuffer> viewData, TerminalJobContext termData)
        {
            Entities
                //.WithoutBurst()
                .WithAll<Item>()
                .ForEach((in Position pos, in Renderable renderable) =>
                {
                    var view = viewData.GetArray<bool>();
                    var term = termData.GetAccessor();

                    int i = Grid2D.PosToIndex(pos, term.Width);
                    if (view[i])
                    {
                        term[i] = renderable.ToTile();
                    }
                }).Schedule();
        }

        void RenderMemory(Entity memoryEntity, TerminalJobContext terminalData)
        {
            var mapEntity = GetSingletonEntity<Map>();
            var mapData = new MapJobContext(this, mapEntity, true);
            var memoryData = new BufferJobData<MapMemoryBuffer>(this, memoryEntity, true);

            var tileAssetsCTX = new MapTileAssetsBufferJobContext(this, true);

            Job
                //.WithoutBurst()
                .WithReadOnly(mapData)
                .WithReadOnly(memoryData)
                .WithCode(() =>
                {
                    var map = mapData.Grid;
                    var memory = memoryData.GetArray<bool>();
                    var term = terminalData.GetAccessor();
                    var tileAssets = tileAssetsCTX.Tiles;

                    if (memory.Length == 0)
                        return;

                    for (int i = 0; i < memory.Length; ++i)
                    {
                        if (memory[i])
                        {
                            int tileType = (int)map[i];
                            var t = tileAssets[tileType];
                            t.FGColor = t.FGColor.ToGreyscale();
                            term[i] = t;
                        }
                    }
                }).Schedule();
        }

        void RenderEverything(TerminalJobContext termCTX)
        {
            RenderFullMap(termCTX);
            RenderAllEntities(termCTX);
        }

        void RenderFullMap(TerminalJobContext termCTX)
        {
            var mapCTX = new MapJobContext(this, GetSingletonEntity<Map>(), true);
            var tileAssetsCTX = new MapTileAssetsBufferJobContext(this, true);

            Job
                //.WithoutBurst()
                .WithReadOnly(mapCTX)
                .WithCode(() =>
                {
                    var map = mapCTX.Grid;
                    var term = termCTX.GetAccessor();
                    var tileAssets = tileAssetsCTX.Tiles;

                    //Debug.Log($"Rendering everything...Map size {map.Size}. Terminal size {term.Size}");

                    for (int i = 0; i < map.Length; ++i)
                    {
                        int tileType = (int)map[i];
                        var tile = tileAssets[tileType];
                        term[i] = tile;
                    }
                }).Schedule();
        }

        void RenderAllEntities(TerminalJobContext termCTX)
        {
            Dependency = new RenderEntities
            {
                PosHandle = GetComponentTypeHandle<Position>(true),
                RenderableHandle = GetComponentTypeHandle<Renderable>(true),
                TermData = termCTX
            }.ScheduleSingle(_renderableItems, Dependency);

            Dependency = new RenderEntities
            {
                PosHandle = GetComponentTypeHandle<Position>(true),
                RenderableHandle = GetComponentTypeHandle<Renderable>(true),
                TermData = termCTX
            }.ScheduleSingle(_renderableActors, Dependency);
        }

        struct RenderEntities : IJobChunk
        {
            [ReadOnly]
            public ComponentTypeHandle<Renderable> RenderableHandle;

            [ReadOnly]
            public ComponentTypeHandle<Position> PosHandle;

            public TerminalJobContext TermData;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var term = TermData.GetAccessor();
                var positions = chunk.GetNativeArray(PosHandle);
                var renderables = chunk.GetNativeArray(RenderableHandle);

                for (int i = 0; i < chunk.Count; ++i)
                {
                    int2 pos = positions[i];
                    Renderable r = renderables[i];
                    term[pos] = r.ToTile();
                }
            }
        }
    }
}
