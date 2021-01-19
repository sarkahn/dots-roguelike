using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;


namespace Sark.Common.Geometry
{
    public struct EmptyCircle : IEnumerable<int2>
    {
        int2 p;
        int radius;

        public EmptyCircle(int xc, int yc, int radius)
        {
            p = new int2(xc, yc);
            this.radius = radius;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(p, radius);
        }

        IEnumerator<int2> IEnumerable<int2>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        static void SubsequencePoints(ref FixedList128<int2> output, 
            int2 c,
            int2 p)
        {
            output.Add(new int2(c.x + p.x, c.y + p.y));
            output.Add(new int2(c.x - p.x, c.y + p.y));
            output.Add(new int2(c.x + p.x, c.y - p.y));
            output.Add(new int2(c.x - p.x, c.y - p.y));
            output.Add(new int2(c.x + p.y, c.y + p.x));
            output.Add(new int2(c.x - p.y, c.y + p.x));
            output.Add(new int2(c.x + p.y, c.y - p.x));
            output.Add(new int2(c.x - p.y, c.y - p.x));
        }

        public struct Enumerator : IEnumerator<int2>
        {
            int2 c;
            int2 p;
            int d;
            int i;
            FixedList128<int2> points;

            public Enumerator(int2 c, int radius)
            {
                points = new FixedList128<int2>();
                p.x = 0;
                p.y = radius;
                this.c = c;
                d = 3 - 2 * radius;
                i = -1;
                SubsequencePoints(ref points, c, p);
            }

            public int2 Current => points[i];

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                if(++i < 8)
                    return true;

                ++p.x;
                if(p.y >= p.x )
                {
                    if (d > 0)
                    {
                        p.y--;
                        d = d + 4 * (p.x - p.y) + 10;
                    }
                    else
                        d = d + 4 * p.x + 6;

                    i = 0;
                    points.Clear();
                    SubsequencePoints(ref points, c, p);
                    return true;
                }
                return false;
            }

            public void Reset() { }
        }
    }
}
