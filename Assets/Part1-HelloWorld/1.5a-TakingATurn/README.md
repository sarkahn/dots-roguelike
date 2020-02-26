
#### [« Previous: 1.5 - Monsters and Refactoring](../1.5-Monsters/README.md) — [Next: In Progress... »]

--------

# 1.5A - TakingTurns


In this chapter we'll go over my implementation of a turn-based system in ECS. There are [many ways to implement turns in a Roguelike](http://www.roguebasin.com/index.php?title=Time_Systems), this is just one way. 


## Energy

I wanted to implement an "energy" system like the one in Dwarf Fortress or ADOM. If you're not familiar, the basic flow goes like this:

1. Give energy to all actors in a loop based on their speed. If any have enough energy to act, add them to the turn buffer. Repeat while the turn buffer is empty.
2. Remove the fastest entity from the turn buffer and let them take their turn. Repeat until the turn buffer is empty.
3. Return to 1.

This loop only returns control to the game when an actor has a turn but hasn't yet taken an action. In most cases this only happens when waiting for the player's input, so almost all non-player actions happen in a single frame and appear "instant" from the perspective of the player. Technically it's turned based but if the user is constantly making inputs it should "feel" realtime.

In OOP, the code for this is actually fairly straightforward. Unfortunately that's not really the case in Unity's ECS. Unity ECS favors systems running a job off the main thread on many many entities across multiple chunks without much regard for the order they're in inside their chunks. That is at the core of the api and it's when you can expect to get the most readable, straightforward and incredibly fast results.

What we want to do is much different. The only part where we can reasonably benefit from a `ForEach` is distributing the energy. And we can't rely on threaded jobs at all - it's a hard requirement that our turn system blocks the main thread until the player can input, otherwise player input would be laggy and delayed as other actors took their turns inside jobs. 

So we know our first step - distribute energy in a `ForEach` on the main thread. After that we choose the correct entity based on speed which is fairly straightforward. The final step turns out to be the really tricky part - allow the currently acting entity to take their turn based on the type of actor it is.

We have no inheritance or polymorphism to rely on, and our turn system blocks the main thread (by design) until the player can act, leaving us well outside the typical ECS workflow of having every system update once per frame. 

Since we're not using OOP we can't rely on inheritance and call something like actor.TakeTurn. We need some mechanism by which to allow an entity to a


###### [TEST.cs](TEST.cs)
```
```

#### TEST



----------------------

These tutorials will always be free and the code will always be open source. With that being said I put quite a lot of work into them. If you find them useful, please consider donating. Any amount you can spare would really help me out a great deal - thank you!

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=Y54CX7AXFKQXG)


--------

#### [« Previous: 1.5 - Monsters and Refactoring](../1.5-Monsters/README.md) — [Next: In Progress... »]