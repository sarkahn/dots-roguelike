using System;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Profiling;

using Unity.Entities;
using System.Collections.Generic;

namespace RLTKTutorial.Common.Tests
{

    // From nunit docs for SetUpFixture-Attribute
    // "A SetUpFixture outside of any namespace provides SetUp and TearDown for the entire assembly."
    [SetUpFixture]
    public class NUnitAssemblyWideSetupEntitiesTests
    {
        [OneTimeSetUp]
        public void Init()
        {
            // TODO This breaks specific SubSceneEditorTests. Commenting for now, but the init/shutdown was
            // fixing undeallocated memory complaints, which need to be addressed.
            //
            // Old comment: 
            // This isn't necessary and is initialized through World.Initialize()->...->EntityManager()->TypeManager.Initialize()
            // but because we shutdown in tests for explicit cleanup, match it with explicit init here.
            //Unity.Entities.TypeManager.Initialize();

            NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace; // Should have stack trace with tests
        }

        [OneTimeTearDown]
        public void Exit()
        {
            // TODO This breaks specific SubSceneEditorTests. Commenting for now, but the init/shutdown was
            // fixing undeallocated memory complaints, which need to be addressed.
            //
            // Old comment: 
            // Avoid a number of memory leak complaints in tests.
            //Unity.Entities.TypeManager.Shutdown();
        }

    }

#if NET_DOTS
    public class EmptySystem : ComponentSystem
    {
        protected override void OnUpdate()
        {

        }
        public new EntityQuery GetEntityQuery(params EntityQueryDesc[] queriesDesc)
        {
            return base.GetEntityQuery(queriesDesc);
        }

        public new EntityQuery GetEntityQuery(params ComponentType[] componentTypes)
        {
            return base.GetEntityQuery(componentTypes);
        }
        public new EntityQuery GetEntityQuery(NativeArray<ComponentType> componentTypes)
        {
            return base.GetEntityQuery(componentTypes);
        }
        public BufferFromEntity<T> GetBufferFromEntity<T>(bool isReadOnly = false) where T : struct, IBufferElementData
        {
            AddReaderWriter(isReadOnly ? ComponentType.ReadOnly<T>() : ComponentType.ReadWrite<T>());
            return EntityManager.GetBufferFromEntity<T>(isReadOnly);
        }
    }
#else
    public class EmptySystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle dep) { return dep; }


        new public EntityQuery GetEntityQuery(params EntityQueryDesc[] queriesDesc)
        {
            return base.GetEntityQuery(queriesDesc);
        }

        new public EntityQuery GetEntityQuery(params ComponentType[] componentTypes)
        {
            return base.GetEntityQuery(componentTypes);
        }
        new public EntityQuery GetEntityQuery(NativeArray<ComponentType> componentTypes)
        {
            return base.GetEntityQuery(componentTypes);
        }
    }

#endif

    public abstract class ECSTestsFixture
    {
        protected World m_PreviousWorld;
        protected World World;
        protected EntityManager m_Manager;
        protected EntityManager.EntityManagerDebug m_ManagerDebug;

        List<ComponentSystemBase> _systems = new List<ComponentSystemBase>();
        
        protected int StressTestEntityCount = 1000;

        [SetUp]
        public virtual void Setup()
        {
            // Redirect Log messages in NUnit which get swallowed (from GC invoking destructor in some cases)
            // System.Console.SetOut(NUnit.Framework.TestContext.Out);

            m_PreviousWorld = World.DefaultGameObjectInjectionWorld;
#if !UNITY_DOTSPLAYER
            World = World.DefaultGameObjectInjectionWorld = new World("Test World");
            
            
#else
            Unity.Burst.DotsRuntimeInitStatics.Init();
            World = DefaultTinyWorldInitialization.Initialize("Test World");
#endif

            m_Manager = World.EntityManager;
            m_ManagerDebug = new EntityManager.EntityManagerDebug(m_Manager);

            _systems.Clear();

#if !UNITY_DOTSPLAYER
#if !UNITY_2019_2_OR_NEWER
            // Not raising exceptions can easily bring unity down with massive logging when tests fail.
            // From Unity 2019.2 on this field is always implicitly true and therefore removed.

            UnityEngine.Assertions.Assert.raiseExceptions = true;
#endif  // #if !UNITY_2019_2_OR_NEWER
#endif  // #if !UNITY_DOTSPLAYER
        }

        [TearDown]
        public virtual void TearDown()
        {
            if (m_Manager != null && m_Manager.IsCreated)
            {
                // Clean up systems before calling CheckInternalConsistency because we might have filters etc
                // holding on SharedComponentData making checks fail
                while (World.Systems.ToArray().Length > 0)
                {
                    World.DestroySystem(World.Systems.ToArray()[0]);
                }

                m_ManagerDebug.CheckInternalConsistency();

                World.Dispose();
                World = null;

                //   World.DefaultGameObjectInjectionWorld = m_PreviousWorld;
                //  m_PreviousWorld = null;
                //   m_Manager = null;

            }

#if UNITY_DOTSPLAYER
            // TODO https://unity3d.atlassian.net/browse/DOTSR-119
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.FreeTempMemory();
#endif

            // Restore output
            var standardOutput = new System.IO.StreamWriter(System.Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            System.Console.SetOut(standardOutput);
        }


        public void AssertSameChunk(Entity e0, Entity e1)
        {
            Assert.AreEqual(m_Manager.GetChunk(e0), m_Manager.GetChunk(e1));
        }

        public void AssertHasVersion<T>(Entity e, uint version) where T :
#if UNITY_DISABLE_MANAGED_COMPONENTS
            struct, 
#endif
            IComponentData
        {
            var type = m_Manager.GetArchetypeChunkComponentType<T>(true);
            var chunk = m_Manager.GetChunk(e);
            Assert.AreEqual(version, chunk.GetComponentVersion(type));
            Assert.IsFalse(chunk.DidChange(type, version));
            Assert.IsTrue(chunk.DidChange(type, version - 1));
        }

        public void AssertHasBufferVersion<T>(Entity e, uint version) where T : struct, IBufferElementData
        {
            var type = m_Manager.GetArchetypeChunkBufferType<T>(true);
            var chunk = m_Manager.GetChunk(e);
            Assert.AreEqual(version, chunk.GetComponentVersion(type));
            Assert.IsFalse(chunk.DidChange(type, version));
            Assert.IsTrue(chunk.DidChange(type, version - 1));
        }

        public void AssertHasSharedVersion<T>(Entity e, uint version) where T : struct, ISharedComponentData
        {
            var type = m_Manager.GetArchetypeChunkSharedComponentType<T>();
            var chunk = m_Manager.GetChunk(e);
            Assert.AreEqual(version, chunk.GetComponentVersion(type));
            Assert.IsFalse(chunk.DidChange(type, version));
            Assert.IsTrue(chunk.DidChange(type, version - 1));
        }


        protected void AddSystem<T>() where T : ComponentSystemBase
        {
            _systems.Add(World.GetOrCreateSystem<T>());
        }

        protected void Update()
        {
            foreach (var s in _systems)
                s.Update();
        }


        public EmptySystem EmptySystem
        {
            get
            {
                return World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EmptySystem>();
            }
        }
    }
}