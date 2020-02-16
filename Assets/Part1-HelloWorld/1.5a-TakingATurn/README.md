
#### [« Previous: 1.5 - Monsters and Refactoring](../1.5-Monsters/README.md) —  [Next: In Progress... »]

--------

# 1.5A - TakingTurns

In this chapter we'll go over the implementation of a turn-based system in ECS. This will allow us to define "phases" for the game to run through. In a "phase" system we hand out "action" components to our entities. In our behaviour systems we can query for these components and remove them when our "turn" is complete to give us turn-based behaviour.

## Turn Based System

First we create the system to iterate over our "phases".





###### [Player/PlayerInputSystem.cs](Player/PlayerInputSystem.cs)
```
```
Movement` component.

#### Field of View Changes



----------------------

These tutorials will always be free and the code will always be open source. With that being said I put quite a lot of work into them. If you find them useful, please consider donating. Any amount you can spare would really help me out a great deal - thank you!

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=Y54CX7AXFKQXG)


--------

#### [« Previous: 1.5 - Monsters and Refactoring](../1.5-Monsters/README.md) —  [Next: In Progress... »]