using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace RLTKTutorial.Part1_1
{

    public class CreateEntities : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var em = dstManager;
            var player = entity;
            dstManager.AddComponentData<Position>(player, new float2(10, 8));
            dstManager.AddComponentData<Renderable>(player, new Renderable
            {
                FGColor = Color.yellow,
                BGColor = Color.black,
                Glyph = RLTK.CodePage437.ToCP437('@')
            });
            dstManager.AddComponentData<InputData>(player, new InputData());

            for (int i = 0; i < 10; ++i)
            {
                var e = conversionSystem.CreateAdditionalEntity(gameObject);
                var renderable = new Renderable
                {
                    FGColor = Color.red,
                    BGColor = Color.black,
                    Glyph = RLTK.CodePage437.ToCP437('☺')
                };
                dstManager.AddComponentData<Position>(e, new float2(i * 3, 13));
                dstManager.AddComponentData(e, renderable);
                dstManager.AddComponentData(e, new MoveLeft { Speed = 15 });
            }
        }
    }

}