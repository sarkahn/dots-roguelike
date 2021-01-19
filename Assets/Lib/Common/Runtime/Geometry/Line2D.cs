
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace Sark.Common.Geometry
{
    public struct Line2D : IEnumerable<int2>
    {
        int2 start;
        int2 end;

        public Line2D(int startX, int startY, int endX, int endY) :
            this(new int2(startX, startY), new int2(endX, endY))
        { }

        public Line2D(int2 start, int2 end)
        {
            this.start = start;
            this.end = end;
        }

        public NativeList<int2> GetPoints(Allocator allocator = Allocator.TempJob)
        {
            var e = GetEnumerator();
            var points = new NativeList<int2>(allocator);
            while (e.MoveNext())
                points.Add(e.Current);

            return points;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<int2> IEnumerable<int2>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<int2>
        {
            Line2D line;
            float2 curr;
            float2 dest;
            float2 slope;
            int2 last;

            public Enumerator(Line2D line)
            {
                this.line = line;
                curr = line.start + new float2(.5f, .5f);
                dest = line.end + new float2(.5f, .5f);
                slope = math.normalize(dest - curr);
                curr -= slope;
                last = (int2)math.floor(curr);
            }

            public int2 Current => new int2(math.floor(curr));
            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (line.start.Equals(line.end))
                    return false;

                int2 p = Current;

                if (last.Equals(line.end))
                    return false;

                while (p.Equals(last))
                {
                    curr += slope;
                    p = Current;
                }
                last = p;

                return true;
            }

            public void Dispose() { }
            public void Reset() { }
        }
    } 
}
