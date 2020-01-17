**([Back to Index](../../README.md)) - ([Previous: Part 2 - "ECS"](../Part2/Part2-ECS.md))- (Next: In Progress...))**

# Part 3 - Walking a Map

In this chapter we're going to learn how to isolate our systems to certain scenes and how to create a map that our character can walk around in.

## Isolating the Previous Example

In the previous chapter we created a few different components and systems to draw some characters to the console and have them move around. But now that we're on to the next chapter we no longer want many of those components and systems to affect our new scene. Rather than just deleting all the work we've done, we can isolate all the systems and components and prevent Unity from loading them automatically.

First we should use namespaces to isolate the code from the first example so that we won't have to worry about any possible naming collisions or having our public namespace polluted with irrelevant types. This is good practice in general - any code that moves beyond the initial stages should be encapsulted into a descriptive namespace. In my case I wrapped all my Part2-related code in `namespace RLTKTutorial.Part2`. You can choose whatever namespace you like.

We also need to prevent Unity from automatically loading our systems and instead allow the systems to run only when the relevant Scene is loaded. To prevent Unity from automatically loading and running the systems we should add a [DisableAutoCreation] attribute to them. So  the class declarations should look like this:

```
namespace RLTKTutorial.Part2
{
    [DisableAutoCreation]
    [AlwaysSynchronizeSystem]
    public class ReadInputSystem : JobComponentSystem
    {
    ...
```
```
namespace RLTKTutorial.Part2
{
    [DisableAutoCreation]
    [AlwaysSynchronizeSystem]
    public class MoveLeftSystem : JobComponentSystem
    {
    ...
```

And so on. The final step is to tie them to the scene. To do this we'll use a [ComponentSystemGroup](https://docs.unity3d.com/Packages/com.unity.entities@0.4/manual/system_update_order.html?#component-system-groups) that will hold all the systems for a given scene. It's very simple to set up. We'll also need a MonoBehaviour to manually handle the creation, update, and destruction of systems in the same way that Unity was doing automatically before. The `ComponentSystemGroup` and bootstrap MonoBehaviour can be grouped together in a single file, we'll call it "Part2Bootstrap":

```
namespace RLTKTutorial.Part2
{
    public class Part2Bootstrap : MonoBehaviour
    {
    }

    [DisableAutoCreation]
    public class Part1SystemGroup : ComponentSystemGroup
    {
    }
}
```

We need the `[DisableAutoCreation]` attribute on our ComponentSystemGroup too, otherwise Unity will automatically initialize it just like the normal ComponentSystems - it is in fact derived from the same base class as a JobComponentSystem. We can add the relevant systems to our `ComponentSystemGroup` inside it's `OnCreate` function:

```
    [DisableAutoCreation]
    public class Part1SystemGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            AddSystemToUpdateList(World.GetOrCreateSystem<TileRenderSystem>());
            AddSystemToUpdateList(World.GetOrCreateSystem<MoveLeftSystem>());
            AddSystemToUpdateList(World.GetOrCreateSystem<MovePlayerSystem>());
            AddSystemToUpdateList(World.GetOrCreateSystem<ReadInputSystem>());
        }
    }
```

Notice that if we ever need to manually create systems it must be done through a [World](https://docs.unity3d.com/Packages/com.unity.entities@0.4/manual/world.html?q=World). Now that our systems are attached to the `ComponentSystemGroup`, we need to attach the system group to our bootstrap MonoBehaviour:

```
    public class Part2Bootstrap : MonoBehaviour
    {
        Part1SystemGroup _systems;

        private void OnEnable()
        {
            systems = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<Part1SystemGroup>();
        }

        private void OnDisable()
        {
            if(systems != null && World.DefaultGameObjectInjectionWorld != null)
                World.DefaultGameObjectInjectionWorld.DestroySystem(systems);
        }

        private void Update()
        {
            systems.Update();
        }
    }
```

This code should be pretty self-explanatory - we create our systems in `OnEnable`, destroy them in `OnDisable`, and update them inside `Update`. You might notice the verbose `DefaultGameObjectInjectionWorld` - this is a static `World` that exists as the intended way for GameObjects to interact with the default ECS `World` at runtime.

With that done you should now be able to manually control when your systems are created. You can add the `Part2Bootstrap` component to a MonoBehaviour in your scene and when you press play it should load up your systems and you should see the same scene as before. You can enabled or disable the MonoBehaviour manually at runtime, and it should destroy and recreate your systems as expected:




**([Back to Index](../../README.md)) - ([Previous: Part 2 - "ECS"](../Part2/Part2-ECS.md))- (Next: In Progress...))**
