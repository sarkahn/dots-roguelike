using Unity.Entities;
using Unity.Mathematics;

using Sark.Common.GridUtil;

namespace DotsRogue
{
	public static class MapUtility
	{
		public static bool TryMoveActor(Entity a,
			ref Position pos,
			int2 oldPos, int2 newPos,
			GridData2D<bool> obstacles,
			GridData2D<Entity> entities)
		{
			int newi = obstacles.PosToIndex(newPos);
			if (obstacles[newi])
				return false;

			pos = newPos;
			int oldi = obstacles.PosToIndex(oldPos);
			UpdateMapState(a, oldi, newi, obstacles, entities);
			return true;
		}

		public static bool TryMoveActor(Entity a,
			ComponentDataFromEntity<Position> posFromEntity,
			int2 oldPos, int2 newPos,
			GridData2D<bool> obstacles,
			GridData2D<Entity> entities)
        {
			int newi = obstacles.PosToIndex(newPos);
			if (obstacles[newi])
				return false;

			posFromEntity[a] = newPos;
			int oldi = obstacles.PosToIndex(oldPos);
			UpdateMapState(a, oldi, newi, obstacles, entities);
			return true;
        }

		public static void MoveActor(Entity e,
			ComponentDataFromEntity<Position> posFromEntity,
			int2 oldPos, int2 newPos,
			GridData2D<bool> obstacles,
			GridData2D<Entity> entities)
        {
			posFromEntity[e] = newPos;
			int oldi = obstacles.PosToIndex(oldPos);
			int newi = obstacles.PosToIndex(newPos);
			UpdateMapState(e, oldi, newi, obstacles, entities);
        }

		public static void MoveActor(Entity a,
			ref Position pos, int2 newPos,
			GridData2D<bool> obstacles,
			GridData2D<Entity> entities)
		{
			int oldi = obstacles.PosToIndex(pos);
			pos = newPos;
			int newi = obstacles.PosToIndex(newPos);
			UpdateMapState(a, oldi, newi, obstacles, entities);
		}

		public static void UpdateMapState(Entity a,
			int oldIndex, int newIndex,
			GridData2D<bool> obstacles,
			GridData2D<Entity> entities)
		{
			obstacles[oldIndex] = false;
			obstacles[newIndex] = true;
			entities[oldIndex] = default;
			entities[newIndex] = a;
		}
	} 
}