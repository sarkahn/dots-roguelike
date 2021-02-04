using Unity.Entities;
using Unity.Transforms;

namespace Sark.Terminals
{
    public struct TerminalClearEveryFrame : IComponentData
    {

    }

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class TerminalClearEveryFrameSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<TerminalClearEveryFrame>()
                .ForEach((
                ref DynamicBuffer<TerminalTilesBuffer> tilesBuffer,
                in TerminalSize size,
                in Translation pos) =>
            {
                var term = new TerminalAccessor(tilesBuffer, size, pos.Value);
                term.ClearScreen();
            }).ScheduleParallel();
        }
    }
}