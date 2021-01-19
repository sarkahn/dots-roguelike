using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;


namespace Sark.Common.Geometry
{
    public struct FilledCircle : IEnumerable<int2>
    {
        int2 p;
        int radius;

        public FilledCircle(int x, int y, int radius)
        {
            p = new int2(x, y);
            this.radius = radius;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(p, radius);
        }

        IEnumerator<int2> IEnumerable<int2>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<int2>
        {
            int2 origin;
            int2 xy;
            int radius;

            public Enumerator(int2 origin, int radius)
            {
                this.origin = origin;
                this.radius = radius;
                xy = -(radius + 1);
            }

            public int2 Current => origin + xy;

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                ++xy.x;
                while (!(xy.x * xy.x + xy.y * xy.y <= radius * radius + radius))
                {
                    ++xy.x;
                    if (xy.x >= radius)
                    {
                        xy.x = -radius;
                        ++xy.y;

                        if (xy.y > radius)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            public void Reset() { }
        }
    }
}
