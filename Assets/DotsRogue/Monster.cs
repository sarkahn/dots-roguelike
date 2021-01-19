using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

using Sark.Common.GridUtil;
using Sark.Pathfinding;
using Sark.RNG.RandomExtensions;

using Debug = UnityEngine.Debug;

namespace DotsRogue
{
    public struct Monster : IComponentData
    {}

    [UpdateAfter(typeof(VisibilitySystem))]
    public class MonsterAISystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;
        NativeReference<Random> _rand;

        EntityQuery _monstersQuery;
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<Map>();
            RequireSingletonForUpdate<Player>();

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _monstersQuery = GetEntityQuery(
                ComponentType.ReadOnly<Monster>(),
                ComponentType.ReadOnly<MapViewBuffer>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<TakingATurn>());

            _rand = new NativeReference<Random>(Allocator.Persistent);
            // TODO: Should be seeded by some global value
            _rand.Value = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _rand.Dispose(Dependency);
        }

        protected override void OnUpdate()
        {
            var mapEntity = GetSingletonEntity<Map>();
            var mapStateData = new MapStateJobContext(this, mapEntity, false);

            var playerEntity = HasSingleton<Player>() ? 
                GetSingletonEntity<Player>() : Entity.Null;

            Dependency = new MonsterAIJob
            {
                MapStateData = mapStateData,
                EnergyHandle = GetComponentTypeHandle<Energy>(false),
                ROViewHandle = GetBufferTypeHandle<MapViewBuffer>(true),
                EntityHandle = GetEntityTypeHandle(),
                NameHandle = GetComponentTypeHandle<Name>(true),
                AttackPowerHandle = GetComponentTypeHandle<AttackPower>(true),
                PosFromEntity = GetComponentDataFromEntity<Position>(false),
                //PlayerPosition = PlayerPos,
                PlayerEntity = playerEntity,
                DefendBFE = GetBufferFromEntity<CombatDefendBuffer>(false),
                RNG = _rand,
                //PlayerData = playerPosData
            }.ScheduleSingle(_monstersQuery, Dependency);

            //PlayerPos.Dispose(Dependency);
        }

        [BurstCompile]
        struct MonsterAIJob : IJobChunk
        {
            public MapStateJobContext MapStateData;

            public Entity PlayerEntity;

            public ComponentTypeHandle<Energy> EnergyHandle;

            [ReadOnly]
            public BufferTypeHandle<MapViewBuffer> ROViewHandle;
            [ReadOnly]
            public ComponentTypeHandle<Name> NameHandle;
            [ReadOnly]
            public ComponentTypeHandle<AttackPower> AttackPowerHandle;

            // We use CDFE so we don't write to position unless
            // we're actually changing it
            public ComponentDataFromEntity<Position> PosFromEntity;
            public BufferFromEntity<CombatDefendBuffer> DefendBFE;

            [ReadOnly]
            public EntityTypeHandle EntityHandle;

            public NativeReference<Random> RNG;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var viewAccessor = chunk.GetBufferAccessor(ROViewHandle);

                var obstacles = MapStateData.Obstacles;
                var mapEntities = MapStateData.Entities;
                int playerIndex = 0;

                if(PlayerEntity != Entity.Null)
                {
                    int2 playerPos = PosFromEntity[PlayerEntity];
                    playerIndex = obstacles.PosToIndex(playerPos);
                }


                var entities = chunk.GetNativeArray(EntityHandle);

                var energyArr = chunk.GetNativeArray(EnergyHandle);
                var attackArr = chunk.GetNativeArray(AttackPowerHandle);

                //Debug.Log("Monster AI running");

                for (int i = 0; i < viewAccessor.Length; ++i)
                {
                    Entity e = entities[i];
                    var viewBuffer = viewAccessor[i].Reinterpret<bool>();
                    var pathMap = new PathingBitGrid(obstacles, Allocator.Temp);

                    //Debug.Log($"Monster is taking a turn");

                    energyArr[i] -= Energy.ActionThreshold;

                    // For testing purposes - comment out 
                    // "RequireSingletonForUpdate<Player>()" in OnCreate to 
                    // let monsters wander in "real time"
                    if(PlayerEntity == Entity.Null)
                    {
                        Wander(e, RNG, PosFromEntity, pathMap, obstacles, mapEntities);
                        return;
                    }

                    if (viewBuffer.Length == 0)
                        return;

                    bool playerIsVisible = viewBuffer[playerIndex];
                    if (playerIsVisible)
                    {
                        int2 curr = PosFromEntity[e];
                        int startIndex = obstacles.PosToIndex(curr);
                        if (startIndex == playerIndex)
                            continue;

                        // Open the player's space for pathfinding
                        pathMap[playerIndex] = false;
                        var astar = new AStar(10, Allocator.Temp);
                        var path = new NativeList<int>(10, Allocator.Temp);
                        astar.FindPath(pathMap, startIndex, playerIndex, path);
                        if (path.Length > 2)
                        {
                            //Debug.Log("MONSTER MOVED");
                            int2 next = obstacles.IndexToPos(path[1]);
                            MapUtility.MoveActor(e, PosFromEntity,
                                curr, next, obstacles, mapEntities);
                        }
                        if (path.Length == 2)
                        {
                            //Debug.Log("Monster is next to the player");
                            var defend = DefendBFE[PlayerEntity];
                            var attack = attackArr[i];
                            int dmg = RNG.RollDice(attack);
                            defend.Add(new CombatDefendBuffer
                            {
                                Attacker = e,
                                Damage = dmg
                            });
                        }
                        pathMap[playerIndex] = true;
                    }
                }
            }

            public static void Wander(Entity e, NativeReference<Random> RNG, ComponentDataFromEntity<Position> PosFromEntity, PathingBitGrid pathMap,
                GridData2D<bool> obstacles,
                GridData2D<Entity> mapEntities)
            {
                int dirIndex = RNG.NextInt(0, 8);
                int2 dir = Grid2D.Directions8Way[dirIndex];
                int2 curr = PosFromEntity[e];
                int2 next = curr + dir;
                if (!pathMap[next])
                {
                    MapUtility.MoveActor(e, PosFromEntity,
                        curr, next, obstacles, mapEntities);
                }
            }
        }
    }
}
