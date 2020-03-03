using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
	[DisableAutoCreation]
	[UpdateBefore(typeof(DeathSystem))]
	public class CombatSystem : SystemBase
	{
		EntityQuery _combatQuery;

		EndSimulationEntityCommandBufferSystem _endSimBarrier;

		protected override void OnCreate()
		{
			base.OnCreate();

			_endSimBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			var healthFromEntity = GetComponentDataFromEntity<Health>(false);

			Entities
				.WithStoreEntityQueryInField(ref _combatQuery)
				.ForEach((in Attacking combat, in Strength strength) =>
				{
					healthFromEntity[combat.target] -= strength;
				}).Schedule();

			_endSimBarrier.CreateCommandBuffer().RemoveComponent<Attacking>(_combatQuery);
			_endSimBarrier.AddJobHandleForProducer(Dependency);
		}
	}
}
