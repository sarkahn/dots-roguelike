using Unity.Collections;
using Unity.Entities;

namespace DotsRogue
{
    public struct Name : IComponentData
    {
        public FixedString128 Value;
        public static implicit operator FixedString128(Name b) => b.Value;
        public static implicit operator Name(FixedString128 v) =>
            new Name { Value = v };
        public static implicit operator Name(string str) =>
            new Name { Value = str };
        public static implicit operator string(Name name) =>
            name.Value.ToString();

        public override string ToString()
        {
            return Value.ToString();
        }

        public static void AddToEntity(EntityManager em, Entity e, string str)
        {
            em.AddComponentData<Name>(e, str);
        }
    }

#if UNITY_EDITOR
    struct Named : IComponentData { }
    public class DebugNameSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithNone<Named>()
                .WithStructuralChanges()
                .ForEach((Entity e, in Name name) =>
                {
                    EntityManager.SetName(e, name);
                    EntityManager.AddComponent<Named>(e);
                }).Run();
        }
    }
#endif
}
