using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part2
{
    struct InputData : IComponentData
    {
        public float2 Value;
    }

    [DisableAutoCreation]
    [UpdateInGroup(typeof(Part1SystemGroup))]
    [AlwaysSynchronizeSystem]
    public class ReadInputSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float hor = Input.GetAxisRaw("Horizontal");
            float ver = Input.GetAxisRaw("Vertical");

            Entities.ForEach((ref InputData input) =>
            {
                input.Value.x = hor;
                input.Value.y = ver;
            }).Run();

            return default;
        }
    }

    [AlwaysSynchronizeSystem]
    public class MovePlayerSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float dt = Time.DeltaTime;
            Entities.ForEach((ref Position pos, in InputData input) =>
            {
                pos.Value += input.Value * 10 * dt;
            }).Run();

            return default;
        }
    }
}