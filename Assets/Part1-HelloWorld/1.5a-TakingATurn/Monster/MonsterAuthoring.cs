using RLTK;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RLTKTutorial.Part1_5A
{
    struct Monster : IComponentData
    { }

    [RequiresEntityConversion]
    public class MonsterAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        char _glyph = 'o';

        [SerializeField]
        Color _color = Color.red;

        [SerializeField]
        int _speed = 25;

        [SerializeField]
        int _viewRange = 8;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Monster>(entity);
            dstManager.AddComponent<Position>(entity);
            dstManager.AddComponent<Movement>(entity);
            dstManager.AddComponent<TilesInView>(entity);
            dstManager.AddComponent<Energy>(entity);
            dstManager.AddComponent<Prefab>(entity);

            dstManager.AddComponentData<ViewRange>(entity, _viewRange);
            dstManager.AddComponentData<Speed>(entity, _speed);
            dstManager.AddComponentData<Name>(entity, name);
            dstManager.AddComponentData<Actor>(entity, new Actor
            {
                actorType = ActorType.Monster
            });

            dstManager.AddComponentData<Renderable>(entity, new Renderable
            {
                bgColor = Color.black,
                fgColor = _color,
                glyph = CodePage437.ToCP437(_glyph)
            });
        }
    }
}