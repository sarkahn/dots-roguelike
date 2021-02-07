
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Sark.Terminals.Utility
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public class TerminalBorderSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer();
            Entities
                .WithAll<TerminalAddBorder>()
                .ForEach((
                Entity e,
                ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer,
                in TerminalSize size,
                in Translation translation) =>
                {
                    if (tilesBuffer.Length == 0)
                        return;

                    var term = new TerminalAccessor(tilesBuffer, size, translation.Value);
                    term.DrawBorder();
                    ecb.RemoveComponent<TerminalAddBorder>(e);
                }).Schedule();
            _barrier.AddJobHandleForProducer(Dependency);
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(TerminalResizeSystem))]
    public class TerminalClearSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            Entities
                .WithAll<TerminalClearEveryFrame>()
                .ForEach((
                ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer,
                in TerminalSize size,
                in Translation pos) =>
                {
                    if (tilesBuffer.Length == 0)
                        return;

                    var term = new TerminalAccessor(tilesBuffer, size, pos.Value);
                    term.ClearScreen();
                }).ScheduleParallel();

            var ecb = _barrier.CreateCommandBuffer();

            Entities
                .WithAll<TerminalClearOnce>()
                .ForEach((
                Entity e,
                ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer,
                in TerminalSize size,
                in Translation pos) =>
                {
                    if (tilesBuffer.Length == 0)
                        return;

                    var term = new TerminalAccessor(tilesBuffer, size, pos.Value);
                    term.ClearScreen();
                    ecb.RemoveComponent<TerminalClearOnce>(e);
                }).ScheduleParallel();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(TerminalClearSystem))]
    public class TerminalFillSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((
                ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer,
                in TerminalFillEveryFrame fill) =>
            {
                if (tilesBuffer.Length == 0)
                    return;

                var arr = tilesBuffer.Reinterpret<TerminalTile>();
                for (int i = 0; i < arr.Length; ++i)
                {
                    var t = arr[i];
                    t.Glyph = fill.glyph;
                    arr[i] = t;
                }
            }).Schedule();

            var ecb = _barrier.CreateCommandBuffer();
            Entities.ForEach((
                Entity e,
                ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer,
                in TerminalFillOnce fill) =>
            {
                if (tilesBuffer.Length == 0)
                    return;

                var arr = tilesBuffer.Reinterpret<TerminalTile>();
                for (int i = 0; i < arr.Length; ++i)
                {
                    var t = arr[i];
                    t.Glyph = fill.glyph;
                    arr[i] = t;
                    ecb.RemoveComponent<TerminalFillOnce>(e);   
                }
            }).Schedule();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(TerminalClearSystem))]
    public class TerminalNoiseSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var rng = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
            Entities
                .WithAll<TerminalNoiseEveryFrame>()
                .ForEach((
                ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer) =>
                {
                    if (tilesBuffer.Length == 0)
                        return;

                    var arr = tilesBuffer.Reinterpret<TerminalTile>();
                for (int i = 0; i < arr.Length; ++i)
                {
                    var t = arr[i];
                    t.Glyph = (byte)rng.NextInt(0, 256);
                    arr[i] = t;
                }
            }).Schedule();

            rng = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
            var ecb = _barrier.CreateCommandBuffer();
            Entities
            .WithAll<TerminalNoiseOnce>()
            .ForEach((
                Entity e,
                ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer) =>
            {
                if (tilesBuffer.Length == 0)
                    return;

                var arr = tilesBuffer.Reinterpret<TerminalTile>();
                for (int i = 0; i < arr.Length; ++i)
                {
                    var t = arr[i];
                    t.Glyph = (byte)rng.NextInt(0, 256);
                    arr[i] = t;
                    ecb.RemoveComponent<TerminalNoiseOnce>(e);
                }
            }).Schedule();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
