using Sark.Common.GridUtil;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Sark.Common.Geometry
{
    public struct Diamond2D : IEnumerable<int2>
    {
        public readonly int Size;
        public int2 pos;

        public Diamond2D(int x, int y, int size) : this(new int2(x, y), size)
        {}
        public Diamond2D(int2 pos, int size)
        {
            this.Size = size;
            this.pos = pos;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(pos, Size);
        }

        IEnumerator<int2> IEnumerable<int2>.GetEnumerator() =>
            GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public struct Enumerator : IEnumerator<int2>
        {
            int2 origin;
            int2 center;
            int len;
            int curr;
            int width;
            int size;

            public Enumerator(int2 center, int size)
            {
                Assert.IsFalse(size <= 0);
                this.size = size;
                width = (size * 2) + 1;
                len = (width * width) - (size + 1);
                this.center = center;
                this.origin = center - width / 2;
                curr = -1;
            }

            public int2 Current
            {
                get
                {
                    int2 p = origin + Grid2D.IndexToPos(curr, width);

                    while (Grid2D.TaxicabDistance(center, p) > size && curr < len)
                    {
                        ++curr;
                        p = origin + Grid2D.IndexToPos(curr, width);
                    }
                    return p;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                ++curr;
                return curr < len;
            }

            public void Reset() { }
        }
    } 
}
