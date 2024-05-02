using System;

namespace _Project.Scripts.Runtime.Landmarks
{
    [Flags]
    public enum LandmarkTag
    {
        Tag1 = 1 << 0,
        Tag2 = 1 << 1,
        Tag3 = 1 << 2,
        Tag4 = 1 << 3,
        Tag5 = 1 << 4,
        Tag6 = 1 << 5,
    }
}