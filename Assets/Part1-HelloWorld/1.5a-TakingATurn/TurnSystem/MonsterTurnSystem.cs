using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace RLTKTutorial.Part1_5A
{
    [DisableAutoCreation]
    public class MonsterTurnSystem : TurnActionSystem
	{
		public override ActorType ActorType => ActorType.Monster;

        EntityQuery _playerQuery;

        Random _rand;

        Entity _playerEntity;
        MapData _mapData;

        MoveSystem _moveSystem;

        bool _dontRun;
        
        protected override void OnCreate()
        {
            base.OnCreate();

            _playerQuery = GetEntityQuery(
                ComponentType.ReadOnly<Player>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<Name>()
                );


            _rand = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        }

        public override void OnFrameBegin()
        {
            var mapEntity = GetSingletonEntity<MapData>();

            _moveSystem = World.GetOrCreateSystem<MoveSystem>();

            // Early out if the map is regenerating.
            if (EntityManager.HasComponent<GenerateMap>(mapEntity)
             || _playerQuery.IsEmptyIgnoreFilter)
            {
                _dontRun = true;
                return;
            }

            _mapData = EntityManager.GetComponentData<MapData>(mapEntity);
            _playerEntity = _playerQuery.GetSingletonEntity();

            _dontRun = false;
        }

        protected override int OnTakeTurn(Entity e)
        {
            var dir = GetRandomDirection(ref _rand);

            _moveSystem.TryMove(e, dir);

            return Energy.ActionThreshold;
        }


        static int2 GetRandomDirection(ref Random rand)
        {
            int i = rand.NextInt(0, 5);
            switch (i)
            {
                case 0: return new int2(-1, 0);
                case 1: return new int2(1, 0);
                case 2: return new int2(0, -1);
                case 3: return new int2(0, 1);
            }
            return default;
        }
    }

}