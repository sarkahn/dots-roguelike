using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
	public static class MapUtility
	{
		public static bool CellIsUnblocked(int posIndex, DynamicBuffer<MapState> state)
		{
			if (posIndex < 0 || posIndex >= state.Length )
				return false;

			return !state[posIndex].blocked;
		}

		/// <summary>
		/// Clears state at <paramref name="oldIndex"/> and inserts the actor content to <paramref name="newIndex"/>.
		/// </summary>
		public static void MoveActor(Entity actor, int oldIndex, int newIndex, DynamicBuffer<MapState> state)
		{
			var cell = state[oldIndex];
			cell.blocked = false;
			cell.content = Entity.Null;
			state[oldIndex] = cell;

			cell = state[newIndex];
			cell.blocked = true;
			cell.content = actor;
			state[newIndex] = cell;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int PosToIndex(int x, int y, int width) => y * width + x;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int PosToIndex(int2 p, int width) => p.y * width + p.x;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int PosToIndex(int2 p, MapData mapData) => p.y * mapData.width + p.x;
	} 
}
