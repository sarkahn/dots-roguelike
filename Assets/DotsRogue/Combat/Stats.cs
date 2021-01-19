using Unity.Entities;

using Sark.RNG;

namespace DotsRogue
{
    public struct MaxHitPoints : IComponentData
    {
        public int Value;
        public static implicit operator int(MaxHitPoints b) => b.Value;
        public static implicit operator MaxHitPoints(int v) =>
            new MaxHitPoints { Value = v };
    }

    public struct HitPoints : IComponentData
    {
        public int Value;
        public static implicit operator int(HitPoints b) => b.Value;
        public static implicit operator HitPoints(int v) =>
            new HitPoints { Value = v };
    }

    public struct Defense : IComponentData
    {
        public int Value;
        public static implicit operator int(Defense b) => b.Value;
        public static implicit operator Defense(int v) =>
            new Defense { Value = v };
    }

    public struct AttackPower : IComponentData
    {
        public DiceValue DiceValue;
        public static implicit operator DiceValue(AttackPower b) => b.DiceValue;
        public static implicit operator AttackPower(DiceValue v) =>
            new AttackPower { DiceValue = v };
    }
}
