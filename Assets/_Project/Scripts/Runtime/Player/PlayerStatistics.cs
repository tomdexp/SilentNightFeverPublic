namespace _Project.Scripts.Runtime.Player
{
    // Those statistics should be locally calculated and then sent to the server at the end of the game
    public class PlayerStatistics
    {
        public uint TongueThrowCount;
        public uint LandmarkActivatedCount;
        public uint PositiveEffectActivatedCount; // The player is the source of the effect
        public uint NegativeEffectActivatedCount; // The player is the source of the effect
        public uint EffectSufferedCount; // The player is the target of the effect
        public uint ElectrocutedCount;
        public float MeanDistanceToCenter;
        public uint MetersTravelled;
        public uint ConsumablesEatenCount;
        public uint SecondsEnemyTongueStuck;
        public uint SecondsCloseToAlly;
        public uint SecondsInTallGrass;
        public uint SecondsVoodooDollActive;
        public uint SecondsZoomActive;
        public uint SecondsDynamicObjectGrabbed;
        public uint UniqueTongueActivationCount; // "Faire ses dents" : "joueur ayant léché le + de trucs différents sur la map"
        public uint GiftToGodsCount;
        public uint GodsAngryCount;
        public uint SecondsTongueStickToPlayerWithTongueAlreadyOut; // "Lizard Centipede" : "joueur ayant collé le + souvent un joueur collant déjà qqch / un joueur"
    }
}