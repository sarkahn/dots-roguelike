using Unity.Entities;
using Unity.Mathematics;

namespace DotsRogue
{
    public struct Position : IComponentData
    {
        public int2 Value;
        public static implicit operator int2(Position b) => b.Value;
        public static implicit operator Position(int2 v) =>
            new Position { Value = v };
    }

    public struct Movement : IComponentData
    {
        public int2 Value;
        public static implicit operator int2(Movement b) => b.Value;
        public static implicit operator Movement(int2 v) =>
            new Movement { Value = v };
    }

    public class Movable
    {
        public static void AddToEntity(EntityManager em, Entity e)
        {
            em.AddComponent<Position>(e);
            em.AddComponent<Movement>(e);
        }
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class MovementSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<Map>();
        }

        protected override void OnUpdate()
        {
            var mapEntity = GetSingletonEntity<Map>();
            var mapData = new MapJobContext(this, mapEntity, true);

            Entities
                .WithChangeFilter<Movement>()
                .WithReadOnly(mapData)
                .ForEach((
                    ref Position position,
                    ref Movement movement) =>
            {
                //Debug.Log($"Moving from {position.Value} to {position + (int2)movement}");
                var map = mapData.Grid;

                int2 next = position + (int2)movement;

                if(map[next] == MapTileType.Floor)
                {
                    position += (int2)movement;
                }
                movement = int2.zero;
            }).Schedule();
        }
    }
}
