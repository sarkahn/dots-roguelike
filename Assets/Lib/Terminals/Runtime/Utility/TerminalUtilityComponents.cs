using Unity.Entities;

namespace Sark.Terminals.Utility
{
    public struct TerminalAddBorder : IComponentData
    {}

    public struct TerminalClearOnce : IComponentData
    {}

    public struct TerminalClearEveryFrame : IComponentData
    {}

    public struct TerminalFillOnce : IComponentData
    {
        public byte glyph;
    }

    public struct TerminalFillEveryFrame : IComponentData
    {
        public byte glyph;
    }

    public struct TerminalNoiseOnce : IComponentData
    {}

    public struct TerminalNoiseEveryFrame : IComponentData
    {}
}