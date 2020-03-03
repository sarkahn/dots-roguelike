using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial
{
    /// <summary>
    /// Utility class for manually adding systems to the automatic update loop.
    /// These systems will appear under "RLTKTutorialSystems" in the entity debugger.
    /// </summary>
    public static class Bootstrap
    {

        public static void AddInitSystem<T>() where T : ComponentSystemBase
        {
            AddSystemToGroup<T, InitializationSystemGroup>();
        }

        public static void AddSimSystem<T>() where T : ComponentSystemBase
        {
            AddSystemToGroup<T, SimulationSystemGroup>();
        }

        public static void AddLateSimSystem<T>() where T: ComponentSystemBase
        {
            AddSystemToGroup<T, LateSimulationSystemGroup>();
        }

        public static void AddRenderSystem<T>() where T : ComponentSystemBase
        {
            AddSystemToGroup<T,PresentationSystemGroup>();
        }

        static void AddSystemToGroup<TSystem,TGroup>()
            where TSystem : ComponentSystemBase
            where TGroup : ComponentSystemGroup
        {
            var group = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<TGroup>();
            var system = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<TSystem>();

            group.AddSystemToUpdateList(system);
        }

    }
}