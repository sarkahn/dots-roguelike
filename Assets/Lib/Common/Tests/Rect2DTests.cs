using NUnit.Framework;
using System.Linq;
using Unity.Mathematics;

using Unity.Jobs;
using Unity.Collections;

using Sark.Common;
using Unity.Burst;
using Sark.Common.Geometry;

[TestFixture]
public class Rect2DTests
{
    [Test]
    public void RectInitialization()
    {
        Rect2D r = Rect2D.FromExtents(0, 0, 10, 10);

        Assert.AreEqual(new int2(0, 0), r.Position);
        Assert.AreEqual(new int2(10, 10), r.Max);
        Assert.AreEqual(new int2(10, 10), r.Size);

        r = Rect2D.FromPositionSize(5, 5, 5, 5);

        Assert.IsFalse(r.Intersect(new int2(0, 0)));
        Assert.AreEqual(new int2(5, 5), r.Position);
        Assert.AreEqual(new int2(5, 5), r.Size);
    }

    [Test]
    public void IEnumerableVisitsExpectedPoints()
    {
        int size = 10;
        Rect2D r = Rect2D.FromExtents(0, 0, size, size);

        var points =
            (from p in r
             select p).ToList();

        for (int x = 0; x < size; ++x)
            for (int y = 0; y < size; ++y)
                Assert.Contains(new int2(x, y), points);

        Assert.AreEqual(size * size, points.Count);
    }

    [Test]
    public void Add()
    {
        var r1 = Rect2D.FromExtents(0, 0, 10, 10);
        var r2 = Rect2D.FromExtents(1, 1, 1, 1);

        r1 += r2;

        Assert.AreEqual(new int2(1, 1), r1.Position);
        Assert.AreEqual(new int2(11, 11), r1.Max);
    }

    [Test]
    public void Intersect()
    {
        var r1 = Rect2D.FromExtents(0, 0, 10, 10);
        var r2 = Rect2D.FromExtents(5, 5, 10, 10);
        var r3 = Rect2D.FromExtents(100, 100, 5, 5);

        Assert.IsTrue(r1.Intersect(r2));
        Assert.IsFalse(r1.Intersect(r3));
        Assert.IsTrue(r1.Intersect(r1));

        r1 = Rect2D.FromExtents(0, 0, 5, 5);
        r2 = Rect2D.FromExtents(6, 6, 10, 10);

        Assert.IsFalse(r1.Intersect(r2));
    }

    [Test]
    public void Center()
    {
        var r1 = Rect2D.FromExtents(0, 0, 10, 10);
        Assert.AreEqual(new int2(5, 5), r1.Center);
    }

    [Test]
    public void SetCenter()
    {
        var r1 = Rect2D.FromPositionSize(0, 10);
        r1.Center = 30;

        Assert.AreEqual(30, r1.Center.x);
        Assert.AreEqual(30, r1.Center.y);
        Assert.AreEqual(25, r1.xMin);
        Assert.AreEqual(25, r1.yMin);
        Assert.AreEqual(35, r1.xMax);
        Assert.AreEqual(35, r1.yMax);
        Assert.AreEqual(10, r1.Width);
        Assert.AreEqual(10, r1.Height);
    }

    [Test]
    public void FromCenterSize()
    {
        var r1 = Rect2D.FromCenterSize(40, 10);

        Assert.AreEqual(10, r1.Width);
        Assert.AreEqual(10, r1.Height);

        Assert.AreEqual(35, r1.xMin);
        Assert.AreEqual(35, r1.yMin);
        Assert.AreEqual(45, r1.xMax);
        Assert.AreEqual(45, r1.yMax);
    }

    [Test]
    public void BurstedJobTest()
    {
        var r1 = Rect2D.FromPositionSize(0, 0, 10, 10);
        NativeList<int2> output = new NativeList<int2>(Allocator.TempJob);
        new GetPointsJob { Output = output, Rect = r1 }.Schedule().Complete();

        Assert.AreEqual(10 * 10, output.Length);

        output.Dispose();
    }

    [BurstCompile]
    struct GetPointsJob : IJob
    {
        public Rect2D Rect;
        public NativeList<int2> Output;
        public void Execute()
        {
            foreach (var p in Rect)
                Output.Add(p);
        }
    }
}