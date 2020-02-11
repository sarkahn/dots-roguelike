
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

using RLTK.MonoBehaviours;
using RLTK.Rendering;

namespace RLTKTutorial.Part1_1
{
    public struct Renderable : IComponentData
    {
        public Color FGColor;
        public Color BGColor;
        public byte Glyph;
    }


    public struct Position : IComponentData
    {
        public float2 Value;
        public static implicit operator float2(Position c) => c.Value;
        public static implicit operator Position(float2 v) => new Position { Value = v };
    }

    [DisableAutoCreation]
    [AlwaysSynchronizeSystem]
    public class RenderSystem : JobComponentSystem
    {
        SimpleConsole _console;

        protected override void OnCreate()
        {
            _console = new SimpleConsole(40, 15);
        }

        protected override void OnStartRunning()
        {
            RenderUtility.AdjustCameraToConsole(_console);
        }

        protected override void OnDestroy()
        {
            _console.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            _console.ClearScreen();

            Entities
                .WithoutBurst()
                .ForEach((in Position pos, in Renderable renderable) =>
            {
                var p = (int2)pos.Value;
                _console.Set(p.x, p.y, renderable.FGColor, renderable.BGColor, renderable.Glyph);
            }).Run();

            _console.Update();
            _console.Draw();

            return default;
        }
    }
}