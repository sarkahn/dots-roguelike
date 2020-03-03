using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
	[GenerateAuthoringComponent]
	public struct Strength : IComponentData
	{
		public int value;
		public static implicit operator int(Strength c) => c.value;
		public static implicit operator Strength(int v) => new Strength { value = v };
	} 
}
