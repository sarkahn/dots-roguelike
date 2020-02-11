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
        public class RLTKTutorialSystems : ComponentSystemGroup { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World.DefaultGameObjectInjectionWorld,
               new List<Type>() {
                        typeof(RLTKTutorialSystems)
               });
        }

        public static void AddSystem<T>() where T : ComponentSystemBase
        {
            var group = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<RLTKTutorialSystems>();
            var system = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<T>();
            group.AddSystemToUpdateList(system);
        }

    }
}