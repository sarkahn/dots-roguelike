using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[DisableAutoCreation]
public abstract class TakingATurnSystem : JobComponentSystem
{
    protected virtual void OnTurnBegin() { }

    protected virtual void OnTurnEnd() { }

    protected abstract JobHandle OnTurnUpdate(JobHandle inputDeps);

    
    protected override sealed JobHandle OnUpdate(JobHandle inputDeps)
    {
        return OnTurnUpdate(inputDeps);
    }
}
