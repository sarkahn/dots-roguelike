**([Back to Index](../../README.md)) - ([Previous: Part 1 - "Hello World"](../Part1/Part1-helloworld.md))- (Next: In Progress...))**

# Part 2 - ECS

In part two we're going to use Unity's ECS system to track some entities and 
render them in the console using their positions.

As of this writing Unity's ECS framework is **very much** in development. The API
seems to be stabilizing but there's a good chance that some aspects could
change in the future and things in this tutorial might not be up to date with
the latest changes. This tutorial was written using the Entities package v[0.4.0].

## Why Use ECS

I don't think I could come up with a better explanation of why one would want
to write a game using ECS than the one from [TheBracket's tutorial](https://bfnightly.bracketproductions.com/rustbook/chapter_2.html#about-entities-and-components),
which this tutorial is based off of. I want to highlight one particular paragraph:
```
For small games, an ECS often feels like it's adding a bit of extra typing 
to your code. It is. You take the additional work up front, to make life 
easier later.
```

This, in my experience, has been the biggest benefit of ECS. It scales very well, it
reduces coupling inherently by *forcing* you to separate your behaviours and it forces you
to think about and format your data in a way that your computer can process very efficiently.

I am not saying one can't accomplish this in OOP. But the bottom line is ECS makes it easier for *me*
to write cleaner and faster code. So I use ECS.

## Learning the "Basics"

Now with all that being said - this is *not* a tutorial about learning ECS. It's a tutorial about
writing a Roguelike. As I mentioned in the introduction I will try give a brief explanation of
what's happening as we write the code, but I couldn't possibly hope to exhaustively explain every
aspect of Unity's ECS and Job system while also trying to write the game.

If you're new to ECS, I would **strongly** recommend these resources to learn the basics:
 * [The Unity ECS Samples](https://github.com/Unity-Technologies/EntityComponentSystemSamples/tree/master/ECSSamples/Assets/HelloCube) are a fantastic starting point. The "HelloCube" examples are extremely small and simple and the readme for each example gives a very clear explanation of what the code is doing. Don't even bother with anything else until you *thoroughly* understand what is happening in these samples. Download them, tinker around with them, and get familiar with them. As I said they are all very basic so it shouldn't take too long. 
 * [Getting Started with DOTS: Scripting Pong](https://www.youtube.com/watch?v=a9AUXNFBWt4) A fantastic video, as of this writing it's the only up-to-date tutorial from Unity that demonstrates how to write a simple game using ECS.
 * [The ECS Manual](https://docs.unity3d.com/Packages/com.unity.entities@latest) is essential while getting comfortable with the API. The manual itself is still being developed but it's come a long way. I highly recommend you keep the following pages open in another tab while you're learning ECS:
    * [General Purpose Components](https://docs.unity3d.com/Packages/com.unity.entities@0.4/manual/component_data.html)
    * [JobComponentSystem](https://docs.unity3d.com/Packages/com.unity.entities@0.4/manual/job_component_system.html)
    * [Using JobComponentSystem and ForEach](https://docs.unity3d.com/Packages/com.unity.entities@0.4/manual/entities_job_foreach.html)

The last page in particular is a big one, but it's essential once you're comfortable
writing systems and components. It thoroughly demonstrates all the proper ways to access
and modify your entities while leveraging the job system and burst compiler to keep 
everything running as fast as possible.

If you're completely new to ECS, don't expect it to come to you easily. It requires you to 
structure your code in a very specific way. You will need to constantly refer back to the manual
and examples to be able to solve problems. Googling will help, but be careful about finding
out-of-date information, the most up to date information will always be in the links I provided above.

If you're *really* struggling and can't figure something out on your own with the resources above,
feel free to [ask in the discord](https://discord.gg/unity) (make sure you go to the "#Dots" 
channel for ECS related questions) or ask [on the forums](https://forum.unity.com/forums/data-oriented-technology-stack.147/). There's almost always people around on the discord willing to
help, as long as you don't expect them to do all the work for you!

So now that you're equipped with the knowledge needed to solve any problems you might encounter when you want write ECS code, let's get started!

## Creating our RenderSystem

In order to get entities drawing to our console, we'll need to create a JobComponentSystem
that will iterate over our entities, read their position, and then write to the console using
that data. This is the basis of all Systems - read whatever data it's concerned with and 
act on it in some way, whether that's writing to other data, printing something to the log, or
in our case, drawing some smileys on screen.

The first step to creating any system is to first create the data that it operates on. In
this example we'll need an int2 representing the entity's position in the console. Create 
a new script called "Position" and put it in the Part2 folder. We define it like so:

```
public struct Position : IComponentData
{
    public int2 Value;
    public static implicit operator int2(Position c) => c.Value;
    public static implicit operator Position(int2 v) => new Position { Value = v };
}
```

"Regular" components should inherit from IComponentData. The implicit operators are just
there to make it more convenient to work with the data while it's being processed, they 
are not required by Unity.

Now we have our Position, we need to create our system. This is what a basic system looks like:
```
public class TileRenderSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return inputDeps;
    }
}
```

Of course it's not going to do much like that. It needs a console to render to:

TODO : Need to set up RLTK.SimpleConsole to be able to render without being attached
to a monobehaviour

**([Back to Index](../../README.md)) - ([Previous: Part 1 - "Hello World"](../Part1/Part1-helloworld.md))- (Next: In Progress...))**
