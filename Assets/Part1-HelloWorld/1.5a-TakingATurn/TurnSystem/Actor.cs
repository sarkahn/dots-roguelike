using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace RLTKTutorial.Part1_5A
{
    
    public enum ActorType : byte
    {
        Player = 0,
        Monster = 1,
    }


    public struct Actor : IComponentData
    {
        public ActorType actorType;
    }
}