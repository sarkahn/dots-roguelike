using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using Sark.Common.GridUtil;

namespace Sark.Pathfinding
{
    public struct PathingMap : IPathingMap<int>
    {
        GridData2D<bool> map;

        public PathingMap(GridData2D<bool> map)
        {
            this.map = map;
        }

        public void GetAvailableExits(int posIndex, NativeList<int> output)
        {
            int2 xy = map.IndexToPos(posIndex);
            for(int i = 0; i < Grid2D.Directions8Way.Length; ++i)
            {
                int2 dir = Grid2D.Directions8Way[i];
                int2 adj = xy + dir;
                if (!map.InBounds(adj))
                    continue;

                if (!IsObstacle(adj))
                    output.Add(map.PosToIndex(adj));
            }
        }

        public int GetCost(int a, int b)
        {
            return 1;
        }

        public float GetDistance(int a, int b)
        {
            var p1 = map.IndexToPos(a);
            var p2 = map.IndexToPos(b);
            return Grid2D.ManhattanDistance(p1, p2);
        }

        public int PosToIndex(int2 p) => map.PosToIndex(p);
        public int2 IndexToPos(int i) => map.IndexToPos(i);

        bool IsObstacle(int2 p)
        {
            return map[p];
        }
    }

    public struct PathingBitGrid : IPathingMap<int>, INativeDisposable
    {
        BitGrid2D bitGrid;

        public NativeBitArray Array => bitGrid.Array;
        public int Width => bitGrid.Width;
        public int Height => bitGrid.Height;
        public int2 Size => bitGrid.Size;
        public int Length => bitGrid.Length;
        public bool IsCreated => bitGrid.IsCreated;

        public bool this[int i]
        {
            get => bitGrid[i];
            set => bitGrid[i] = value;
        }

        public bool this[int2 p]
        {
            get => bitGrid[p];
            set => bitGrid[p] = value;
        }

        public bool this[int x, int y]
        {
            get => bitGrid[x, y];
            set => bitGrid[x, y] = value;
        }

        public PathingBitGrid(GridData2D<bool> data, Allocator allocator)
        {
            bitGrid = new BitGrid2D(data.Width, data.Height, allocator);
            for (int i = 0; i < data.Length; ++i)
                bitGrid[i] = data[i];
        }

        public void GetAvailableExits(int posIndex, NativeList<int> output)
        {
            int2 xy = bitGrid.IndexToPos(posIndex);
            for (int i = 0; i < Grid2D.Directions8Way.Length; ++i)
            {
                int2 dir = Grid2D.Directions8Way[i];
                int2 adj = xy + dir;
                if (!bitGrid.InBounds(adj))
                    continue;

                if (!IsObstacle(adj))
                    output.Add(bitGrid.PosToIndex(adj));
            }
        }

        public int GetCost(int a, int b)
        {
            return 1;
        }

        public float GetDistance(int a, int b)
        {
            var p1 = bitGrid.IndexToPos(a);
            var p2 = bitGrid.IndexToPos(b);
            return Grid2D.ManhattanDistance(p1, p2);
        }

        bool IsObstacle(int2 p)
        {
            return bitGrid[p];
        }

        public void MemClear()
        {
            Fill(false);
        }

        public void Fill(bool value)
        {
            bitGrid.Fill(value);
        }

        public void Dispose()
        {
            bitGrid.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            return bitGrid.Dispose(inputDeps);
        }
    }
}
