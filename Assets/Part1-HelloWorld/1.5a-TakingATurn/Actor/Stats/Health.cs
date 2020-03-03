using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
	[GenerateAuthoringComponent]
	public struct Health : IComponentData
	{
		public int value;
		public static implicit operator int(Health c) => c.value;
		public static implicit operator Health(int v) => new Health { value = v };
	} 
}
