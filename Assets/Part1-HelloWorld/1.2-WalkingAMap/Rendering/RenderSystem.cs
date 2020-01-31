using RLTK;
using RLTK.Consoles;
using RLTK.Rendering;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

using RLTKTutorial.Game;
using RLTKTutorial.Part3.Map;

using static RLTK.CodePage437;
using Unity.Mathematics;

namespace RLTKTutorial.Part3
{
    [DisableAutoCreation]
    [AlwaysSynchronizeSystem]
    public class RenderSystem : JobComponentSystem
    {
        SimpleConsole _console;

        protected override void OnStartRunning()
        {
            var mapEntity = GetSingletonEntity<TileBuffer>();
            var mapData = EntityManager.GetComponentData<MapData>(mapEntity);
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
            var mapData = GetSingleton<MapData>();
            int2 mapSize = new int2(mapData.width, mapData.height);

            if(mapData.width != _console.Width || mapData.height != _console.Height)
            {
                _console.Resize(mapData.width, mapData.height);
                RenderUtility.AdjustCameraToConsole(_console);
                return inputDeps;
            }

            _console.ClearScreen();

            // Since we're iterating all tiles in the map we can probably benefit from using burst.
            // We can't use the console in a job since it's a managed object, so copy all tile data
            var tiles = _console.ReadAllTiles(Allocator.TempJob);

            Entities
                .ForEach((ref DynamicBuffer<TileBuffer> map) =>
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

                        tiles[i] = t;
                    }
                }).Run();

            _console.WriteAllTiles(tiles);

            tiles.Dispose();

            Entities
                .WithoutBurst()
                .WithAll<Player>()
                .ForEach((in Position pos) =>
                {
                    int2 p = math.clamp(pos, 1, mapSize - 1);
                    _console.Set(p.x, p.y, Color.yellow, Color.black, ToCP437('@'));
                }).Run();

            _console.Update();
            _console.Draw();

            return default;
        }
    }
}