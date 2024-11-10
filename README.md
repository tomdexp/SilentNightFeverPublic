![Unity](https://img.shields.io/badge/unity-%23000000.svg?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)
![Notion](https://img.shields.io/badge/Notion-%23000000.svg?style=for-the-badge&logo=notion&logoColor=white)

![GitHub commit activity](https://img.shields.io/github/commit-activity/t/tomdexp/SilentNightFeverPublic?authorFilter=tomdexp&style=for-the-badge)

## Disco-Gecko
## Table of contents
- [What is this repository ?](#what-is-this-repository-)
- [General](#general)
  - [The pitch](#the-pitch)
  - [Context](#context)
  - [Credits](#credits)
- [My work on this project](#my-work-on-this-project)
  - [Features](#features)
- [Challenges](#challenges)
  - [Challenge 1 : Local, Online and both at the same time gameplay](#challenge-1--local-online-and-both-at-the-same-time-gameplay)
  - [Challenge 2 : A large crowd that the player has to navigate](#challenge-2--a-large-crowd-that-the-player-has-to-navigate)
  - [Challenge 3 : A physics-based tongue for interactions](#challenge-3--a-physics-based-tongue-for-interactions)
    - [The sensors of the tongue system](#the-sensors-of-the-tongue-system)
    - [Showcase of the tongue's features](#showcase-of-the-tongues-features)
    - [The replication of the tongue system](#the-replication-of-the-tongue-system)
- [What could be improved](#what-could-be-improved)
- [Gallery](#gallery)
  - [UI Video Showcase](#ui-video-showcase)
  - [Fake Players and Possession Video Showcase](#fake-players-and-possession-video-showcase)
  - [Input Record and Replay for testing Video Showcase](#input-record-and-replay-for-testing-video-showcase)
  - [Custom Logger Video Showcase](#custom-logger-video-showcase)

## What is this repository ?
This repository is the public version of the source code of my Master 1 Game : Disco-Gecko (was SilentNightFever), it's the public version because I have trimmed all paid Plugins that should not be in the public repository.

IT WON'T COMPILE (Because there is a lot of missing plugins)

## General
### The pitch
Reptiles, lights and an 80’s glitter !

DISCO GECKO is a 2v2 party game where you’re lost in a huge jungle night club and have to find your friend ! Use your tongue and the environment to get in the way of the opposite team… and be the first two geckos to kiss !!

DISCO GECKO is playable only with 4 players, ON SITE or ONLINE !

After a short onboarding phase, the game will start. In order to win, a team has to succeed 3 rounds (you can change the number of rounds in the settings menu!). The first duo to find each other and kiss win the round !

But be careful ! The north can't help you : all cameras angles are randomized every round. Some usable landmarks will help you, while others will allow you to sabotage the other team...

To find yourself, you will have to memorize the environment around you and communicate with your friend !

### Context
This game was made in 4 month between March 2024 and July 2024 by a Team of 8 people and 2 freelance composers during my Master's degree 1st year project.
Some members of the team were part-time on this project. I was full-time on it.

### Credits
Aurore BERTHET, Producer

Margot THETIOT, UX Designer & User Researcher

Tom D'EXPORT, Online & Gameplay Programmer _(that's me)_

Tristan BADANA, Generalist Programmer

Patxi M. GONZALEZ, Game Designer & 2D Artist

Hugo SANNIER-DURAND, Sound Designer

Jules GIBERT, Game Art

Mahé LETTELIER, Concept Artist & Animator

and

Foucauld DESROUSSEAUX & Clara ROTTI, composers

## My work on this project
I was the Online and Gameplay Programmer of this project, I also did the UI. I did not code the Procedural Generations, but I contributed to networking it.

You can find all of my scripts at the path `Assets/_Project/Scripts`

### Features
Here is a non-exhaustive list of features I made :
- Multiplayer
  - Networking with Fishnet
  - Local, Online and both at the same time gameplay
  - Tools for testing multiplayer features locally
    -  Inputs are provided via an interface IInputProvider
    -  In editor, the developer can possess other characters by switching the character that is provided by the inputs
    -  There is a class that can record inputs and replay them as if it was a real player for multiplayer interactions testing on the same computer
  - Unity Gaming Services integration
    - Players can create and join lobbies with a join code
    - We use the Unity Transport Integration for Fishnet : https://github.com/ooonush/FishyUnityTransport
- Procedural Generation
- Character's tongue are physics-based
  - Players can pull each other
  - Players can stick their tongue at objects to interact with them (pull, push, press)
- World-Space UI with replicated menus for the lobby
- An ApplicationSettings system that uses Unity's Player Pref but enable strongly typed variables that auto-load on start and auto-save when changed

## Challenges
Disco Gecko was a technical challenges on multiple features 
1. Local, Online and both at the same time gameplay
2. A large crowd that the player has to navigate
3. A physics-based tongue for interactions

### Challenge 1 : Local, Online and both at the same time gameplay
This game is split-screen, it's meant to be a 4-player couch game, but also an online game...or both at the same time.
It means that you can have any of these configuration : 4 player on the same device, 4 player on 4 device, 4 players on 2 device, etc...

My goal was to code the game so that the code was not different for real online gameplay and local gameplay, 
it meant being online by default and carefully architecture players inputs so that at the end the same code was used. 

The first thing I did to solve this technical challenge was to represent each player by two things :
- Their client ID
- Their device they use to send input to Unity
- Their location on the screen (since the screen is split in 4 square, I identity them by A,B,C,D from the upper left corner to the lower right corner)

The code of the script that represent a player ([Assets/_Project/Scripts/Runtime/Player/RealPlayerInfo.cs](Assets/_Project/Scripts/Runtime/Player/RealPlayerInfo.cs))
```cs
/// <summary>
/// This struct represent a Real player since our game can have multiple players on the same client
/// We should have four of these, one for each player
/// The PlayerManager should create all four players from these infos
/// </summary>
[Serializable]
public struct RealPlayerInfo
{
    public byte ClientId;
    public PlayerIndexType PlayerIndexType;
    public string DevicePath;
}

[Serializable]
public enum PlayerIndexType
{
    A, // upper left player
    B, // upper right player
    C, // lower left player
    D, // lower right player
    Z // Z == not set yet
}
```

The player index type is used extensively in the project, for example, prefab that was meant to exist for each player were called "ExamplePrefab_Z".
Inside the RealPlayerInfo we also replicate a string for the DevicePath, string can consumes a lot of bandwidth but since it's a list of four RealPlayerInfo 
replicated during the joining phase but doesn't change during the rest of the game we assume it's ok.

This code is the base architecture of the whole project and has worked well for our needs. 
Each time there is an input, we get the associated DevicePath and we can differentiate between local players easily.

### Challenge 2 : A large crowd that the player has to navigate
Crowds are known to be difficult to create in games for multiple reasons, but one the main thing is performance.
When a game starts, the procedural generation happens and just after that we spawn every NPC, around 1300 of them.

That is a lot of NPCs, but for us it was even worse because we had four cameras, all rotated at different angles at the start of each round.
When we first tested the crowd with the final character and not placeholder capsule, the game was unplayable with a frame time of ~35 ms (29 FPS).

FPS was a problem but the other issue was that at first in the game's design, the NPCs in the crowd were interacting with players (dancing, following, throwing their tongue, etc...)
and since our game was online it meant replicating all of this NPCs, due to time constraints and performance, we scrapped that idea.

Since the NPCs were not directly interacting with the players anymore, I tried to scrap the most amount of performance as possible with :
- Asking our artistic team for a version of the Character with less bone and less polys than the Player's one (it worked quite well, now our frame time was ~30 ms so 33 FPS)
- Implementing distance based culling that would totally disable the NPC's gameobject (frame time was now  ~28.2ms so 35.4 FPS)

Due to time constraints, this is the only optimisations we tried, with more time we could have explored solutions with DOTS ECS, or making the animations with shader instead of using the built-in animator.

### Challenge 3 : A physics-based tongue for interactions
What's one of the worst thing you can combine with an online game ? Physics !
Early in the development, the team decided that the main characters of the game would be Geckos, and thus,
the idea of having the Gecko interacting with its environment with their tongue was suggested.
One of the first thing I tested was to make a real physics-based tongue using an Unity plugin to simulate a rope ([Obi Rope](https://assetstore.unity.com/packages/tools/physics/obi-rope-55579?srsltid=AfmBOorwtYpKUIvK3BSZdcTCxvMs7aaQw_ISrfTv7nVd5xGT993HV4Bj)).
It worked well locally, but things started to get complicated when networking this system. First let's see the final version of this system.

#### The sensors of the tongue system
The system is composed of 3 sensors :
1. FOV Sensor : This is a sensor that is always facing in front of the player, it goes far and require the player to be precise
2. Aim Assist Sensor : This is the sensor that replace the FOV Sensor when the accessibility feature "Aim Assist" is activated _(or ApplicationSettings.UseRadialTongueSensor in the code)_, it enables the player to be less precise
3. Close Sensor : This is a sensor that is always activated, and is used to make it easier to kiss your ally when you are close to them, it only detects your ally

![Tongue System Sensors](Content/discogecko-tongue-sensors.jpg)

#### Showcase of the tongue's features
[![Disco Gecko Tongue System Showcase](https://res.cloudinary.com/marcomontalbano/image/upload/v1731239582/video_to_markdown/images/youtube--0vD_-j-0rPU-c05b58ac6eb4c4700831b2b3070cd403.jpg)](https://youtu.be/0vD_-j-0rPU "Disco Gecko Tongue System Showcase")

#### The replication of the tongue system
Some issues started to reveal themselves when working on the replication of the tongue system.
The first issue was that for non-host players to move objects that have a NetworkTransform component, they would have to take ownership, 
this is not really an issue itself, but that would later enforce the fact that anchors can only be interacted with 1 tongue at once,
because multiples clients taking ownership over the same object was not a technical challenge I could solve with the project time constraints.
However this issue HAD to be solved when starting to work on Player To Player tongue interaction with different clients. 
Here is my attempt to solve this (this code run inside the Player Controller Update):

```cs
// This code is only run on the locally controlled player

if (_otherPlayerAttachedFromTongue) // Is another player grabbing us with their tongue ?
{
    if (_otherPlayerAttachedFromTongue.GetNetworkPlayer().IsOnline) // Is the other player a remote client ?
    {
        _distanceToAttachedPlayer = Vector3.Distance(transform.position, _otherPlayerAttachedFromTongue.transform.position);
        // Should we be affected by the other player tongue ?
        _influencedByAttachedTongue = _distanceToAttachedPlayer > _networkPlayer.PlayerData.OtherTongueMinDistance; 
        if (_influencedByAttachedTongue) 
        {
            var direction = _otherPlayerAttachedFromTongue.transform.position - transform.position;
            direction.y = 0;
            direction.Normalize();
            // Add the direction to the movement
            movement += direction * _networkPlayer.PlayerData.OtherTongueAttachedForce;
        }
    }
    else // The other player is not a remote client, then just let the local physic do it's job
    {
        _influencedByAttachedTongue = false;
        _distanceToAttachedPlayer = 0;
    }
}
else // No player is grabbing us
{
    _influencedByAttachedTongue = false;
    _distanceToAttachedPlayer = 0;
}
```

Basically what this code does it that, in the case of a local player being grabbed by a 
Tongue that was belonging to an "Online Player" (which means another client connected to the server), 
then we try to simulate locally the force.
It does not work really well and the interaction feels quite laggy compared to the local version of this interaction. 
However, given the time we had, we decided it was "ok", since "player-to-player" tongue interaction is not the most frequent
interaction (player-to-object is almost 90 % of the interactions a player will do).


## What could be improved
- Use service lobbies instead of having the the lobby directly connecting the players into the same scene
- The replication of the physic interactions can be messy on non-host players
- Optimize the crowd

## Gallery
### UI Video Showcase
[![Disco Gecko UI Showcase](https://res.cloudinary.com/marcomontalbano/image/upload/v1731259320/video_to_markdown/images/youtube--P2-A9gy5t08-c05b58ac6eb4c4700831b2b3070cd403.jpg)](https://youtu.be/P2-A9gy5t08 "Disco Gecko UI Video Showcase")

### Fake Players and Possession Video Showcase
[![Disco Gecko Fake Players and Possession Showcase](https://res.cloudinary.com/marcomontalbano/image/upload/v1731259588/video_to_markdown/images/youtube--a56EZIJLckk-c05b58ac6eb4c4700831b2b3070cd403.jpg)](https://youtu.be/a56EZIJLckk "Disco Gecko Fake Players and Possession Video Showcase")

### Input Record and Replay for testing Video Showcase
[![Disco Gecko Input Record and Replay](https://res.cloudinary.com/marcomontalbano/image/upload/v1731260170/video_to_markdown/images/youtube--72La3kHCXA8-c05b58ac6eb4c4700831b2b3070cd403.jpg)](https://youtu.be/72La3kHCXA8 "Disco Gecko Input Record and Replay Video Showcase")

### Custom Logger Video Showcase
[![DiscoGecko Custom Logger Showcase](https://res.cloudinary.com/marcomontalbano/image/upload/v1731260479/video_to_markdown/images/youtube--loiA8GbgSHo-c05b58ac6eb4c4700831b2b3070cd403.jpg)](https://youtu.be/loiA8GbgSHo "DiscoGecko Custom Logger Video Showcase")