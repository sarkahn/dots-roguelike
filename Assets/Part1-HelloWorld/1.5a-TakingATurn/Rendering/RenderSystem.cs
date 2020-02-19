
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

using RLTK;
using RLTK.Consoles;
using RLTK.Rendering;
using Unity.Mathematics;

using static RLTK.CodePage437;


namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    [AlwaysSynchronizeSystem]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RenderSystem : SystemBase
    {
        SimpleConsole _console;

        EntityQuery _mapQuery;

        EntityQuery _playerQuery;

        protected override void OnCreate()
        {
            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<MapTiles>(),
                ComponentType.ReadOnly<MapData>()
                );

            _playerQuery = GetEntityQuery(
                ComponentType.ReadOnly<Player>()
                );

            RequireForUpdate(_playerQuery);
            RequireForUpdate(_mapQuery);
        }

        protected override void OnStartRunning()
        {
            var mapData = _mapQuery.GetSingleton<MapData>();
            _console = new SimpleConsole(mapData.width, mapData.height);

            RenderUtility.AdjustCameraToConsole(_console);
        }

        protected override void OnDestroy()
        {
            if(_console != null )
                _console.Dispose();
        }

        protected override void OnUpdate()
        {
            var mapEntity = _mapQuery.GetSingletonEntity();

            var mapData = _mapQuery.GetSingleton<MapData>();
            
            if(mapData.width != _console.Width || mapData.height != _console.Height)
            {
                _console.Resize(mapData.width, mapData.height);
                RenderUtility.AdjustCameraToConsole(_console);
                return;
            }

            var map = EntityManager.GetBuffer<MapTiles>(mapEntity);

            // The map has been resized but not yet updated
            if( (mapData.width * mapData.height) != map.Length )
            {
                return;
            }

            if (_playerQuery.IsEmptyIgnoreFilter)
                return;

            // Since we're iterating all tiles in the map we can probably benefit from using burst.
            // We can't use the console in a job since it's a managed object, so we'll work directly with tiles
            var tiles = new NativeArray<Tile>(_console.CellCount, Allocator.Temp, NativeArrayOptions.ClearMemory);

            var playerEntity = _playerQuery.GetSingletonEntity();

            bool hasView = EntityManager.HasComponent<TilesInView>(playerEntity);
            bool hasMemory = EntityManager.HasComponent<TilesInMemory>(playerEntity);
            
            _console.ClearScreen();

            if (!hasMemory && !hasView)
                RenderEverything(map, tiles, mapData.Size);
            else
            {
                if (hasMemory)
                {
                    var memory = EntityManager.GetBuffer<TilesInMemory>(playerEntity);
                    RenderMemory(map, memory, tiles, mapData.Size);
                }

                if (hasView)
                {
                    var view = EntityManager.GetBuffer<TilesInView>(playerEntity);
                    RenderView(map, view, tiles, mapData.Size);
                }
                
            }

            _console.WriteAllTiles(tiles);

            _console.Update();
            _console.Draw();
        }

        void RenderView(
            DynamicBuffer<MapTiles> map, 
            DynamicBuffer<TilesInView> view, 
            NativeArray<Tile> consoleTiles,
            int2 mapSize)
        {
            Job.WithCode(() =>
            {
                Color fg = default;
                Color bg = Color.black;
                char ch = default;
                for( int i = 0; i < map.Length; ++i )
                {
                    if(view[i])
                    {
                        var tileType = (TileType)map[i];
                        
                        switch (tileType)
                        {
                            case TileType.Floor:
                                fg = new Color(0, .5f, .5f);
                                ch = '.';
                                break;
                            case TileType.Wall:
                                fg = new Color(0, 1, 0);
                                ch = '#';
                                break;
                        }

                        consoleTiles[i] = new Tile
                        {
                            fgColor = fg,
                            bgColor = bg,
                            glyph = CodePage437.ToCP437(ch)
                        };
                    }
                }
            }).Run();

            RenderEntitiesInView(consoleTiles, view, mapSize);
        }

        void RenderMemory(
            DynamicBuffer<MapTiles> map,
            DynamicBuffer<TilesInMemory> memory,
            NativeArray<Tile> consoleTiles,
            int2 mapSize)
        {
            Job.WithCode(() =>
            {
                Color fg = default;
                Color bg = Color.black;
                char ch = default;
                for (int i = 0; i < map.Length; ++i)
                {
                    if (memory[i])
                    {
                        var tileType = (TileType)map[i];

                        fg = new Color(0.1f, .1f, 0.1f);
                        switch (tileType)
                        {
                            case TileType.Floor:
                                ch = '.';
                                break;
                            case TileType.Wall:
                                ch = '#';
                                break;
                        }

                        consoleTiles[i] = new Tile
                        {
                            fgColor = fg,
                            bgColor = bg,
                            glyph = CodePage437.ToCP437(ch)
                        };
                    }
                }
            }).Run();
        }

        void RenderEntitiesInView(NativeArray<Tile> consoleTiles, DynamicBuffer<TilesInView> view, int2 mapSize)
        {
            Entities
                .ForEach((in Position pos, in Renderable renderable) =>
                {
                    int2 p = pos.value;
                    int i = p.y * mapSize.x + p.x;
                    if(view[i])
                    {
                        consoleTiles[i] = new Tile
                        {
                            fgColor = renderable.fgColor,
                            bgColor = renderable.bgColor,
                            glyph = renderable.glyph
                        };
                    }
                }).Run();
        }


        void RenderEverything(DynamicBuffer<MapTiles> map, NativeArray<Tile> consoleTiles, int2 mapSize)
        {
            // Draw map tiles
            Job.WithCode(() =>
            {
                for (int i = 0; i < map.Length; ++i)
                {
                    Tile t = new Tile();
                    t.bgColor = Color.black;
                    switch ((TileType)map[i])
                    {
                        case TileType.Floor:
                            t.fgColor = new Color(0.5f, 0.5f, 0.5f);
                            t.glyph = ToCP437('.');
                            break;

                        case TileType.Wall:
                            t.fgColor = new Color(0, 1, 0);
                            t.glyph = ToCP437('#');
                            break;
                    }

                    consoleTiles[i] = t;
                }
            }).Run();

            RenderAllEntities(consoleTiles, mapSize);
        }

        void RenderAllEntities(NativeArray<Tile> consoleTiles, int2 mapSize)
        {
            // Draw renderables
            Entities
                .ForEach((in Position pos, in Renderable renderable) =>
                {
                    int2 p = pos.value;
                    int i = p.y * mapSize.x + p.x;
                    consoleTiles[i] = new Tile
                    {
                        fgColor = renderable.fgColor,
                        bgColor = renderable.bgColor,
                        glyph = renderable.glyph
                    };
                }).Run();
        }
    }
}