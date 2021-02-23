using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Assertions;
using System.Runtime.InteropServices;

using Sark.Terminals;
using Sark.EntityUtils;
using Sark.Common.Geometry;
using Sark.Common.GridUtil;

using static Sark.Common.GridUtil.Grid2D;
using static Sark.Common.MathUtil;

namespace DotsRogue
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RenderUILogSystem : SystemBase
    {
        EntityQuery _termQuery;

        protected override void OnCreate()
        {
            _termQuery = GetEntityQuery(
                ComponentType.ReadOnly<UI>(),
                ComponentType.ReadOnly<Terminal>());
        }

        public BufferJobData<UILogBuffer> GetLogBuffer(SystemBase system)
        {
            var e = GetSingletonEntity<UILogBuffer>();
            return new BufferJobData<UILogBuffer>(system, e, false);
        }

        public ComponentJobData<HitPointsUI> GetHitPointsUI(SystemBase system)
        {
            var e = GetSingletonEntity<HitPointsUI>();
            return new ComponentJobData<HitPointsUI>(system, e, false);
        }

        protected override void OnUpdate()
        {
            var terminalEntity = _termQuery.GetSingletonEntity();
            var jobData = new TerminalJobContext(this, terminalEntity, false);

            Entities
                .WithChangeFilter<UILogBuffer, HitPointsUI>()
                //.WithoutBurst()
                .ForEach((
                    ref DynamicBuffer<UILogBuffer> logBuffer,
                    in HitPointsUI hp) =>
                {
                    var term = jobData.GetAccessor();

                    if (term.Length == 0)
                        return;

                    term.ClearScreen();
                    term.DrawBorder();

                    var emptyColor = new Color(0.05f, 0.05f, 0.05f);

                    FixedString128 hpString = $"HP: {hp.current} / {hp.max}";

                    int barWidth = term.Width - 20;
                    int barX = term.Width - barWidth - 1;
                    int hpX = barX - hpString.Length - 1;

                    term.PrintFGColor(hpX, term.Height - 1, hpString, Color.yellow);
                    term.DrawHorizontalBar(
                        term.Width - barWidth - 1, 
                        term.Height - 1, 
                        barWidth,
                        hp.current, hp.max,
                        Color.red, emptyColor);

                    int lineCount = 0;
                    for (int i = logBuffer.Length - 1;
                    i >= 0 && lineCount < term.Height - 2; 
                    --i, ++lineCount)
                    {
                        term.Print(2, term.Height - 2 - lineCount, logBuffer[i].Value);
                    }
                }).Schedule();
        }
    }

    struct ItemWithName
    {
        public Entity item;
        public Name name;
    }

    [UpdateAfter(typeof(RenderSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ShowInventorySystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;
        EntityQuery _gameStateChangedQuery;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<GameStateShowInventory>();
            _barrier = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            _gameStateChangedQuery = GetEntityQuery(ComponentType.ReadOnly<GameState>(),
                ComponentType.ReadOnly<GameStateShowInventory>());
            _gameStateChangedQuery.AddChangedVersionFilter(typeof(GameStateShowInventory));
        }

        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer();
            var gameStateCTX = new GameStateJobContext(this);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameStateCTX.ChangeGameState<GameStateTakingTurns>(ecb);
                _barrier.AddJobHandleForProducer(Dependency);
                return;
            }


            var playerEntity = GetSingletonEntity<Player>();

            NativeList<ItemWithName> playerItems = new NativeList<ItemWithName>(5, Allocator.TempJob);

            Entities
            .ForEach((Entity e, in InInventory inInventory, in Item item, in Name name) =>
            {
                if (inInventory.owner == playerEntity)
                {
                    playerItems.Add(new ItemWithName
                    {
                        item = e,
                        name = name
                    });
                }
            }).Schedule();

            if (!_gameStateChangedQuery.IsEmpty)
            {
                ShowInventory(playerItems);
            }

            if(InputUtility.ReadMenuInput(out int index))
            {
                Job.WithCode(() =>
                {
                    if (index < 0 || index >= playerItems.Length)
                        return;

                    var pair = playerItems[index];
                    var item = pair.item;
                    var name = pair.name;

                    ecb.AddComponent(item, new TryUseItem
                    {
                        target = playerEntity,
                        user = playerEntity
                    });
                }).Schedule();
            }
            
            playerItems.Dispose(Dependency);

            _barrier.AddJobHandleForProducer(Dependency);
        }

        void ShowInventory(NativeList<ItemWithName> playerItems)
        {
            var ecb = _barrier.CreateCommandBuffer();
            var ui = new UIJobContext(this);
            var stateCTX = new GameStateJobContext(this);
            var termContext = new TerminalJobContext(this, GetSingletonEntity<MapRenderTerminal>());
            Entities
            .WithChangeFilter<TerminalTilesBuffer>()
            .WithAll<MapRenderTerminal>()
            .ForEach((ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer,
            in TerminalSize size,
            in GridToWorld gridToWorld) =>
            {
                Debug.Log("Showing inventory");
                if (playerItems.Length == 0)
                {
                    stateCTX.ChangeGameState<GameStateTakingTurns>(ecb);
                    ui.Log("You don't have any items.");
                    return;
                }

                var term = new TerminalAccessor(tilesBuffer, size);

                // Could stand to be cleaned up! Too many magic numbers.
                int longest = 0;
                foreach (var pair in playerItems)
                    longest = math.max(longest, pair.name.Value.Length);
                Rect2D area = Rect2D.FromCenterSize(gridToWorld.Center, new int2(longest + 8, playerItems.Length + 4));
                term.DrawFilledBox(area.xMin, area.yMin, area.Width, area.Height);
                term.PrintFGColor(area.TopLeft + Right * 3, "Inventory", Color.yellow);

                int2 start = area.TopLeft + Right * 6 + Down * 2;
                for (int i = 0; i < playerItems.Length; ++i, start.y--)
                {
                    term.Print(start, playerItems[i].name);
                    int x = start.x - 2;
                    term.SetChar(x, start.y, ')');
                    term.SetChar(x - 2, start.y, '(');
                    term.SetCharFGColor(x - 1, start.y, (char)(97 + i), Color.yellow);
                }

                term.PrintFGColor(area.BottomLeft + Right * 3, "ESCAPE to cancel", Color.yellow);
            }).Schedule();
        }
    }

    public struct EntityWithName
    {
        public Entity entity;
        public FixedString128 name;
    }

    public struct TerminalData
    {
        public float3 pos;
        public float2 tileSize;
        public int2 size;
    }

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(RenderSystem))]
    public class CursorSystem : SystemBase
    {
        EntityQuery _stateQuery;

        protected override void OnCreate()
        {
            _stateQuery = GetEntityQuery(
                ComponentType.ReadOnly<GameState>(),
                ComponentType.Exclude<GameStateShowInventory>());

            RequireForUpdate(_stateQuery);
            RequireSingletonForUpdate<MapRenderTerminal>();
            RequireSingletonForUpdate<Player>();

        }

        protected override void OnUpdate()
        {
            var cam = Camera.main;

            if(cam == null)
            {
                Debug.LogWarning("CursorSystem: Main camera not found!");
            }

            float3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            var mapEntity = GetSingletonEntity<Map>();
            var termEntity = GetSingletonEntity<MapRenderTerminal>();
            var playerEntity = GetSingletonEntity<Player>();

            NativeReference<TerminalData> termDataRef = new NativeReference<TerminalData>(Allocator.TempJob);
            NativeReference<int2> xyRef = new NativeReference<int2>(Allocator.TempJob);

            Job.WithCode(() =>
            {
                termDataRef.Value = new TerminalData
                {
                    pos = GetComponent<Translation>(termEntity).Value,
                    tileSize = GetComponent<TileSize>(termEntity),
                    size = GetComponent<TerminalSize>(termEntity)
                };

                xyRef.Value = TerminalUtility.WorldToTileIndex(mousePos,
                    termDataRef.Value.pos, termDataRef.Value.size, termDataRef.Value.tileSize);
            }).Schedule();

            var viewFromEntity = GetBufferFromEntity<MapViewBuffer>(true);

            var termFromEntity = GetComponentDataFromEntity<Terminal>(false);
            var posFromEntity = GetComponentDataFromEntity<Translation>(true);
            var gridToWorldFromEntity = GetComponentDataFromEntity<GridToWorld>(true);

            float dt = (float)Time.ElapsedTime;
            Job .WithReadOnly(viewFromEntity)
                .WithReadOnly(posFromEntity)
                .WithCode(() =>
            {
                var xy = xyRef.Value;
                if (!InBounds(xy, termDataRef.Value.size))
                    return;

                if (viewFromEntity.HasComponent(playerEntity))
                {
                    var mapSize = GetComponent<MapSize>(mapEntity);
                    var viewBuffer = viewFromEntity[playerEntity];
                    var view = new GridData2D<bool>(
                        viewBuffer.Reinterpret<bool>().AsNativeArray(),
                        mapSize);

                    if (!view[xy])
                        return;
                }
                float t = pingpong(dt, 1);
                Color col0 = Color.white;
                Color col1 = Color.black;
                Color col = Color.Lerp(col0, col1, t);

                var tiles = GetBuffer<TerminalTilesBuffer>(termEntity);
                var worldPos = posFromEntity[termEntity].Value;
                var term = new TerminalAccessor(tiles, termDataRef.Value.size);
                term.SetBGColor(xy.x, xy.y, col);

                // Force the renderer to refresh next frame. Is there a better way to do this?
                // Maybe this should be "MapRenderTerminal" instead?
                SetComponent(termEntity, new Terminal());
            }).Schedule();

            NativeList<EntityWithName> targets = new NativeList<EntityWithName>(1, Allocator.TempJob);

            Entities
            .ForEach((Entity e, in Position pos, in Name nameC)=>
            {
                var xy = xyRef.Value;
                if (!InBounds(xy, termDataRef.Value.size) )
                    return;

                if (!xy.Equals(pos.Value.xy))
                    return;

                if (viewFromEntity.HasComponent(playerEntity))
                {
                    var mapSize = GetComponent<MapSize>(mapEntity);
                    var viewBuffer = viewFromEntity[playerEntity];
                    var view = new GridData2D<bool>(
                        viewBuffer.Reinterpret<bool>().AsNativeArray(),
                        mapSize);

                    if (!view[xy])
                        return;
                }

                targets.Add(new EntityWithName
                {
                    entity = e,
                    name = nameC
                });
            }).Schedule();

            Job.WithCode(() =>
            {
                if (targets.Length == 0)
                    return;

                var xy = xyRef.Value;

                var tiles = GetBuffer<TerminalTilesBuffer>(termEntity);
                var term = new TerminalAccessor(tiles, termDataRef.Value.size);

                int width = 0;
                foreach (var pair in targets)
                    width = math.max(width, pair.name.Length);
                width += 3;

                if (xy.x > 40)
                {
                    int x = xy.x - width;
                    int y = xy.y;
                    int2 arrow = new int2(xy.x - 2, xy.y);

                    foreach (var pair in targets)
                    {
                        var name = pair.name;

                        term.PrintFGColorBGColor(x, y, name, Color.white, Color.grey);

                        int padding = (width - name.Length)-1;
                        for (int i = 0; i < padding; ++i)
                            term.PrintFGColorBGColor(arrow.x - i, y, " ", Color.white, Color.grey);

                        term.PrintFGColorBGColor(arrow.x, arrow.y, "->", Color.white, Color.grey);
                        y++;
                    }
                }
                else
                {
                    int x = xy.x + 3;
                    int y = xy.y;
                    int2 arrow = new int2(xy.x + 1, xy.y);

                    foreach(var pair in targets)
                    {
                        var name = pair.name;

                        term.PrintFGColorBGColor(x + 1, y, name, Color.white, Color.grey);

                        int padding = (width - name.Length)-1;
                        for (int i = 0; i < padding; ++i)
                            term.PrintFGColorBGColor(arrow.x + 1 + i, y, " ", Color.white, Color.grey);

                        term.PrintFGColorBGColor(arrow.x, arrow.y, "<-", Color.white, Color.grey);
                        y++;
                    }
                }
            }).Schedule();

            termDataRef.Dispose(Dependency);
            xyRef.Dispose(Dependency);
            targets.Dispose(Dependency);
        }
    }

    public struct PositionWithEntity
    {
        public int2 pos;
        public Entity entity;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AOEData
    {
        public NativeReference<int2> mapSize;
        public NativeReference<Rect2D> area;
        public NativeList<PositionWithEntity> tiles;

        public AOEData(Allocator allocator)
        {
            mapSize = new NativeReference<int2>(allocator);
            area = new NativeReference<Rect2D>(allocator);
            tiles = new NativeList<PositionWithEntity>(16, allocator);

            area.Value = default;
            mapSize.Value = default;
        }

        public void Dispose(JobHandle dependency = default)
        {
            mapSize.Dispose(dependency);
            area.Dispose(dependency);
            tiles.Dispose(dependency);
        }

        public void Clear()
        {
            tiles.Clear();
            area.Value = default;
        }
    }

    public struct ColorPulse
    {
        public Color colorA;
        public Color colorB;

        public Color GetColor(float elapsedTime)
        {
            return Color.Lerp(colorA, colorB, pingpong(elapsedTime, 1));
        }
    }

    [UpdateAfter(typeof(RenderSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class TargetingSystem : SystemBase
    {
        EntityQuery _terminalChangedQuery;
        EntityQuery _gameStateChangedQuery;
        EntityCommandBufferSystem _barrier;

        NativeList<PositionWithEntity> _playerAreaTiles;
        AOEData _aoeData;

        ColorPulse _tileHighlight = new ColorPulse
        {
            colorA = Color.black,
            colorB = new Color32(73, 85, 89, 255)
        };

        ColorPulse _targetHighlight = new ColorPulse
        {
            colorA = Color.black,
            colorB = Color.red
        };

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<GameStateShowTargeting>();
            _terminalChangedQuery = GetEntityQuery(ComponentType.ReadOnly<MapRenderTerminal>(),
                ComponentType.ReadOnly<TerminalTilesBuffer>());
            _terminalChangedQuery.AddChangedVersionFilter(typeof(TerminalTilesBuffer));

            _gameStateChangedQuery = GetEntityQuery(ComponentType.ReadOnly<GameStateShowTargeting>());

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _playerAreaTiles = new NativeList<PositionWithEntity>(128, Allocator.Persistent);
            _aoeData = new AOEData(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _playerAreaTiles.Dispose(Dependency);
            _aoeData.Dispose(Dependency);
        }

        protected override void OnUpdate()
        {
            var cam = Camera.main;

            if (cam == null)
            {
                Debug.LogWarning("ShowTargetingSystem: Main camera not found!");
            }

            float3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            var playerCTX = new PlayerJobContext(this, true);
            var gameStateCTX = new GameStateJobContext(this);
            var mapStateCTX = new MapStateJobContext(this);
            var playerView = new MapViewJobContext(this, GetSingletonEntity<Player>(), true);
            var termCTX = new TerminalJobContext(this, GetSingletonEntity<MapRenderTerminal>());

            NativeReference<int2> cursorIndex = new NativeReference<int2>(Allocator.TempJob);
            NativeList<int2> tiles = new NativeList<int2>(64, Allocator.TempJob);

            Job.WithCode(() =>
            {
                cursorIndex.Value = TerminalUtility.WorldToTileIndex(mousePos, termCTX.WorldPos, termCTX.Size);
            }).Schedule();

            if(!_gameStateChangedQuery.IsEmpty)
            {
                Dependency = GetTilesInView(playerView, mapStateCTX, tiles, Dependency);
            }

            //if(!_terminalChangedQuery.IsEmpty)
            {
                Dependency = HighlightTiles(tiles, termCTX, Dependency);
            }

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                var ecb = _barrier.CreateCommandBuffer();
                var playerEntity = GetSingletonEntity<Player>();
                Entities
                .WithAll<TryUseItem>()
                .ForEach((Entity e, in Item item, in InInventory inventory) =>
                {
                    if(inventory.owner == playerEntity)
                    {
                        ecb.RemoveComponent<TryUseItem>(e);
                        gameStateCTX.ChangeGameState<GameStateShowInventory>(ecb);
                    }
                }).Schedule();

                _barrier.AddJobHandleForProducer(Dependency);
            }

            if (Input.GetMouseButtonDown(0))
            {
                Entities
                .WithReadOnly(tiles)
                .ForEach((ref GameStateShowTargeting targeting) =>
                {
                    if (!tiles.Contains(cursorIndex.Value))
                        return;

                    Entity tar = mapStateCTX.Entities[cursorIndex.Value];

                    if (tar != Entity.Null)
                    {
                        targeting.target = tar;
                    }
                }).Schedule();
            }

            cursorIndex.Dispose(Dependency);
            tiles.Dispose(Dependency);
        }

        public JobHandle HighlightTiles(NativeList<int2> tiles,
            TerminalJobContext termCTX,
            JobHandle inputDeps)
        {
            var tilePulse = _tileHighlight;
            float elapsed = (float)Time.ElapsedTime;
            return Entities
            .WithReadOnly(tiles)
            .ForEach((in GameStateShowTargeting targeting) =>
            {
                var term = termCTX.GetAccessor();

                foreach (var p in tiles)
                {
                    term.SetBGColor(p.x, p.y, tilePulse.GetColor(elapsed));
                }
            }).Schedule(inputDeps);
        }

        public JobHandle GetPositionsWithEntitiesInView(
            MapViewJobContext viewContext,
            MapStateJobContext mapStateCTX,
            NativeList<PositionWithEntity> tiles,
            JobHandle inputDeps)
        {
            return Entities
            .WithReadOnly(viewContext)
            .WithReadOnly(mapStateCTX)
            .ForEach((in GameStateShowTargeting targeting) =>
            {
                tiles.Clear();
                var entities = mapStateCTX.Entities;
                var view = viewContext.GetView(mapStateCTX.Size);
                int2 origin = viewContext.Position;

                Rect2D area = Rect2D.FromCenterSize(origin, targeting.range * 2 + 1);

                foreach (var p in area)
                    if (view[p] && ManhattanDistance(p, origin) <= targeting.range)
                    {
                        tiles.Add(new PositionWithEntity
                        {
                            entity = entities[p],
                            pos = p
                        });
                    }
            }).Schedule(inputDeps);
        }

        public JobHandle GetAOEData(MapViewJobContext viewCTX,
            MapStateJobContext mapStateCTX,
            AOEData data,
            JobHandle inputDeps)
        {
            return Entities
            .WithReadOnly(viewCTX)
            .WithReadOnly(mapStateCTX)
            .ForEach((in GameStateShowTargeting targeting) =>
            {
                var entities = mapStateCTX.Entities;
                var view = viewCTX.GetView(mapStateCTX.Size);
                int2 origin = viewCTX.Position;

                Rect2D area = Rect2D.FromCenterSize(origin, targeting.range * 2 + 1);
                data.area.Value = area;
                data.mapSize.Value = mapStateCTX.Size;
                ref var tiles = ref data.tiles;

                foreach (var p in area)
                    if (view[p] && ManhattanDistance(p, origin) <= targeting.range)
                    {
                        tiles.Add(new PositionWithEntity
                        {
                            entity = entities[p],
                            pos = p
                        });
                    }
            }).Schedule(inputDeps);
        }

        public JobHandle GetTilesInView(MapViewJobContext viewCTX,
            MapStateJobContext stateCTX,
            NativeList<int2> tiles,
            JobHandle inputDeps)
        {
            Assert.IsTrue(EntityManager.HasComponent<MapViewBuffer>(viewCTX.entity), "Attempting to retrieve the view for an entity, but that entity doesn't have a map view buffer");
            return Entities
           .WithReadOnly(viewCTX)
           .WithReadOnly(stateCTX)
           .ForEach((in GameStateShowTargeting targeting) =>
           {
               tiles.Clear();
               var view = viewCTX.GetView(stateCTX.Size);
               int2 origin = viewCTX.Position;

               Rect2D area = Rect2D.FromCenterSize(origin, targeting.range * 2 + 1);

               foreach (var p in area)
                   if (view[p] && ManhattanDistance(p, origin) <= targeting.range)
                   {
                       tiles.Add(p);
                   }
           }).Schedule(inputDeps);
        }
    }
}
