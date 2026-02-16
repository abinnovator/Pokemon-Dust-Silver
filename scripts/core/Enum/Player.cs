namespace Game.Core;

public enum PlayerStoryState 
{
    NEW_GAME,        // Just started, hasn't reached the lab
    MET_OAK,         // In the lab, currently choosing
    HAS_STARTER,     // Chosen a Pokemon, ready for Route 1
    DEFEATED_RIVAL,  // Completed first battle
    PARCEL_DELIVERED // Finished the "delivery" quest
}

public enum StarterChoice
{
    NONE,
    BULBASAUR,
    CHARMANDER,
    SQUIRTLE
}