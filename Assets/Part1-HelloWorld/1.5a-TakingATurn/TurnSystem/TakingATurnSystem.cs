using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public abstract class TurnActionSystem : SystemBase
{
    EntityQuery _actors;

    protected virtual void OnTurnBegin() { }

    protected virtual void OnTurnEnd() { }

    protected abstract JobHandle OnTurnUpdate(JobHandle inputDeps);

    void ActionComplete(int cost)
    {

    }

    protected abstract EntityQuery CreateActorQuery();
    
    protected override sealed void OnUpdate()
    {
    }
}

