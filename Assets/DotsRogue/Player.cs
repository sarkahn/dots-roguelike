using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using Sark.Common.GridUtil;
using Sark.RNG.RandomExtensions;

using Input = UnityEngine.Input;
using KeyCode = UnityEngine.KeyCode;

namespace DotsRogue
{
    public struct Player : IComponentData
    { 
    }

    public struct PlayerJobContext
    {
        public Entity entity;
        ComponentDataFromEntity<Position> posFromEntity;
        BufferFromEntity<MapViewBuffer> mapViewFromEntity;

        public PlayerJobContext(SystemBase system, bool readOnly)
        {
            entity = system.GetSingletonEntity<Player>();
            posFromEntity = system.GetComponentDataFromEntity<Position>(readOnly);
            mapViewFromEntity = system.GetBufferFromEntity<MapViewBuffer>(readOnly);
        }

        public int2 Position
        {
            get => posFromEntity[entity];
            set => posFromEntity[entity] = value;
        }

        public GridData2D<bool> GetMapView(int2 mapSize)
        {
            var viewArr = mapViewFromEntity[entity].Reinterpret<bool>().AsNativeArray();
            var grid = new GridData2D<bool>(viewArr, mapSize);
            return grid;
        }
    }

    public class PlayerTurnSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;
        NativeReference<Random> _RNG;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<Player>();
            RequireSingletonForUpdate<Map>();
            RequireSingletonForUpdate<GameStateTakingTurns>();

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _RNG = new NativeReference<Random>(Allocator.Persistent);
            // TODO: Should be seeded by some global value
            _RNG.Value = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _RNG.Dispose(Dependency);
        }

        static bool IsPressed(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        // Not great...done this way to be somewhat easy to switch back to Project Tiny.
        static int2 GetMoveInput()
        {
            int hor = 0;
            int ver = 0;

            if (IsPressed(KeyCode.A) || IsPressed(KeyCode.Keypad4) ||
                IsPressed(KeyCode.LeftArrow))
                hor = -1;
            if (IsPressed(KeyCode.D) || IsPressed(KeyCode.Keypad6) ||
                IsPressed(KeyCode.RightArrow))
                hor = 1;
            if (IsPressed(KeyCode.W) || IsPressed(KeyCode.Keypad8) ||
                IsPressed(KeyCode.UpArrow))
                ver = 1;
            if (IsPressed(KeyCode.S) || IsPressed(KeyCode.Keypad2) ||
                IsPressed(KeyCode.DownArrow))
                ver = -1;

            if (IsPressed(KeyCode.Keypad9) ||
                IsPressed(KeyCode.PageUp) || IsPressed(KeyCode.E))
            {
                hor = 1;
                ver = 1;
            }
            if (IsPressed(KeyCode.Keypad7) ||
                IsPressed(KeyCode.Home) || IsPressed(KeyCode.Q))
            {
                hor = -1;
                ver = 1;
            }
            if (IsPressed(KeyCode.Keypad1) || IsPressed(KeyCode.End) ||
                IsPressed(KeyCode.Z))
            {
                hor = -1;
                ver = -1;
            }
            if (IsPressed(KeyCode.Keypad3) || IsPressed(KeyCode.PageDown) ||
                IsPressed(KeyCode.C))
            {
                hor = 1;
                ver = -1;
            }
            return new int2(hor, ver);
        }

        static bool GetWaitInput()
        {
            return IsPressed(KeyCode.Space) || IsPressed(KeyCode.Keypad5);
        }

        static bool GetPickupInput()
        {
            return IsPressed(KeyCode.G);
        }

        static bool GetDisplayInventoryInput()
        {
            return IsPressed(KeyCode.I) || IsPressed(KeyCode.B);
        }

        protected override void OnUpdate()
        {
            DoMove();
            DoPickup();
            DoShowInventory();
            _barrier.AddJobHandleForProducer(Dependency);
        }

        void DoMove()
        {
            Entities.WithStructuralChanges().ForEach((Entity e) =>
            {
                UnityEngine.Debug.Log(e);
            }).Run();
            int2 moveInput = GetMoveInput();
            bool wait = GetWaitInput();

            if (math.all(moveInput == 0) && !wait)
                return;

            var mapEntity = GetSingletonEntity<Map>();
            var mapStateData = new MapStateJobContext(this, mapEntity, false);
            var rng = _RNG;

            Entities
                //.WithoutBurst()
                .WithNone<AttackPower>()
                .WithAll<Player, TakingATurn>()
                .ForEach((Entity e,
                 ref Energy energy,
                 ref Position curr) =>
            {
                if (wait)
                {
                    energy = 0;
                    return;
                }

                var obstacles = mapStateData.Obstacles;
                var entities = mapStateData.Entities;

                int2 dest = curr + moveInput;

                int cost = 0;

                bool destIsObstacle = obstacles[dest];
                Entity targetEntity = entities[dest];
                //Debug.Log("Trying to move player...");
                if (!destIsObstacle)
                {
                    //Debug.Log("PLAYER MOVED"); 
                    MapUtility.MoveActor(e, ref curr, dest, obstacles, entities);
                    cost = Energy.ActionThreshold;
                }
                energy -= cost;
            }).Schedule();

            Entities
                //.WithoutBurst()
                .WithAll<Player, TakingATurn>()
                .ForEach((Entity e,
                    ref Energy energy,
                    ref Position curr,
                    in AttackPower attack) =>
            {
                if (wait)
                {
                    energy = 0;
                    return;
                }

                var obstacles = mapStateData.Obstacles;
                var entities = mapStateData.Entities;

                int2 dest = curr + moveInput;

                int cost = 0;

                bool destIsObstacle = obstacles[dest];
                Entity targetEntity = entities[dest];
                //Debug.Log("Trying to move player...");
                if (!destIsObstacle)
                {
                    //Debug.Log("PLAYER MOVED"); 
                    MapUtility.MoveActor(e, ref curr, dest, obstacles, entities);
                    cost = Energy.ActionThreshold;
                }
                else if (HasComponent<Monster>(targetEntity))
                {
                    var name = GetComponent<Name>(targetEntity);
                    var defendBuffer = GetBuffer<CombatDefendBuffer>(targetEntity);
                    defendBuffer.Add(new CombatDefendBuffer
                    {
                        Attacker = e,
                        Damage = rng.RollDice(attack)
                    });
                    cost = Energy.ActionThreshold;
                }
                energy -= cost;
                //Debug.Log("Player took their turn");
            }).Schedule();
        }

        void DoPickup()
        {
            if (!GetPickupInput())
                return;

            var playerEntity = GetSingletonEntity<Player>();

            if (!HasComponent<TakingATurn>(playerEntity))
                return;

            EntityManager.AddComponentData(playerEntity, new WantsToPickUpItem
            {
                collector = playerEntity,
                item = Entity.Null
            });

            var playerPos = EntityManager.GetComponentData<Position>(playerEntity);
            Entities
                .ForEach((int entityInQueryIndex, Entity e,
                in Item item, in Position pos) =>
            {
                if (!pos.Value.Equals(playerPos.Value))
                    return;

                SetComponent(playerEntity, new WantsToPickUpItem
                {
                    collector = playerEntity,
                    item = e
                });
                SetComponent<Energy>(playerEntity, 0);
            }).Run();
        }

        void DoShowInventory()
        {
            if (!GetDisplayInventoryInput())
                return;

            var ecb = _barrier.CreateCommandBuffer();
            var stateCTX = new GameStateJobContext(this);
            stateCTX.ChangeGameState<GameStateShowInventory>(ecb); 
        }
    }

}
