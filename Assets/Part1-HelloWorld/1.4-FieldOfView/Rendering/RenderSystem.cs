
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

using RLTK;
using RLTK.Consoles;
using RLTK.Rendering;
using Unity.Mathematics;

using static RLTK.CodePage437;


namespace RLTKTutorial.Part1_4
{
    [DisableAutoCreation]
    [AlwaysSynchronizeSystem]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RenderSystem : JobComponentSystem
    {
        SimpleConsole _console;

        EntityQuery _mapQuery;

        EntityQuery _fovQuery;

        protected override void OnCreate()
        {
            _mapQuery = GetEntityQuery(
                ComponentType.ReadOnly<TileBuffer>(),
                ComponentType.ReadOnly<MapData>()
                );

            _fovQuery = GetEntityQuery(
                ComponentType.ReadOnly<TilesInView>(),
                ComponentType.ReadOnly<TilesInMemory>()
                );

            RequireForUpdate(_mapQuery);
            RequireForUpdate(_fovQuery);
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
            

            var fovEntity = _fovQuery.GetSingletonEntity();
            var fovTiles = EntityManager.GetBuffer<TilesInView>(fovEntity).AsNativeArray();
            var mapMemory = EntityManager.GetBuffer<TilesInMemory>(fovEntity);

            var map = EntityManager.GetBuffer<TileBuffer>(mapEntity);
            
            _console.ClearScreen();

            Job.WithoutBurst().WithCode(() =>
            {
                for( int x = 0; x < mapData.width; ++x )
                    for( int y = 0; y < mapData.height; ++y )
                    {
                        int idx = y * mapData.width + x;
                        if (mapMemory[idx])
                        {
                            switch ((TileType)map[idx])
                            {
                                case TileType.Floor:
                                    _console.Set(x, y, new Color(0.1f, 0.1f, 0.1f), Color.black, ToCP437('.'));
                                    break;
                                case TileType.Wall:
                                    _console.Set(x, y, new Color(0.1f, .1f, 0.1f), Color.black, ToCP437('#'));
                                    break;
                            }
                        }
                    }
            }).Run();

            Job.WithoutBurst().WithCode(() =>
            {
                for( int i = 0; i < fovTiles.Length; ++i )
                {
                    var p = fovTiles[i].value;
                    int idx = p.y * mapData.width + p.x;
                    
                    switch ((TileType)map[idx])
                    {
                        case TileType.Floor:
                            _console.Set(p.x, p.y, new Color(0, 0.5f, 0.5f), Color.black, ToCP437('.'));
                            break;
                        case TileType.Wall:
                            _console.Set(p.x, p.y, new Color(0, 1, 0), Color.black, ToCP437('#'));
                            break;
                    }
                }
            }).Run();
            
            Entities
                .WithoutBurst()
                .WithAll<Player>()
                .ForEach((in Position pos) =>
                {
                    int2 p = math.clamp(pos, 1, mapData.Size - 1);
                    _console.Set(p.x, p.y, Color.yellow, Color.black, ToCP437('@'));
                }).Run();

            _console.Update();
            _console.Draw();

            return default;
        }
    }
}