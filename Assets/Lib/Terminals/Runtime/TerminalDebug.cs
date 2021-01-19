using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
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
                in TerminalSize size) =>
                {
                    var term = new TerminalAccessor(tilesBuffer, size);
                    term.DrawBorder();
                }).Schedule();
            ecb.RemoveComponent<TerminalBorderOnCreate>(_eq);
            _barrier.AddJobHandleForProducer(Dependency);
        }
    } 
}
