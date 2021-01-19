using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace DotsRogue
{
    public struct UILogBuffer : IBufferElementData
    {
        public FixedString128 Value;
        public static implicit operator FixedString128(UILogBuffer b) => b.Value;
        public static implicit operator UILogBuffer(FixedString128 v) =>
            new UILogBuffer { Value = v };
        public static implicit operator UILogBuffer(string str) =>
            new UILogBuffer { Value = str };
    }

    public struct HitPointsUI : IComponentData
    {
        public int max;
        public int current;
    }

    public struct UI : IComponentData
    { 
        public static void AddToEntity(EntityManager em, Entity e)
        {
        }
    }

    public struct UIJobContext
    {
        public BufferFromEntity<UILogBuffer> LogBuffer;
        public ComponentDataFromEntity<HitPointsUI> HitPointUI;
        public Entity Entity;

        public UIJobContext(SystemBase sys, Entity e, bool readOnly = false)
        {
            Entity = e;
            HitPointUI = sys.GetComponentDataFromEntity<HitPointsUI>(readOnly);
            LogBuffer = sys.GetBufferFromEntity<UILogBuffer>(readOnly);
        }

        public UIJobContext(SystemBase sys, bool readOnly = false)
            : this(sys, sys.GetSingletonEntity<UILogBuffer>(), readOnly)
        { }

        public void Log(FixedString128 str)
        {
            //Debug.Log($"Logging string len {str.Length}: {str}");
            LogBuffer[Entity].Add(str);
        }

        public void SetHitPointsUI(int current, int max)
        {
            HitPointUI[Entity] = new HitPointsUI
            {
                current = current,
                max = max
            };
        }
    }

    public class UpdateUIHealthSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<UI>();
        }
        protected override void OnUpdate()
        {
            var uiEntity = GetSingletonEntity<UI>();
            var ui = new UIJobContext(this, uiEntity, false);
            Entities
               .WithChangeFilter<HitPoints>()
               .WithAll<Player>()
               .ForEach((
                   in MaxHitPoints max,
                   in HitPoints hp) =>
               {
                   ui.SetHitPointsUI(hp, max);
               }).Schedule();
        }
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateAfter(typeof(ResolveCombatSystem))]
    public class CombatLogSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<UI>();
        }
        protected override void OnUpdate()
        {
            var uiEntity = GetSingletonEntity<UI>();
            var ui = new UIJobContext(this, uiEntity, false);
            Entities
                .WithoutBurst()
                .ForEach((in Name name,
                in DynamicBuffer<CombatDefendBuffer> defendBuffer,
                in HitPoints hp) =>
                {

                    for(int i = 0; i < defendBuffer.Length; ++i)
                    {
                        var defend = defendBuffer[i];
                        var attackerName = GetComponent<Name>(defend.Attacker);
                        int dmg = defend.Damage;
                        FixedString128 str = $"{attackerName.Value} attacked {name.Value} for {dmg} damage";

                        //Debug.Log($"STRLen from CombatLogSystem {str.Length}: {str}");
                        ui.Log(str);
                    }

                    if(hp <= 0)
                    {
                        ui.Log($"{name.Value} was killed!");
                    }
                }).Run();
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class PickupItemLogSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var uiEntity = GetSingletonEntity<UI>();
            var ui = new UIJobContext(this, uiEntity, false);
            Entities
                .WithoutBurst()
                .WithAll<Player>()
                .ForEach((in WantsToPickUpItem pickup) =>
                {
                    if(pickup.item == Entity.Null)
                    {
                        ui.Log($"There is nothing here to pick up.");
                    }
                    else
                    {
                        var itemName = GetComponent<Name>(pickup.item);
                        ui.Log($"You picked up a {itemName}.");
                    }
                }).Run();
        }
    }

}