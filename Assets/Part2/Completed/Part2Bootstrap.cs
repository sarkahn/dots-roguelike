using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part2
{
    public class Part2Bootstrap : MonoBehaviour
    {
        Part1SystemGroup _systems;

        private void OnEnable()
        {
            _systems = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<Part1SystemGroup>();
        }

        private void OnDisable()
        {
            if(_systems != null && World.DefaultGameObjectInjectionWorld != null)
                World.DefaultGameObjectInjectionWorld.DestroySystem(_systems);
        }

        private void Update()
        {
            _systems.Update();
        }

    }


    [DisableAutoCreation]
    public class Part1SystemGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            AddSystemToUpdateList(World.GetOrCreateSystem<TileRenderSystem>());
            AddSystemToUpdateList(World.GetOrCreateSystem<MoveLeftSystem>());
            AddSystemToUpdateList(World.GetOrCreateSystem<MovePlayerSystem>());
            AddSystemToUpdateList(World.GetOrCreateSystem<ReadInputSystem>());
        }
    }
}