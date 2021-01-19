using Unity.Mathematics;

// From http://www.adammil.net/blog/v125_Roguelike_Vision_Algorithms.html#mine
namespace DotsRogue
{
    public interface IVisibilityMap
    {
        bool IsOpaque(int2 p);
        bool IsInBounds(int2 p);
        void SetVisible(int2 p);
        float Distance(int2 a, int2 b);
    }

    public static class FOV
    {
        static public void Compute<T>(int2 origin, int range, T map) 
            where T : IVisibilityMap
        { 
            map.SetVisible(origin);
            for (int octant = 0; octant < 8; octant++)
                ComputeOctant(octant, origin, range, 1, 
                    new Slope(1, 1), 
                    new Slope(0, 1), map);
        }

        static void ComputeOctant<T>(int octant, int2 origin, int range, int x, Slope top, Slope bottom, T map) 
            where T : IVisibilityMap
        {
            for (; x <= range; x++)
            {
                int2 yCoords = ComputeYCoordinates(octant, origin, x, map, 
                    ref top, ref bottom);

                int topY = yCoords.x;
                int bottomY = yCoords.y;

                if (!ComputeVisibility(topY, bottomY, 
                    range, octant, origin, x, map, 
                    ref top, ref bottom))
                    break;
            }
        }

        static int2 ComputeYCoordinates<T>(
            int octant, int2 origin, int x, T map, 
            ref Slope top, ref Slope bottom) 
            where T : IVisibilityMap
        {
            int topY;
            if (top.x == 1)
            {
                topY = x;
            }
            else
            {
                topY = ((x * 2 - 1) * top.y + top.x) / (top.x * 2);
                if (BlocksLight(x, topY, octant, origin, map))
                {
                    if (top.GreaterOrEqual(topY * 2 + 1, x * 2) && !BlocksLight(x, topY + 1, octant, origin, map))
                        topY++;
                }
                else
                {
                    int ax = x * 2;
                    if (BlocksLight(x + 1, topY + 1, octant, origin, map))
                        ax++;
                    if (top.Greater(topY * 2 + 1, ax))
                        topY++;
                }
            }

            int bottomY;
            if (bottom.y == 0)
            {
                bottomY = 0;
            }
            else
            {
                bottomY = ((x * 2 - 1) * bottom.y + bottom.x) / (bottom.x * 2);
                if (bottom.GreaterOrEqual(bottomY * 2 + 1, x * 2) && BlocksLight(x, bottomY, octant, origin, map) &&
                   !BlocksLight(x, bottomY + 1, octant, origin, map))
                {
                    bottomY++;
                }
            }
            return new int2(topY, bottomY);
        }

        static bool ComputeVisibility<T>(
            int topY, int bottomY, 
            int range, int octant, int2 origin, int x, T map, 
            ref Slope top, ref Slope bottom) 
            where T : IVisibilityMap
        {
            int wasOpaque = -1;
            for (int y = topY; y >= bottomY; y--)
            {
                if (range < 0 || map.Distance(0, new int2(x, y)) <= range)
                {
                    bool isOpaque = BlocksLight(x, y, octant, origin, map);
                    bool isVisible =
                      isOpaque ||
                      ((y != topY || top.Greater(y * 4 - 1, x * 4 + 1)) &&
                      (y != bottomY || bottom.Less(y * 4 + 1, x * 4 - 1)));

                    if (isVisible)
                        SetVisible(x, y, octant, origin, map);

                    if (x != range)
                    {
                        if (isOpaque)
                        {
                            if (wasOpaque == 0)
                            {
                                int nx = x * 2, ny = y * 2 + 1;
                                if (BlocksLight(x, y + 1, octant, origin, map))
                                    nx--;
                                if (top.Greater(ny, nx))
                                {
                                    if (y == bottomY)
                                    {
                                        bottom = new Slope(ny, nx);
                                        break;
                                    }
                                    else
                                        ComputeOctant(octant, origin, range, x + 1, top, new Slope(ny, nx), map);
                                }
                                else
                                {
                                    if (y == bottomY)
                                        return true;
                                }
                            }
                            wasOpaque = 1;
                        }
                        else
                        {
                            if (wasOpaque > 0)
                            {
                                int nx = x * 2, ny = y * 2 + 1;
                                if (BlocksLight(x + 1, y + 1, octant, origin, map))
                                    nx++;
                                if (bottom.GreaterOrEqual(ny, nx))
                                    return false;
                                top = new Slope(ny, nx);
                            }
                            wasOpaque = 0;
                        }
                    }
                }
            }

            return !(wasOpaque != 0);
        }

        // NOTE: the code duplication between BlocksLight and SetVisible is for performance. don't refactor the octant
        // translation out unless you don't mind an 18% drop in speed
        static bool BlocksLight<T>(int x, int y, int octant, int2 origin, T map) where T : IVisibilityMap
        {
            int nx = origin.x, ny = origin.y;
            switch (octant)
            {
                case 0: nx += x; ny -= y; break;
                case 1: nx += y; ny -= x; break;
                case 2: nx -= y; ny -= x; break;
                case 3: nx -= x; ny -= y; break;
                case 4: nx -= x; ny += y; break;
                case 5: nx -= y; ny += x; break;
                case 6: nx += y; ny += x; break;
                case 7: nx += x; ny += y; break;
            }
            return map.IsOpaque(new int2((int)nx, (int)ny));
        }

        static void SetVisible<T>(int x, int y, int octant, int2 origin, T map) where T : IVisibilityMap
        {
            int nx = origin.x, ny = origin.y;
            switch (octant)
            {
                case 0: nx += x; ny -= y; break;
                case 1: nx += y; ny -= x; break;
                case 2: nx -= y; ny -= x; break;
                case 3: nx -= x; ny -= y; break;
                case 4: nx -= x; ny += y; break;
                case 5: nx -= y; ny += x; break;
                case 6: nx += y; ny += x; break;
                case 7: nx += x; ny += y; break;
            }
            map.SetVisible(new int2((int)nx, (int)ny));
        }

        struct Slope // represents the slope Y/X as a rational number
        {
            public Slope(int y, int x) { this.y = y; this.x = x; }

            // this > y/x
            public bool Greater(int y, int x) { return this.y * x > this.x * y; }

            // this >= y/x
            public bool GreaterOrEqual(int y, int x) { return this.y * x >= this.x * y; }

            // this < y/x
            public bool Less(int y, int x) { return this.y * x < this.x * y; } 

            public readonly int x, y;
        }
    }
}