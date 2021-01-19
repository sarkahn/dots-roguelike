using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using Sark.RNG;

namespace DotsRogue
{
    public struct Item : IComponentData
    { 
        public static void AddToEntity(EntityManager em, Entity e)
        {
            em.AddComponent<Item>(e);
        }
    }

    public struct InInventory : IComponentData
    {
        public Entity owner;
    }

    public struct GiveItem : IComponentData
    {
        public Entity item;
        public Entity receiver;
        public int amount;
    }

    public struct TryUseItem : IComponentData
    {
        public Entity user;
        public Entity target;
    }

    public struct Consumable : IComponentData
    {
        public int uses;
    }

    public struct HealsOnUse : IComponentData
    {
        public int healAmount;
    }

    public struct InflictsDamage : IComponentData
    {
        public DiceValue damage;
    }

    public struct Ranged : IComponentData
    {
        public int range;
    }

    public struct WantsToPickUpItem : IComponentData
    {
        public Entity item;
        public Entity collector;
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class PickUpItemSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;
        EntityQuery _eq;
        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer();
            Entities
                .WithStoreEntityQueryInField(ref _eq)
                .ForEach((Entity e, in WantsToPickUpItem pickup) =>
                {
                    if (pickup.item == Entity.Null)
                        return;

                    ecb.RemoveComponent<Position>(pickup.item);
                    ecb.AddComponent(pickup.item, new InInventory
                    {
                        owner = e
                    });
                }).Schedule();
            ecb.RemoveComponent<WantsToPickUpItem>(_eq);
            _barrier.AddJobHandleForProducer(Dependency);
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class GiveItemSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer();
            Entities
                .ForEach((Entity e,in GiveItem give) =>
                {
                    for (int i = 0; i < give.amount; ++i)
                    {
                        var item = ecb.Instantiate(give.item);
                        ecb.AddComponent<InInventory>(item, new InInventory
                        {
                            owner = give.receiver
                        });
                    }
                    ecb.DestroyEntity(e);
                }).Schedule();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class ConsumableSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer();
            Entities
                .WithChangeFilter<Consumable>()
                .ForEach((Entity e, in Item item, in Consumable consumable) =>
                {
                    if (consumable.uses <= 0)
                    {
                        ecb.DestroyEntity(e);
                    }
                }).Schedule();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }

    public class UseHealingItemSystem : SystemBase
    {
        EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ui = new UIJobContext(this, false);
            var stateCTX = new GameStateJobContext(this);
            var ecb = _barrier.CreateCommandBuffer();
            Entities.WithoutBurst().ForEach((Entity e, 
                in Name name,
                in Item item,
                in TryUseItem use,
                in HealsOnUse potion) =>
            {
                if (!HasComponent<HitPoints>(use.target) || !HasComponent<MaxHitPoints>(use.target))
                {
                    ui.Log("Invalid target.");
                    return;
                }

                int hp = GetComponent<HitPoints>(use.target);
                int max = GetComponent<MaxHitPoints>(use.target);
                int original = hp;
                hp = math.min(max, hp + potion.healAmount);

                SetComponent<HitPoints>(use.target, hp);

                if(HasComponent<Player>(use.target))
                {
                    FixedString128 str = $"You use the {name.Value} and heal {hp - original} hit points.";
                    ui.Log(str);
                    stateCTX.ChangeGameState<GameStateTakingTurns>(ecb);
                }

                if (HasComponent<Consumable>(e))
                {
                    var consumable = GetComponent<Consumable>(e);
                    consumable.uses--;
                    if (consumable.uses <= 0)
                        ecb.DestroyEntity(e);
                    else
                        SetComponent(e, consumable);
                }
            }).Schedule();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }

    // TODO : Components and system name doesn't really make sense yet
    public class InflictDamageItemSelection : SystemBase
    {
        EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            RequireSingletonForUpdate<GameStateShowInventory>();
        }

        protected override void OnUpdate()
        {
            var ecb = _barrier.CreateCommandBuffer();
            var stateCTX = new GameStateJobContext(this);

            Entities
                .ForEach((Entity e, in TryUseItem use, in InflictsDamage dmg, in Ranged range) =>
                {
                    stateCTX.ChangeGameState(ecb, new GameStateShowTargeting
                    {
                        range = range.range,
                        source = e,
                        target = default
                    });
                }).Schedule();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }

    public class InflictDamageTargetSelected : SystemBase
    {
        EntityCommandBufferSystem _barrier;
        EntityQuery _targetStateChangedQuery;

        NativeReference<Random> _rand;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<GameStateShowTargeting>();
            RequireSingletonForUpdate<Player>();

            _targetStateChangedQuery = GetEntityQuery(ComponentType.ReadOnly<GameStateShowTargeting>());
            _targetStateChangedQuery.AddChangedVersionFilter(typeof(GameStateShowTargeting));

            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _rand = new NativeReference<Random>(Allocator.Persistent); 
            _rand.Value = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _rand.Dispose(Dependency);
        }

        protected override void OnUpdate()
        {
            var stateCTX = new GameStateJobContext(this);
            var ecb = _barrier.CreateCommandBuffer();
            var rand = _rand;
            var playerEntity = GetSingletonEntity<Player>();
            var uiCTX = new UIJobContext(this);

            Entities
            .WithoutBurst()
            .ForEach((Entity e, in Item item, in TryUseItem use, in InflictsDamage dmg,
            in InInventory inventory, in Name name) =>
            {
                var tarState = GetComponent<GameStateShowTargeting>(stateCTX.entity);
                Entity tar = tarState.target;
                if (tar == Entity.Null)
                    return;

                var hp = GetComponent<HitPoints>(tar);
                int damage = dmg.damage.Roll(rand);
                int start = hp.Value;
                hp.Value = math.max(hp.Value - damage, 0);
                int diff = start - hp.Value;
                SetComponent<HitPoints>(tar, hp);

                ecb.RemoveComponent<TryUseItem>(e);
                stateCTX.ChangeGameState<GameStateTakingTurns>(ecb);

                if(inventory.owner == playerEntity)
                {
                    var tarName = GetComponent<Name>(tar);
                    uiCTX.Log($"You use the {name.Value} on the {tarName.Value} and deal {diff} damage.");
                }

                if(HasComponent<Consumable>(e))
                {
                    var con = GetComponent<Consumable>(e);
                    con.uses--;
                    SetComponent<Consumable>(e, con);
                }
            }).Schedule();

            _barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
