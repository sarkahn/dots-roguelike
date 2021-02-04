using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Sark.Terminals.Debugging
{
    public class TerminalBorderOnCreateSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;
        EntityQuery _eq;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer();
            Entities
                .WithStoreEntityQueryInField(ref _eq)
                .WithAll<TerminalBorderOnCreate>()
                .ForEach((ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer,
                in TerminalSize size,
                in Translation translation) =>
                {
                    var term = new TerminalAccessor(tilesBuffer, size, translation.Value);
                    term.DrawBorder();
                }).Schedule();
            ecb.RemoveComponent<TerminalBorderOnCreate>(_eq);
            _barrier.AddJobHandleForProducer(Dependency);
        }
    } 
}
