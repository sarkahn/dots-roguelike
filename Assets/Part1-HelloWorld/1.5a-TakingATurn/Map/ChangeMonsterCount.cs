using Unity.Entities;

namespace RLTKTutorial.Part1_5A
{
	public struct ChangeMonsterCount : IComponentData
	{
		public int value;
		public static implicit operator int (ChangeMonsterCount c) => c.value;
		public static implicit operator ChangeMonsterCount (int v) => new ChangeMonsterCount { value = v };
	} 
}
