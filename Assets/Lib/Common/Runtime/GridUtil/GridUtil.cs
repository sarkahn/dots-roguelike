using System.Runtime.CompilerServices;
using Unity.Mathematics;
using static Sark.Common.MathUtil;

namespace Sark.Common.GridUtil
{
    public static class Grid3D
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LocalToIndex(int x, int y, int z, int sizeX, int sizeY)
        {
            return x + sizeX * (y + sizeY * z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LocalToIndex(int3 p, int2 sizeXY)
        {
            return LocalToIndex(p.x, p.y, p.z, sizeXY.x, sizeXY.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 IndexToLocal(int i, int sizeX, int sizeY)
        {
            int w = sizeX;
            int h = sizeY;

            int z = i / (w * h);
            int y = (i - z * w * h) / w;
            int x = i - w * (y + h * z);

            return new int3(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 ToLocal(int3 pos, int3 sizeX)
        {
            return mod(pos, sizeX);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 WorldToCell(float3 worldPos, int3 sizeXYZ)
        {
            return (int3)(math.floor(worldPos / sizeXYZ));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 WorldToCell(int3 worldPos, int3 sizeXYZ)
        {
            return WorldToCell((float3)worldPos, sizeXYZ);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WorldToIndex(int3 worldPos, int3 sizeXYZ)
        {
            return LocalToIndex(ToLocal(worldPos, sizeXYZ), sizeXYZ.xy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WorldToIndex(float3 worldPos, int3 sizeXYZ)
        {
            int3 floored = (int3)math.floor(worldPos);
            return LocalToIndex(ToLocal(floored, sizeXYZ), sizeXYZ.xy);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InBounds(int3 localPos, int3 sizeXYZ)
        {
            return !(math.any(localPos < 0) || math.any(localPos >= sizeXYZ));
        }

        public static readonly int3 Up = new int3(0, 1, 0);
        public static readonly int3 Down = new int3(0, -1, 0);
        public static readonly int3 West = new int3(-1, 0, 0);
        public static readonly int3 East = new int3(1, 0, 0);
        public static readonly int3 North = new int3(0, 0, 1);
        public static readonly int3 South = new int3(0, 0, -1);

        /// <summary>
        /// List of the six directions on each axis of a cube
        /// </summary>
        public static readonly int3[] Directions6Way = new int3[6]
        {
            new int3(0, 1, 0),  // Up
            new int3(0, -1, 0), // Down
            new int3(-1, 0, 0), // West
            new int3(1, 0, 0),  // East
            new int3(0, 0, 1),  // North
            new int3(0, 0, -1), // South
        };

        /// <summary>
        /// List of directions along the x/z plane
        /// </summary>
        public static readonly int3[] DirectionsHorizontal = new int3[4]
        {
            new int3(-1, 0, 0), // West
            new int3(1, 0, 0),  // East
            new int3(0, 0, 1),  // North
            new int3(0, 0, -1), // South
        };
    }

    public static class Grid2D
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 WorldToCell(int2 worldPos, int2 sizeXY)
        {
            return WorldToCell((float2)worldPos, sizeXY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 WorldToCell(float2 worldPos, int2 sizeXY)
        {
            return (int2)math.floor(worldPos / sizeXY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PosToIndex(int x, int z, int sizeX)
        {
            return z * sizeX +x; 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 IndexToPos(int i, int sizeX)
        {
            int x = i % sizeX;
            int y = i / sizeX;
            return new int2(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PosToIndex(int2 pos, int sizeX)
        {
            return PosToIndex(pos.x, pos.y, sizeX);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InBounds(int2 localPos, int2 sizeXY)
        {
            return !(math.any(localPos < 0) || math.any(localPos >= sizeXY));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TaxicabDistance(int2 a, int2 b)
        {
            return math.abs(a.x - b.x) + math.abs(a.y - b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ManhattanDistance(int2 a, int2 b) =>
            TaxicabDistance(a, b);

        public static readonly int2 Up = new int2(0, 1);
        public static readonly int2 Down = new int2(0, -1);
        public static readonly int2 Right = new int2(1, 0);
        public static readonly int2 Left = new int2(-1, 0);
        public static readonly int2 RightUp = new int2(1, 1);
        public static readonly int2 RightDown = new int2(1, -1);
        public static readonly int2 LeftUp = new int2(-1, 1);
        public static readonly int2 LeftDown = new int2(-1, -1);

        public static readonly int2[] Directions4Way = new int2[]
        {
            new int2(0, 1),  // Up
            new int2(0, -1), // Down
            new int2(1, 0),  // Right
            new int2(-1, 0), // Left
        };

        public static readonly int2[] Directions8Way = new int2[]
        {
            new int2(0, 1),  // Up
            new int2(0, -1), // Down
            new int2(1, 0),  // Right
            new int2(-1, 0), // Left
            new int2(1, 1), // Right Up
            new int2(-1, 1), // Left Up
            new int2( 1, -1), // Right Down
            new int2(-1, -1), // Left Down
        };
    }
}
