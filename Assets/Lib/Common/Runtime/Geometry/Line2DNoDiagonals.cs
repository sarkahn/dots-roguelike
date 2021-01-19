using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Sark.Common.Geometry
{
    public struct Line2DNoDiagonals : IEnumerable<int2>
    {
        int2 start;
        int2 end;

        public Line2DNoDiagonals(int2 start, int2 end)
        {
            this.start = start;
            this.end = end;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(start, end);
        }

        IEnumerator<int2> IEnumerable<int2>.GetEnumerator() =>
            GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public struct Enumerator : IEnumerator<int2>
        {
            int2 dist;
            int2 step;
            int error;
            int2 curr;
            int2 end;

            public Enumerator(int2 start, int2 end)
            {
                curr = start;
                this.end = end;

                dist = new int2(
                    math.abs(end.x - start.x),
                    -math.abs(end.y - start.y)
                    );

                step = math.select(-1, 1, start < end);
                error = dist.x + dist.y;
                curr = start;

                if (2 * error - dist.y > dist.x - 2 * error)
                {
                    error -= dist.y;
                    curr.x -= step.x;
                }
                else
                {
                    error -= dist.x;
                    curr.y -= step.y;
                }
            }

            public int2 Current => (int2)math.floor(curr);

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (curr.x == end.x && curr.y == end.y)
                    return false;


                if (2 * error - dist.y > dist.x - 2 * error)
                {
                    error += dist.y;
                    curr.x += step.x;
                }
                else
                {
                    error += dist.x;
                    curr.y += step.y;
                }

                return true;
            }

            public void Reset()
            { }

            public void Dispose()
            { }
        }
    }
}