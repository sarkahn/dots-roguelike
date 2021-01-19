using Unity.Mathematics;
using Unity.Collections;

namespace Sark.RNG.RandomExtensions
{
    public static class RandomExtension
    {
        public static int RollDice(this ref Random rand, int numDice, int sides)
        {
            int v = 0;
            for (int i = 0; i < numDice; ++i)
                v += rand.NextInt(1, sides + 1);
            return v;
        }

        public static int RollDice(this ref NativeReference<Random> rand, int numDice, int sides)
        {
            var r = rand.Value;
            int val = r.RollDice(numDice, sides);
            rand.Value = r;
            return val;
        }

        public static int RollDice(this ref Random rand, in DiceValue dice)
        {
            return RollDice(ref rand, dice.numDice, dice.sides);
        }

        public static int RollDice(this ref NativeReference<Random> rand, DiceValue dice)
        {
            var r = rand.Value;
            int val = r.RollDice(dice.numDice, dice.sides);
            rand.Value = r;
            return val;
        }

        public static int NextInt(this ref NativeReference<Random> rand, int min, int max)
        {
            var r = rand.Value;
            int v = r.NextInt(min, max);
            rand.Value = r;
            return v;
        }

        public static int2 NextInt2(this ref NativeReference<Random> rand, int2 min, int2 max)
        {
            var r = rand.Value;
            var v = r.NextInt2(min, max);
            rand.Value = r;
            return v;
        }

        public static int3 NextInt3(this ref NativeReference<Random> rand, int3 min, int3 max)
        {
            var r = rand.Value;
            var v = r.NextInt3(min, max);
            rand.Value = r;
            return v;
        }

    }
}
