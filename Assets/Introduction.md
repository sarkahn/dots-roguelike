**([Back to Index](../Readme.md)) - ([Next: Part 1 - Hello World](Part1/Part1-HelloWorld.md#part-1---hello-world))**

# Introduction
The goal of this tutorial is pretty much to create a Unity version of TheBracket's 
[excellent roguelike tutorial in Rust](https://bfnightly.bracketproductions.com/rustbook/chapter_1.html).
For the most part I will be trying to re-create each part of the original tutorial - except in Unity. 

## Who It's For
This tutorial assumes a basic level of knowledge on how to work with Unity - you
should know how to create a project and understand the workflow of creating 
scripts and adding them to GameObjects. You should have some experience with C# 
or programming in general. If you've never coded before then this is probably not 
a good place to start. I will generally not be explaining the basic concepts of Unity
or C#.

With that being said we will also be using Unity's ECS and Jobs systems. As of this writing these 
are relatively new so I will try to give an overview of what's happening as we hit
those and provide resources where you can learn more from sources that can explain
things far better than I ever could.

## What You Need
This project relies on my renderer [RLTK for Unity](https://github.com/sarkahn/rltk_unity), 
which is itself a port of TheBracket's fantastic ASCII rendering engine for Rust. If you're using
the repository then RLTK should be automatically included via the package manager when you open the
project in Unity. You should see a folder named "RLTK" under "Packages" in your hierarchy. 

As of right now creating custom user packages is a very arcane process so it's possible I could screw
something up along the way. If RLTK isn't downloading properly or is throwing errors you can remove it via the 
package manager, download/clone it directly from the [github repo](https://github.com/sarkahn/rltk_unity) 
and install it directly in your assets folder.

If you're not using the repository and are starting from a fresh project you'll need to ensure you have RLTK, 
whether installed in your Assets folder or via the Unity Package Manager. You can see 
details on how to do that [in the RLTK repository](https://github.com/sarkahn/rltk_unity#how-to-get-it).


## Project Structure
This project is structured so that each example will sit in it's own folder in 
the Assets directory. A single Unity project can be quite large on disk so it's not 
recommended to try and create a separate project for each tutorial. Instead we'll 
divide each tutorial up by scene. In the folder for each tutorial you'll find the 
readme for that particular part and a subfolder with a completed example. 

All documentation will be hosted in this repository and included in the project when you 
download it via ".md" files. You can read it locally or on github. If you want to read the 
tutorials locally you'll probably want some kind of MD reader, otherwise I would recommend 
just viewing them [on github](https://github.com/sarkahn/rltk_unity_roguelike/blob/master/Assets/Introduction.md#project-structure).

**([Back to Index](../Readme.md)) - ([Next: "Part 1 - Hello World"](Part1/Part1-HelloWorld.md#part-1---hello-world))**







