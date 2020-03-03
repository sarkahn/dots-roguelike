using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace RLTKTutorial.Part1_5A
{
	public struct CollidableStateComponent : ISystemStateComponentData
	{
		public int2 prevPos;
	} 
}
