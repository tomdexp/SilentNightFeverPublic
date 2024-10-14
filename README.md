# Disco-Gecko Source
## What is this repository ?
This repository is the public version of the source code of my Master 1 Game : Disco-Gecko (was SilentNightFever), it's the public version because I have trimmed all paid Plugins that should not be in the public repository.

IT WON'T COMPILE (Because there is a lot of missings plugins)

## What did I do on this project ?
I was the Online and Gameplay Programmer of this project, but I also did the UI. I did not code the Procedural Generations, but I contributed to networking it.

You can find all of my scripts at the path `Assets/_Project/Scripts`

## Features
- Multiplayer
  - Networking with Fishnet
  - Local, Online and both at the same time gameplay
  - Tools for testing multiplayer features locally
    -  Inputs are provided via an interface IInputProvider
    -  In editor, the developer can possess other characters by switching the character that is provided by the inputs
    -  There is a class that can record inputs and replay them as if it was a real player for multiplayer interactions testing on the same computer
  - Unity Gaming Services integration (with Unity Relay and Player Authentication)
- Procedural Generation
- Character's tongue are physics-based