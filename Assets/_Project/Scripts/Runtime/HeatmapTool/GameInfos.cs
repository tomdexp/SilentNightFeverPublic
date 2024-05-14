using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameInfos
{
    public List<LandmarksInfos> LandmarksLocation;
    public List<RoundInfos> RoundInfo;
}

[Serializable]
public class LandmarksInfos
{
    public string Name;
    public Vector3 Location;

    public LandmarksInfos(string newName, Vector3 newLocation)
    {
        Name = newName;
        Location = newLocation;
    }
}

[Serializable]
public class RoundInfos
{
    // float is time since beginning of the round.
    public List<PlayerInfos> PlayerInfos;
    public float timeInterval;
}

[Serializable]
public class PlayerInfos
{
    public PlayerInfos(Vector3 newPlayerALocation, Vector3 newPlayerBLocation , Vector3 newPlayerCLocation , Vector3 newPlayerDLocation) {
        PlayerALocation = newPlayerALocation;
        PlayerBLocation = newPlayerBLocation;
        PlayerCLocation = newPlayerCLocation;
        PlayerDLocation = newPlayerDLocation;
    }

    public Vector3 PlayerALocation;
    public Vector3 PlayerBLocation;
    public Vector3 PlayerCLocation;
    public Vector3 PlayerDLocation;
}