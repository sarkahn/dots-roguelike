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

            if (_dontRun)
                return -1;

            var playerPos = (int2)EntityManager.GetComponentData<Position>(_playerEntity);
            var playerName = EntityManager.GetComponentData<Name>(_playerEntity);

            int playerIndex = playerPos.y * _mapData.width + playerPos.x;

            //StandAndDeliver(playerIndex, buffer);
            Wander(e);

            return 100;
        }

        


        void StandAndDeliver(int playerIndex, EntityCommandBuffer buffer)
        {
            Entities
                .WithoutBurst()
                .WithAll<Monster>()
                .ForEach((int entityInQueryIndex, Entity e, in DynamicBuffer<TilesInView> view, in Name name) =>
                {
                    var action = new ActionPerformed();

                    if (view[playerIndex])
                    {
                        //Debug.Log($"{name} shouts angrily at {playerName}");
                        action.cost = 100;
                    }
                    else
                    {
                        //Debug.Log($"{name} stands around");
                        action.cost = 50;
                    }

                    buffer.AddComponent(e, action);
                }).Run();
        }

        void Wander(Entity e)
        {
            var rand = _rand;

            // Could do a foreach on all monsters in OnFrameBegin and cache this value for every
            // monster.
            var dir = GetRandomDirection(ref rand);

            EntityManager.SetComponentData<Movement>(e, dir);

            _rand = rand;
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