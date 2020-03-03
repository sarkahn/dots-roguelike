using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
	/// <summary>
	/// Maintains and updates map state by tracking actor changes.
	/// </summary>
	[DisableAutoCreation]
	[UpdateAfter(typeof(GenerateMapSystem))]
	public class UpdateMapStateSystem : SystemBase
	{
		EntityQuery _mapQuery;
		EntityQuery _actorQuery;

		EndSimulationEntityCommandBufferSystem _endSimBarrier;

		protected override void OnCreate()
		{
			base.OnCreate();

			_mapQuery = GetEntityQuery(
				ComponentType.ReadWrite<MapState>(),
				ComponentType.ReadOnly<MapTiles>()
				);

			_actorQuery = GetEntityQuery(
				typeof(Collidable),
				typeof(CollidableStateComponent)
				);

			_endSimBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		}

		protected override void OnUpdate()
		{
			var mapEntity = _mapQuery.GetSingletonEntity();
			var mapState = EntityManager.GetBuffer<MapState>(mapEntity);
			var mapTiles = EntityManager.GetBuffer<MapTiles>(mapEntity);
			var mapData = EntityManager.GetComponentData<MapData>(mapEntity);

			var buffer = _endSimBarrier.CreateCommandBuffer();

			if (EntityManager.HasComponent<GenerateMap>(mapEntity))
			{
				OnMapResized(mapTiles, mapState, mapData, buffer);
				return;
			}

			CleanDestroyedActors(mapState, mapData, buffer);

			HandleNewActors(mapState, mapData, buffer);

			HandleMovedActors(mapState, mapData);

			_endSimBarrier.AddJobHandleForProducer(Dependency);
		}

		// Reset all state immediately
		void OnMapResized(
			DynamicBuffer<MapTiles> mapTiles, 
			DynamicBuffer<MapState> mapState,
			MapData mapData,
			EntityCommandBuffer buffer )
		{
			Job
				.WithCode(() =>
				{
					mapState.ResizeUninitialized(mapTiles.Length);
					for (int i = 0; i < mapTiles.Length; ++i)
					{
						mapState[i] = new MapState
						{
							blocked = mapTiles[i] == TileType.Wall,
						};
					}
				}).Run();

			EntityManager.RemoveComponent<CollidableStateComponent>(_actorQuery);
		}

		void CleanDestroyedActors(DynamicBuffer<MapState> mapState, MapData mapData, EntityCommandBuffer buffer)
		{
			int mapWidth = mapData.width;

			Entities
				.WithNone<Collidable>()
				.ForEach((int entityInQueryIndex, Entity e, in CollidableStateComponent state) =>
				{
					var p = state.prevPos;
					int index = MapUtility.PosToIndex(p, mapWidth);

					// Clear the cell
					mapState[index] = default;

					buffer.RemoveComponent<CollidableStateComponent>(e);
				}).Schedule();
		}

		void HandleNewActors(DynamicBuffer<MapState> mapState, MapData mapData, EntityCommandBuffer buffer)
		{
			int mapWidth = mapData.width;

			Entities
				.WithAll<Collidable>()
				.WithNone<CollidableStateComponent>()
				.ForEach((int entityInQueryIndex, Entity e, in Position pos) =>
				{
					int2 p = pos;
					int index = MapUtility.PosToIndex(p, mapWidth);
					var cell = mapState[index];
					cell.content = e;
					cell.blocked = true;
					mapState[index] = cell;

					buffer.AddComponent<CollidableStateComponent>(e,
						new CollidableStateComponent
						{
							prevPos = pos
						});

				}).Schedule();
		}

		void HandleMovedActors(DynamicBuffer<MapState> mapState, MapData mapData)
		{
			int mapWidth = mapData.width;

			Entities
				.WithChangeFilter<Position>()
				.WithAll<Collidable>()
				.ForEach((Entity e, ref CollidableStateComponent state, in Position pos) =>
				{
					int2 p = pos;
					int2 prev = state.prevPos;
					if (p.x != prev.x || p.y != prev.y)
					{
						int oldIndex = MapUtility.PosToIndex(prev, mapWidth);
						int newIndex = MapUtility.PosToIndex(p, mapWidth);

						MapUtility.MoveActor(e, oldIndex, newIndex, mapState);
					}

					state.prevPos = pos;
				}).Schedule();
		}
	}
}
