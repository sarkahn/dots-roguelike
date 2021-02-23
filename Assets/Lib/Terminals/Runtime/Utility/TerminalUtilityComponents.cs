using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Sark.Terminals.Utility
{
    public struct TerminalAddBorder : IComponentData
    {}

    public struct TerminalClearOnce : IComponentData
    {}

    public struct TerminalClearEveryFrame : IComponentData
    {}

    public struct TerminalAddText : IComponentData
    {
        public FixedString128 str;
        public int2 position;
    }

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