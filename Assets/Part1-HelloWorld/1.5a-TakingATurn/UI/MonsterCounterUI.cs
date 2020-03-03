using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace RLTKTutorial.Part1_5A
{
    public class MonsterCounterUI : MonoBehaviour
    {
        [SerializeField]
        Text _counterText = null;

        void Update()
        {
            _counterText.text = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MonsterCounterUISystem>().MonsterCount.ToString();
        }

        [DisableAutoCreation]
        internal class MonsterCounterUISystem : SystemBase
        {
            EntityQuery _monsterQuery;

            public int MonsterCount => _monsterQuery.CalculateEntityCount();

            protected override void OnCreate() => _monsterQuery = GetEntityQuery(ComponentType.ReadOnly<Monster>());

            protected override void OnUpdate()
            {}
        }
    }

}