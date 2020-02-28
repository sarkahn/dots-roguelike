
#### [« Previous: 1.5 - Monsters and Refactoring](../1.5-Monsters/README.md) — [Next: In Progress... »]

--------

# 1.5A - TakingTurns


In this chapter we'll go over my implementation of a turn-based system in ECS. There are [many ways to implement turns in a Roguelike](http://www.roguebasin.com/index.php?title=Time_Systems), this is just one way. 

## Energy

For my game I wanted to implement an "energy" system like the one in Dwarf Fortress or ADOM. It's a simple and flexible system that allows entities to act in order based on their speed. It also handles state changes during processing very well. If you're not familiar, the basic flow goes like this:

1. Give energy to all actors in a loop based on their speed. If any have enough energy to act, add them to the turn buffer. Repeat while the turn buffer is empty.
2. Remove the fastest entity from the turn buffer and let them take their turn. Repeat until the turn buffer is empty.
3. Return to 1.

This loop only returns control to the game when an actor has a turn but hasn't yet taken an action. In most cases this only happens when waiting for the player's input, so almost all non-player actions happen in a single frame and appear "instant" from the perspective of the player. Technically it's turned based but if the user is constantly making inputs it should "feel" realtime.

Unfortunately the requirements of this system means it doesn't really fit nicely into the typical Unity ECS workflow. In Unity's ECS the api is built around the idea of doing `ForEach` jobs over massive amounts of entities in multi-threaded jobs across multiple chunks. Realistically the only part where we can benefit from a standard `ForEach` is distributing the energy and populating the turn buffer. And it's important to note we can't rely on threaded jobs at all - it's a fixed requirement that our turn system blocks the main thread until the player can input. This is important to keep input feeling responsive even if you have hundreds or thousands of entities all taking turns between player inputs. 

## The Turn System

The turn system is designed to run the main loop we described above but in context of ECS. It's fairly straightforward until we get to the part where we process the entity's turn. The top level loop looks like this:

###### [TurnSystem/GameTurnSystem.cs](TurnSystem/GameTurnSystem.cs)
```csharp
protected override void OnUpdate()
{
    while (!_actors.IsEmptyIgnoreFilter)
    {
        while( _actingEntity == Entity.Null )
        {
            if (_turnBuffer.Length == 0)
                PopulateTurnBuffer();

            _actingEntity = GetNextActor();
        }

        if (!PerformTurnActions(_actingEntity))
            break;
        else
            _actingEntity = Entity.Null;
    }
}
```

The logic is fairly straightforward - distribute energy, get the next actor, take a turn. As mentioned earlier, we only exit the loop when an actor is taking a turn but doesn't perform any actions. It's worth noting this works for any actor, not just the player. So if for example anything happens during a non-player turn that needs to be seen by the player, we can just delay that turn from completing for a few seconds, then finish the turn and let the loop continue.

We track the currently acting `Entity` and the state of the turn buffer to ensure we're doing things in the proper order. Energy is only distributed until we have actors in the turn buffer, and we skip energy distribution until all actors who are able to act have taken their turn.

## Populating the Turn Buffer

We'll go over each function to get a better idea of what's happening in the loop. First, `PopulateTurnbuffer`:

###### [TurnSystem/GameTurnSystem.cs](TurnSystem/GameTurnSystem.cs)
```csharp
void PopulateTurnBuffer()
{
    var turnBuffer = _turnBuffer;
    while (turnBuffer.Length == 0)
    {
        Entities
            .WithAll<Actor>()
            .ForEach((Entity e, ref Energy energy, in Speed speed) =>
            {
                int value = speed.value == 0 ? Speed.Default.value : speed.value;

                energy += value;

                if (energy >= Energy.ActionThreshold)
                    turnBuffer.Add(e);
            }).Run();
    }
}
```

We continually distribute energy until someone has enough to act, then we add them to the turn buffer. Nothing surprising there. 

## Getting the next Actor

Next is `GetNextActor`:

###### [TurnSystem/GameTurnSystem.cs](TurnSystem/GameTurnSystem.cs)
```csharp
Entity GetNextActor()
{
    var energyFromEntity = GetComponentDataFromEntity<Energy>(true);
    var speedFromEntity = GetComponentDataFromEntity<Speed>(true);
    
    var turnBuffer = _turnBuffer;

    int index = -1;
    Entity e = Entity.Null;

    Job
        .WithCode(() =>
    {
        // Remove anyone that's lost the ability to act
        for (int i = 0; i < turnBuffer.Length; ++i)
            if (!ActorCanAct(turnBuffer[i], energyFromEntity))
            {
                turnBuffer.RemoveAtSwapBack(i);
                i--;
            }

        if (turnBuffer.Length == 0)
            return;

        int highestSpeed = int.MinValue;
        for( int i = 0; i < turnBuffer.Length; ++i )
        {
            int speed = speedFromEntity[turnBuffer[i]];
            if (speed > highestSpeed)
            {
                highestSpeed = speed;
                index = i;
            }
        }
    }).Run();

    if (index != -1)
    {
        e = _turnBuffer[index];
        _turnBuffer.RemoveAtSwapBack(index);
    }

    return e;
}
```

