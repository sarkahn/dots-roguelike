
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

using RLTK.MonoBehaviours;

[AlwaysSynchronizeSystem]
public class TileRenderSystem : JobComponentSystem
{
    SimpleConsole _console;

    protected override void OnCreate()
    {
        _console = new SimpleConsole(40, 15, Resources.Load<Material>("Materials/ConsoleMat"), new Mesh());

        var player = EntityManager.CreateEntity();
        EntityManager.AddComponentData<Position>(player, new float2(10, 8));
        EntityManager.AddComponentData<Renderable>(player, new Renderable
        {
            FGColor = Color.yellow,
            BGColor = Color.black,
            Glyph = RLTK.CodePage437.ToCP437('@')
        });
        EntityManager.AddComponentData<InputData>(player, new InputData());

        for (int i = 0; i < 10; ++i)
        {
            var e = EntityManager.CreateEntity();
            var renderable = new Renderable
            {
                FGColor = Color.red,
                BGColor = Color.black,
                Glyph = RLTK.CodePage437.ToCP437('☺')
            };
            EntityManager.AddComponentData<Position>(e, new float2(i * 3, 13));
            EntityManager.AddComponentData(e, renderable);
            EntityManager.AddComponentData(e, new MoveLeft { Speed = 15 });
        }
    }

    protected override void OnStartRunning()
    {
        var camEntity = GetSingletonEntity<LockCameraToConsole>();
        var attach = EntityManager.GetComponentObject<LockCameraToConsole>(camEntity);
        attach.SetTarget(_console, Vector3.zero);
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

        return inputDeps;
    }
}