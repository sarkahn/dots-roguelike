using Unity.Mathematics;

using Sark.RNG.RandomExtensions;
using Unity.Collections;

namespace Sark.RNG
{
    [System.Serializable]
    public struct DiceValue
    {
        public int numDice;
        public int sides;

        public DiceValue(int numDice, int sides = 6)
        {
            this.numDice = numDice;
            this.sides = sides;
        }

        public int Roll(Random rand)
        {
            return rand.RollDice(numDice, sides);
        }

        public int Roll(NativeReference<Random> rand)
        {
            return rand.RollDice(this);
        }
    } 
}