It's a bit long but the logic is extremely straightforward. First we clean the turn buffer in case any relevant state has changed since the previous turn (IE: Dead actors, energy changes, etc). If the turn buffer isn't empty after that we do a single iteration over the list to find the fastest entity, and return that entity for processing.

## Perform Turn Actions

Once we have the actor whose turn is next we call `PerformTurnActions` back in `OnUpdate`:


###### [TurnSystem/GameTurnSystem.cs](TurnSystem/GameTurnSystem.cs)
```csharp
bool PerformTurnActions(Entity e)
{
    var actorType = EntityManager.GetComponentData<Actor>(e).actorType;
    var actionSystem = _dispatchMap[(int)actorType];

    return actionSystem.ProcessEntityTurn(e);
}
```

Here we pass the actor to the appropriate system given it's actor type, allowing it to take it's turn. The "dispatchMap" is just a dictionary mapping actor types to the action system that processes them. These are added explicitly to the `GameTurnSystem` in the bootstrap class for this scene. 

## Action Systems

Any system that processes an actor's turn will derive from `TurnActionSystem`, which allows derived classes to define behaviour in the `OnTakeTurn` function:

```csharp
public abstract class TurnActionSystem : SystemBase
{
    public abstract ActorType ActorType { get; }

    public bool ProcessEntityTurn(Entity e)
    {
        int cost = OnTakeTurn(e);

        if (cost <= 0)
            return false;

        var energy = EntityManager.GetComponentData<Energy>(e);
        energy -= cost;
        EntityManager.SetComponentData(e, energy);

        return energy < Energy.ActionThreshold;
    }

    protected abstract int OnTakeTurn(Entity actor);

    public virtual void OnFrameBegin() {}

    protected override void OnUpdate() {}
}
```

`ProcessEntityTurn` is what gets called by the Turn System. It returns whether or not the current turn is complete based on the cost of any actions performed in the `OnTakeTurn` function and the current energy level of the actor.

Inside `OnTakeTurn` derived classes can fully define what an actor is going to do during their turn. In this example there's two `TurnActionSystem`s - one for the player and one for the monsters.

## Monster Turns

In the monster system we simply have them wander in a random direction whenever their turn comes around.

###### [TurnSystem/MonsterTurnSystem.cs](TurnSystem/MonsterTurnSystem.cs)
```csharp
public class MonsterTurnSystem : TurnActionSystem
{
    Random _rand;
    MoveSystem _moveSystem;

    protected override void OnCreate()
    {
        _moveSystem = World.GetOrCreateSystem<MoveSystem>();
        _rand = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
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
```

We use the `MoveSystem` to handle moving entities on the map. The return value of the `OnTakeTurn` function is the cost of our action - in this case it's the total amount of energy required for an entity to act. 

The base class will subtract that cost from the entity's energy in `OnProcessEntityTurn` and report to the Turn System that the monster's turn is complete, allowing the turn system to immediately move to the next monster without returning control to the game.

## Player Turns

For the player though we want to ensure that Turn processing stops between inputs:

###### [TurnSystem/PlayerTurnSystem.cs](TurnSystem/PlayerTurnSystem.cs)
```csharp
protected override int OnTakeTurn(Entity actor)
{
    int2 move = (int2)(_moveAction.triggered ? (float2)_moveAction.ReadValue<Vector2>() : float2.zero);

    int cost = Energy.ActionThreshold;

    if ((_previousMove.x == move.x && _previousMove.y == move.y)
        || (move.x == 0 && move.y == 0) )
    {
        cost = 0;
    }
    else
        _moveSystem.TryMove(actor, move);

    if (_quitAction.triggered)
        Application.Quit();

    _previousMove = move;


    return cost;
}
```

In this case we return 0 any time there's no input or a repeated input. In effect this returns control to the game for a single frame between inputs if, for example, the player is holding down a move direction. Back in `ProcessEntityTurn` we can see the effect of returning 0 cost from `OnTakeTurn`:

###### [TurnSystem/TurnActionSystem.cs](TurnSystem/TurnActionSystem.cs)
```csharp
    public bool ProcessEntityTurn(Entity e)
    {
        int cost = OnTakeTurn(e);

        if (cost <= 0)
            return false;

        var energy = EntityManager.GetComponentData<Energy>(e);
        energy -= cost;
        EntityManager.SetComponentData(e, energy);

        return energy < Energy.ActionThreshold;
    }
```

If it has no cost to subtract from the entity's energy it immediately returns false. As mentioned earlier, back in `GameTurnSystem` when an entity taking their turn returns false it's a signal to break from the turn loop:

###### [TurnSystem/GameTurnSystem.cs](TurnSystem/GameTurnSystem.cs)
```csharp
protected override void OnUpdate()
{
    while (!_actors.IsEmptyIgnoreFilter)
    {
        ...

        if (!PerformTurnActions(_actingEntity))
            break;
        else
            _actingEntity = Entity.Null;
    }
}
```

----------------------

These tutorials will always be free and the code will always be open source. With that being said I put quite a lot of work into them. If you find them useful, please consider donating. Any amount you can spare would really help me out a great deal - thank you!

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=Y54CX7AXFKQXG)


--------

#### [« Previous: 1.5 - Monsters and Refactoring](../1.5-Monsters/README.md) — [Next: In Progress... »]