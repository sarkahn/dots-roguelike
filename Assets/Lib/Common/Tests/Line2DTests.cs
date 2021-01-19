using NUnit.Framework;
using Unity.Mathematics;
using Unity.Collections;

using Sark.Common.Geometry;

[TestFixture]
public class Line2DTests
{
    [Test]
    public void ForwardLine()
    {
        int endX = 10;
        var line = new Line2D(0, 0, endX, 0);

        var points = line.GetPoints(Allocator.Temp);

        Assert.AreEqual(11, points.Length);

        for (int x = 0; x <= endX; ++x)
            Assert.AreEqual(new int2(x, 0), points[x]);
    }

    [Test]
    public void BackwardLine()
    {
        int endX = 10;
        var line = new Line2D(0, 0, -endX, 0);

        var points = line.GetPoints(Allocator.Temp);

        Assert.AreEqual(endX + 1, points.Length);

        for (int i = 0; i <= endX; ++i)
            Assert.AreEqual(new int2(-i, 0), points[i]);
    }

    [Test]
    public void UpwardsLine()
    {
        int endY = 10;
        var line = new Line2D(0, 0, 0, endY);

        var points = line.GetPoints(Allocator.Temp);

        Assert.AreEqual(endY + 1, points.Length);

        for (int i = 0; i <= endY; ++i)
            Assert.AreEqual(new int2(0, i), points[i]);

    }

    [Test]
    public void DownwardsLine()
    {
        int len = 10;
        var line = new Line2D(0, 0, 0, -len);

        var points = line.GetPoints(Allocator.Temp);

        Assert.AreEqual(len + 1, points.Length);

        for (int i = 0; i <= len; ++i)
            Assert.AreEqual(new int2(0, -i), points[i]);
    }

    [Test]
    public void UpRightLine()
    {
        var begin = new int2(0, 0);
        var end = new int2(5, 5);

        var line = new Line2D(begin, end);
        var points = line.GetPoints(Allocator.Temp);


        Assert.AreEqual(6, points.Length);

        for (int i = 0; i < 6; ++i)
        {
            int x = points[i].x;
            int y = points[i].y;
            Assert.AreEqual(i, x);
            Assert.AreEqual(i, y);
        }
    }

    [Test]
    public void DownLeftLine()
    {
        var begin = new int2(0, 0);
        var end = new int2(-5, -5);

        var line = new Line2D(begin, end);
        var points = line.GetPoints(Allocator.Temp);

        Assert.AreEqual(6, points.Length);

        for (int i = 0; i < 6; ++i)
        {
            int x = points[i].x;
            int y = points[i].y;
            Assert.AreEqual(-i, x);
            Assert.AreEqual(-i, y);
        }
    }
}