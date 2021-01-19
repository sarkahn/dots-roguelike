
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Sark.Common.Geometry
{
    public struct Rect2D : System.IEquatable<Rect2D>, IEnumerable<int2>
    {
        public int xMin;
        public int yMin;

        public int xMax;
        public int yMax;

        public int Width => xMax - xMin;
        public int Height => yMax - yMin;

        public int2 TopLeft => new int2(xMin, yMax-1);
        public int2 TopRight => new int2(xMax, yMax-1);
        public int2 BottomLeft => new int2(xMin, yMin);
        public int2 BottomRight => new int2(xMax, yMin);

        public int2 Min
        {
            get => Position;
            set
            {
                xMin = value.x;
                yMin = value.y;
            }
        }

        public int2 Max
        {
            get => new int2(xMax, yMax);
            set
            {
                xMax = value.x;
                yMax = value.y;
            }
        }

        public int2 Position
        {
            get => new int2(xMin, yMin);
            // Moves position without affecting size
            set
            {
                int2 s = Size;
                xMin = value.x;
                yMin = value.y;
                Size = s;
            }
        }

        public int2 Size
        {
            get => new int2(Width, Height);
            set
            {
                xMax = xMin + value.x;
                yMax = yMin + value.y;
            }
        }

        public int2 Center
        {
            get => Position + Size / 2;
            set
            {
                Position = value - Size / 2;
            }
        }

        public static Rect2D FromCenterSize(int2 center, int2 size) =>
            new Rect2D { Size = size, Center = center };

        public static Rect2D FromPositionSize(int x, int y, int width, int height) =>
            new Rect2D { xMin = x, yMin = y, xMax = x + width, yMax = y + height };

        public static Rect2D FromPositionSize(int2 pos, int2 size) =>
            new Rect2D { Position = pos, Size = size };

        public static Rect2D FromExtents(int xMin, int yMin, int xMax, int yMax) =>
            new Rect2D { xMin = xMin, yMin = yMin, xMax = xMax, yMax = yMax };

        public static Rect2D FromExtents(int2 min, int2 max) =>
            new Rect2D { Position = min, Size = max - min };

        public bool Intersect(Rect2D other)
        {
            return !(math.any(Max < other.Min) || math.any(Min > other.Max));
        }

        public bool Intersect(int2 p)
        {
            return p.x >= xMin && p.x <= xMax && p.y >= yMin && p.y <= yMax;
        }

        public static Rect2D operator +(Rect2D lhs, Rect2D rhs)
        {
            lhs.Position += rhs.Position;
            return lhs;
        }

        public bool Equals(Rect2D other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max); 
        }

        public override string ToString()
        {
            return $"P({xMin}, {xMax}), S({Width}, {Height})";
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<int2> IEnumerable<int2>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<int2>
        {
            public int2 Current => curr;

            object IEnumerator.Current => Current;

            int2 curr;
            int2 min;
            int2 max;

            public Enumerator(Rect2D r)
            {
                min = r.Min;
                max = r.Max;
                curr = min;
                curr.x--;
            }

            public bool MoveNext()
            {
                curr.x++;

                if (curr.x == max.x)
                {
                    curr.x = min.x;
                    curr.y++;
                }

                return math.all(curr < max);
            }

            public void Dispose() { }
            public void Reset() { }
        }
    }
}