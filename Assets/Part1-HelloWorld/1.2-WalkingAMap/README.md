
#### [« Previous: 1.1 - ECS](../1.1-ECS/README.md#11---ecs) —  [Next: 1.3 - A More Interesting Map »](../1.3-AMoreInterestingMap/README.md)

--------

# 1.2 - Walking a Map

This chapter is focused on generating a map in ECS for the player to move around in. It also displays controls at runtime to control the map size.

## Representing the Map - aka Shared Data

For this example the map data is defined simply as a [DynamicBuffer](https://docs.unity3d.com/Packages/com.unity.entities@0.5/manual/dynamic_buffers.html) of `TileType`:


###### [TileType.cs](Map/TileType.cs)
```
public enum TileType : int
{
    Floor = 0,
    Wall = 1,
};
```

###### [MapTiles.cs](Map/MapTiles.cs)
```
public struct MapTiles : IBufferElementData
{
    public TileType value;
    public static implicit operator TileType(MapTiles b) => b.value;
    public static implicit operator MapTiles(TileType v) => new MapTiles{ value = v };
}
```

Defining it this way lets us easily query and read from/write to the map inside different systems without having to manually manage job dependencies. 

When creating some common shared data like a world map, you might be tempted to create a custom data structure or self-contained class to represent your data. It's very important to consider how you intend to use your data, and how it's going to be accessed. If you intend to use that data inside a job and between systems, you would then have to [manually manage your job dependencies](https://docs.unity3d.com/2019.3/Documentation/Manual/JobSystemJobDependencies.html). 

In some cases doing that might make sense, but I've found it's a *huge* pain and not even remotely worth the trouble in most cases. It tends to make your system code much harder to parse and is horribly annoying to maintain when you need to make changes. 

Basically if you have some data **that will be shared between systems** and you can get away with representing your data as an `IComponentData` or a `DynamicBuffer`, you should do so. This will let you benefit from the inherent speed and data-separation that comes from ECS while not having to worry about the dependency-management headache. By defining your data inside components you can let Unity handle all dependency management for you and only focus on your logic.

Moving on - the MapTiles alone can't entirely represent our map. We also use a `MapData` component to hold the map dimensions:

###### [MapProxy.cs](Map/MapProxy.cs)
```
public struct MapData : IComponentData
{
    public int width;
    public int height;
}
```

So we can then represent our map as an Entity with a `MapData` component and a `MapTiles`, to be queried in other systems. We create the initial empty map via a simple conversion script:

###### [MapProxy.cs](Map/MapProxy.cs)
```
    public class MapProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        int _width = 80;
        [SerializeField]
        int _height = 50;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new MapData
            {
                width = _width,
                height = _height,
            });

            var buffer = dstManager.AddBuffer<MapTiles>(entity);

        }
    }
```

This gives us a simple component we can add to a gameobject. We can then tweak the size of the map through the inspector and it will be generated at runtime via the [Conversion System](https://docs.unity3d.com/Packages/com.unity.entities@0.5/manual/gp_overview.html#authoring-overview).

## Map Generation

That gives us a map entity, but right now the map is empty. To populate it we use a map generatino system, but for that to work it needs the map generation parameters. We provide these via the `GenerateMap` component, which is added to map entity during conversion with the `GenerateMapProxy` MonoBehaviour:

###### GenerateMapProxy.cs
```
[GenerateAuthoringComponent]
public struct GenerateMap : IComponentData
{
    public int iterationCount;
    public int2 playerPos;
    public int seed;
}
```

This uses the useful `[GenerateAuthoringComponent]` attribute. This will automatically generate an `IConvertGameObjectToEntity` MonoBehaviour from our `IComponentData`. Since we don't require any special logic for this particular component, we can just let Unity generate the Converter for us so we can easily tweak the values in the inspector:

![](images~/mapconversion.png)

Above you see the gameobject representation of our map. When we hit play, the conversion system will run and convert it into a pure entity for our systems to work with.

Now, the actual map generation happens in `GenerateMapSystem`. The process is fairly straightforward - first we gather the relevant map and generation data from our map entity:

###### [GenerateMapSystem.cs](Map/GenerateMapSystem.cs)
```
protected override void OnUpdate()
{
    ...

    var mapEntity = _generateMapQuery.GetSingletonEntity();
    var genData = EntityManager.GetComponentData<GenerateMap>(mapEntity);
    var mapData = EntityManager.GetComponentData<MapData>(mapEntity);
    var map = EntityManager.GetBuffer<MapTiles>(mapEntity);
    ...
}
```

We then pass it into our job for the actual generation:

###### [GenerateMapSystem.cs](Map/GenerateMapSystem.cs)
```
inputDeps = Job.WithCode(() =>
{
    int w = mapData.width;
    int h = mapData.height;

    InitializeMap(map, w, h);

    BuildBorder(map, w, h);

    genData.seed = genData.seed == 0 ? randomSeed : genData.seed;
    GenerateWalls(map, w, h, genData);

}).Schedule(inputDeps);
```

We use static methods inside the `GenerateMapSystem` class to make things a bit more readable. It's okay to use static methods in jobs as long as they aren't touching any outside mutable or managed data. The first step, `InitializeMap` just sets all tiles to floors.

#### Placing the Borders

Inside `BuildBorder` we set all the border tiles to walls:

###### [GenerateMapSystem.cs](Map/GenerateMapSystem.cs)
```
[MethodImpl(MethodImplOptions.AggressiveInlining)]
static int At(int x, int y, int width) => y * width + x;

static void BuildBorder(DynamicBuffer<MapTiles> map, int w, int h)
{
    for (int x = 0; x < w; ++x)
    {
        map[At(x, 0, w)] = TileType.Wall;
        map[At(x, h - 1, w)] = TileType.Wall;
    }

    for (int y = 0; y < h; ++y)
    {
        map[At(0, y, w)] = TileType.Wall;
        map[At(w - 1, y, w)] = TileType.Wall;
    }
}
```


#### Placing the Random Walls

In `GenerateWalls` we just do a big loop using our `GenerateMap` parameters and randomly set tiles to walls as long as they aren't at the player position:

###### [GenerateMapSystem.cs](Map/GenerateMapSystem.cs)
```
static void GenerateWalls(DynamicBuffer<MapTiles> map, int w, int h, GenerateMap genData )
{
    Random rand = new Random((uint)genData.seed);
    for (int i = 0; i < genData.iterationCount; ++i)
    {
        int x = rand.NextInt(1, w - 2);
        int y = rand.NextInt(1, h - 2);

        if (x == genData.playerPos.x && y == genData.playerPos.y)
            continue;
                
        map[At(x,y,w)] = TileType.Wall;
    }
}
```

`Random` in this case refers to Unity's job-safe [Random struct](https://docs.unity3d.com/Packages/com.unity.mathematics@1.1/api/Unity.Mathematics.Random.html?q=Random) from their DOTs Mathematics package, not the  static "UnityEngine.Random" class.

#### Placing the Player

The remainder of `OnUpdate` simply sets the player's position in a standard `ForEach` and queues the `GenerateMap` component for removal via an [EntityCommandBuffer](https://docs.unity3d.com/Packages/com.unity.entities@0.5/manual/entity_command_buffer.html):

```
inputDeps = Entities
    .WithAll<Player>()
    .ForEach((ref Position pos) =>
    {
        var p = genData.playerPos;
        pos = p;
    }).Schedule(inputDeps);
            
commandBuffer.RemoveComponent<GenerateMap>(_generateMapQuery);
_barrier.AddJobHandleForProducer(inputDeps);
```

#### Generating a new Map

When we want to generate a new map, we simply add a new `GenerateMap` component to the map entity. The `GenerateMapSystem` is querying for this component, so whenever one is added it will automatically run through this whole process again and give us a shiny new map.

You can see an example of us forcing a new map to generate inside the  `ResizeMapInputSystem`:

###### [ResizeMapInputSystem.cs](Player/ResizeMapInputSystem.cs)
```
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float2 resize = _resizeMapAction.triggered ?
            (float2)_resizeMapAction.ReadValue<Vector2>() : float2.zero;
            
        if (math.lengthsq(resize) == 0)
            return inputDeps;

        var commandBuffer = _barrierSystem.CreateCommandBuffer();

        var mapEntity = _mapQuery.GetSingletonEntity();
        var mapData = EntityManager.GetComponentData<MapData>(mapEntity);
            
        inputDeps = Job.WithCode(() =>
        {
            mapData.width += (int)resize.x;
            mapData.height += (int)resize.y;

            mapData.width = math.max(3, mapData.width);
            mapData.height = math.max(3, mapData.height);
            commandBuffer.SetComponent<MapData>(mapEntity, mapData);
                
            var genData = GenerateMap.Default;
            commandBuffer.AddComponent(mapEntity, genData);
        }).Schedule(inputDeps);

        _barrierSystem.AddJobHandleForProducer(inputDeps);
  
        return inputDeps;
    }
```

There's a lot of boilerplate here but what it accomplishes is very straightforward - if our "ResizeMap" controls are triggered then we execute a job. Inside the job we use an [EntityCommandBuffer](https://docs.unity3d.com/Packages/com.unity.entities@0.5/manual/entity_command_buffer.html) to resize our map and add a `GenerateMap` component to the map so the `GenerateMapSystem` will do it's thing.

And that's it for this chapter. If you're confused about anything ecs-related you've seen so far, I encourage you to please refer back to the learning materials provided in [chapter 1.1](../1.1-ECS/README.md) to learn the basics of ECS and get a better feel for the API before continuing.

----------------------

These tutorials will always be free and the code will always be open source. With that being said I put quite a lot of work into them. If you find them useful, please consider donating. Any amount you can spare would really help me out a great deal - thank you!

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=Y54CX7AXFKQXG)


--------

#### [« Previous: 1.1 - ECS](../1.1-ECS/README.md#11---ecs) —  [Next: 1.3 - A More Interesting Map »](../1.3-AMoreInterestingMap/README.md)