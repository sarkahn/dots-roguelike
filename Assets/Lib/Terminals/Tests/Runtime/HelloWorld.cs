using Unity.Entities;

using Sark.Terminals;

[DisableAutoCreation]
//[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public class HelloWorldSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var termCTX = new TerminalJobContext(this, GetSingletonEntity<Terminal>());
        Job.WithCode(() =>
        {
            var term = termCTX.GetAccessor();
            term.ClearScreen();
            term.DrawBorder();
            term.Print(5, 5, "Hello, world!");
        }).Schedule();
        Enabled = false;
    }
}
