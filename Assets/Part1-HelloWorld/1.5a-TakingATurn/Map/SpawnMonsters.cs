using UnityEngine;
using System.Collections;
using Unity.Entities;

namespace RLTKTutorial.Part1_5A
{
	public struct ChangeMonsterCount : IComponentData
	{
		public int count;
	}
}
