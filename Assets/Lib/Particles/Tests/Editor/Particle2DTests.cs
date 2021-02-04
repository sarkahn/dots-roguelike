using NUnit.Framework;
using Sark.Particles2D;

[TestFixture]
public class ParticleTests : ECSTestBase
{
    [Test]
    public void Test()
    {
        AddSystemToWorld<Particle2DSteeringSystem>();
        AddSystemToWorld<Particle2DCleanupSystem>();

        
    }
}
