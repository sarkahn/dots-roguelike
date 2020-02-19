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
        [UpdateInGroup(typeof(SimulationSystemGroup))]
        public class RLTKSimSystems : ComponentSystemGroup { }

        [UpdateInGroup(typeof(PresentationSystemGroup))]
        public class RLTKRenderSystems : ComponentSystemGroup { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            //DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World.DefaultGameObjectInjectionWorld,
             //  new List<Type>() {
            //            typeof(RLTKSimSystems)
             //  });
        }

        public static void AddSimSystem<T>() where T : ComponentSystemBase
        {
            var group = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<RLTKSimSystems>();
            var system = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<T>();
            group.AddSystemToUpdateList(system);
        }

        public static void AddRenderSystem<T>() where T : ComponentSystemBase
        {
            var group = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<RLTKRenderSystems>();
            var system = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<T>();

            group.AddSystemToUpdateList(system);
        }

    }
}