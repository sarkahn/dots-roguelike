
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
    public class RenderSystem : JobComponentSystem
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
                ComponentType.ReadOnly<Player>(),
                ComponentType.ReadOnly<TilesInView>(),
                ComponentType.ReadOnly<TilesInMemory>()
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

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var mapEntity = _mapQuery.GetSingletonEntity();

            var mapData = _mapQuery.GetSingleton<MapData>();
            
            if(mapData.width != _console.Width || mapData.height != _console.Height)
            {
                _console.Resize(mapData.width, mapData.height);
                RenderUtility.AdjustCameraToConsole(_console);
                return inputDeps;
            }

            var map = EntityManager.GetBuffer<MapTiles>(mapEntity);

            // The map has been resized but not yet updated
            if( (mapData.width * mapData.height) != map.Length )
            {
                return inputDeps;
            }

            if (_playerQuery.IsEmptyIgnoreFilter)
                return inputDeps;

            var playerEntity = _playerQuery.GetSingletonEntity();
            var view = EntityManager.GetBuffer<TilesInView>(playerEntity);
            var memory = EntityManager.GetBuffer<TilesInMemory>(playerEntity);

            _console.ClearScreen();

            Job
                .WithoutBurst()
                .WithCode(() =>
            {
                Color fg = default;
                Color bg = Color.black;
                char ch = default;
                for( int x = 0; x < mapData.width; ++x )
                    for( int y = 0; y < mapData.height; ++y )
                    {
                        int idx = y * mapData.width + x;
                        if (memory[idx])
                        {
                            var tile = (TileType)map[idx];
                            if( view[idx] )
                                switch(tile)
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
                            else
                            {
                                fg = new Color(0.1f, .1f, 0.1f);
                                switch (tile)
                                {
                                    case TileType.Floor:
                                        ch = '.';
                                        break;
                                    case TileType.Wall:
                                        ch = '#';
                                        break;
                                }
                            }

                            _console.Set(x, y, fg, bg, ToCP437(ch));
                        }
                    }
            }).Run();
            

            Entities
                .WithoutBurst()
                .ForEach((in Renderable render, in Position pos) =>
                {
                    int2 p = math.clamp(pos, 1, mapData.Size - 1);
                    if( view[p.y * mapData.width + p.x] )
                        _console.Set(p.x, p.y, render.fgColor, render.bgColor, render.glyph);
                }).Run();

            _console.Update();
            _console.Draw();

            return default;
        }
    }
}