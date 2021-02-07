using Unity.Entities;

using Sark.Terminals;
using UnityEngine;
using Unity.Transforms;

namespace Sark.Terminals.Examples
{
    [GenerateAuthoringComponent]
    public struct HelloWorld : IComponentData
    { }

    public class HelloWorldSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer();
            Entities.WithAll<HelloWorld>()
            .ForEach((
            Entity e,
            ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer,
            in TerminalSize size,
            in Translation translation) =>
            {
                if (tilesBuffer.Length == 0)
                    return;
                var term = new TerminalAccessor(tilesBuffer, size, translation.Value);
                term.ClearScreen();
                term.DrawBorder();
                term.Print(5, 5, "Hello, world!");
                ecb.RemoveComponent<HelloWorld>(e);
            }).Schedule();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    } 
}
