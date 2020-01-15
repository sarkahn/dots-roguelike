using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part2
{
    public class Part2Bootstrap : MonoBehaviour
    {
        Part1SystemGroup systems;

        private void OnEnable()
        {
            systems = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<Part1SystemGroup>();
        }

        private void OnDisable()
        {
            if(systems != null && World.DefaultGameObjectInjectionWorld != null)
                World.DefaultGameObjectInjectionWorld.DestroySystem(systems);
        }

        private void Update()
        {
            systems.Update();
        }

    }


    [DisableAutoCreation]
    public class Part1SystemGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            AddSystemToUpdateList(World.GetOrCreateSystem<TileRenderSystem>());
            AddSystemToUpdateList(World.GetOrCreateSystem<MoveLeftSystem>());
            AddSystemToUpdateList(World.GetOrCreateSystem<ReadInputSystem>());
        }
    }
}