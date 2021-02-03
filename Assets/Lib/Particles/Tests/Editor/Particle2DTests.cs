using NUnit.Framework;
using Sark.Particles2D;

[TestFixture]
public class ParticleTests : ECSTestBase
{
    [Test]
    public void Test()
    {
        AddSystemToWorld<Particle2DMainUpdateSystem>();
        AddSystemToWorld<Particle2DCleanupParticlesSystem>();

        
    }
}
