using System;
using System.Collections.Generic;
using System.IO;
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
    public Dictionary<float, PlayerInfos> PlayerInfos;
}

[Serializable]
public class PlayerInfos
{
    public Vector3 PlayerALocation;
    public Vector3 PlayerBLocation;
    public Vector3 PlayerCLocation;
    public Vector3 PlayerDLocation;
}