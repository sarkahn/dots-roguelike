using NUnit.Framework;
using System.Runtime.InteropServices;
using Unity.Entities;
using UnityEngine;

// Credit to https://github.com/5argon/EcsTesting\
public class ECSTestBase
{
    protected World World { get; private set; }
    protected EntityManager EntityManager => World.EntityManager;

    protected EntityQuery CreateEntityQuery(params ComponentType[] components) => EntityManager.CreateEntityQuery(components);

    protected EntityQuery GetEntityQuery(params ComponentType[] components) =>
        CreateEntityQuery(components);

    [SetUp]
    public void SetUpBase()
    {
        DefaultWorldInitialization.DefaultLazyEditModeInitialize();
        World = World.DefaultGameObjectInjectionWorld;
        // Unity lazily generates the world time component on the first 
        // world update. This will cause a structural  change which 
        // could invalidate certain state during testing
        World.Update();
    }

    [TearDown]
    public void TearDown()
    {
        World.Dispose();
    }

    // Add a system to the world - repects the systems'
    // [UpdateInGroup] and [UpdateBefore/After] attribute settings
    protected T AddSystemToWorld<T>() where T : SystemBase
    {
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(
            World,
            typeof(T));
        return World.GetExistingSystem<T>();
    }
}