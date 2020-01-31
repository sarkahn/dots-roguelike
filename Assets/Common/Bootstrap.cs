using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial
{
    public abstract class Bootstrap : MonoBehaviour
    {
        protected abstract void AddSystems();

        BootstrapGroup _systemGroup;
        List<System.Type> _systems = new List<System.Type>();

        private void OnEnable()
        {
            _systemGroup = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BootstrapGroup>();
            
            AddSystems();
            foreach (var t in _systems)
            {
                var s = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem(t);
                _systemGroup.AddSystemToUpdateList(s);
            }
        }

        protected void AddSystem<T>() where T : ComponentSystemBase
        {
            _systems.Add(typeof(T));
        }

        private void OnDisable()
        {
            if (_systemGroup != null && World.DefaultGameObjectInjectionWorld != null)
                World.DefaultGameObjectInjectionWorld.DestroySystem(_systemGroup);
        }

        private void Update()
        {
            _systemGroup.Update();
        }


        [DisableAutoCreation]
        internal class BootstrapGroup : ComponentSystemGroup
        {

            protected override void OnCreate()
            {
            }
        }
    }


}