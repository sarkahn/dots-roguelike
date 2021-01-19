using UnityEngine;

namespace DotsRogue
{
    public static class InputUtility
    {
        public static bool ReadMenuInput(out int i)
        {
            i = -1;
            for(int j = 0; j < 26; ++j)
            {
                if (Input.GetKeyDown((KeyCode)97 + j))
                {
                    i = j;
                    return true;
                }
            }

            return false;
        }

        // Convert from a-z to 0-25. Returns -1 if key is out of range
        public static int LetterToIndex(KeyCode key)
        {
            return key switch
            {
                KeyCode.A => 0,
                KeyCode.B => 1,
                KeyCode.C => 2,
                KeyCode.D => 3,
                KeyCode.E => 4,
                KeyCode.F => 5,
                KeyCode.G => 6,
                KeyCode.H => 7,
                KeyCode.I => 8,
                KeyCode.J => 9,
                KeyCode.K => 10,
                KeyCode.L => 11,
                KeyCode.M => 12,
                KeyCode.N => 13,
                KeyCode.O => 14,
                KeyCode.P => 15,
                KeyCode.Q => 16,
                KeyCode.R => 17,
                KeyCode.S => 18,
                KeyCode.T => 19,
                KeyCode.U => 20,
                KeyCode.V => 21,
                KeyCode.W => 22,
                KeyCode.X => 23,
                KeyCode.Y => 24,
                KeyCode.Z => 25,
                _ => -1
            };
        }
    }
}
