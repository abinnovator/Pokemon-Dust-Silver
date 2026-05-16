using Game.Core;

public static class TypeChart
{
    public static float GetMultiplier(PokemonType moveType, PokemonType defenderType)
    {
        return (moveType, defenderType) switch
        {
            // Normal
            (PokemonType.Normal, PokemonType.Rock) => 0.5f,
            (PokemonType.Normal, PokemonType.Steel) => 0.5f,
            (PokemonType.Normal, PokemonType.Ghost) => 0f,

            // Fire
            (PokemonType.Fire, PokemonType.Fire) => 0.5f,
            (PokemonType.Fire, PokemonType.Water) => 0.5f,
            (PokemonType.Fire, PokemonType.Rock) => 0.5f,
            (PokemonType.Fire, PokemonType.Dragon) => 0.5f,
            (PokemonType.Fire, PokemonType.Grass) => 2f,
            (PokemonType.Fire, PokemonType.Ice) => 2f,
            (PokemonType.Fire, PokemonType.Bug) => 2f,
            (PokemonType.Fire, PokemonType.Steel) => 2f,

            // Water
            (PokemonType.Water, PokemonType.Water) => 0.5f,
            (PokemonType.Water, PokemonType.Grass) => 0.5f,
            (PokemonType.Water, PokemonType.Dragon) => 0.5f,
            (PokemonType.Water, PokemonType.Fire) => 2f,
            (PokemonType.Water, PokemonType.Ground) => 2f,
            (PokemonType.Water, PokemonType.Rock) => 2f,

            // Electric
            (PokemonType.Electric, PokemonType.Electric) => 0.5f,
            (PokemonType.Electric, PokemonType.Grass) => 0.5f,
            (PokemonType.Electric, PokemonType.Dragon) => 0.5f,
            (PokemonType.Electric, PokemonType.Ground) => 0f,
            (PokemonType.Electric, PokemonType.Water) => 2f,
            (PokemonType.Electric, PokemonType.Flying) => 2f,

            // Grass
            (PokemonType.Grass, PokemonType.Fire) => 0.5f,
            (PokemonType.Grass, PokemonType.Grass) => 0.5f,
            (PokemonType.Grass, PokemonType.Poison) => 0.5f,
            (PokemonType.Grass, PokemonType.Flying) => 0.5f,
            (PokemonType.Grass, PokemonType.Bug) => 0.5f,
            (PokemonType.Grass, PokemonType.Dragon) => 0.5f,
            (PokemonType.Grass, PokemonType.Steel) => 0.5f,
            (PokemonType.Grass, PokemonType.Water) => 2f,
            (PokemonType.Grass, PokemonType.Ground) => 2f,
            (PokemonType.Grass, PokemonType.Rock) => 2f,

            // Ice
            (PokemonType.Ice, PokemonType.Water) => 0.5f,
            (PokemonType.Ice, PokemonType.Ice) => 0.5f,
            (PokemonType.Ice, PokemonType.Steel) => 0.5f,
            (PokemonType.Ice, PokemonType.Fire) => 0.5f,
            (PokemonType.Ice, PokemonType.Grass) => 2f,
            (PokemonType.Ice, PokemonType.Ground) => 2f,
            (PokemonType.Ice, PokemonType.Flying) => 2f,
            (PokemonType.Ice, PokemonType.Dragon) => 2f,

            // Fighting
            (PokemonType.Fighting, PokemonType.Poison) => 0.5f,
            (PokemonType.Fighting, PokemonType.Flying) => 0.5f,
            (PokemonType.Fighting, PokemonType.Psychic) => 0.5f,
            (PokemonType.Fighting, PokemonType.Bug) => 0.5f,
            (PokemonType.Fighting, PokemonType.Fairy) => 0.5f,
            (PokemonType.Fighting, PokemonType.Ghost) => 0f,
            (PokemonType.Fighting, PokemonType.Normal) => 2f,
            (PokemonType.Fighting, PokemonType.Ice) => 2f,
            (PokemonType.Fighting, PokemonType.Rock) => 2f,
            (PokemonType.Fighting, PokemonType.Dark) => 2f,
            (PokemonType.Fighting, PokemonType.Steel) => 2f,

            // Poison
            (PokemonType.Poison, PokemonType.Poison) => 0.5f,
            (PokemonType.Poison, PokemonType.Ground) => 0.5f,
            (PokemonType.Poison, PokemonType.Rock) => 0.5f,
            (PokemonType.Poison, PokemonType.Ghost) => 0.5f,
            (PokemonType.Poison, PokemonType.Steel) => 0f,
            (PokemonType.Poison, PokemonType.Grass) => 2f,
            (PokemonType.Poison, PokemonType.Fairy) => 2f,

            // Ground
            (PokemonType.Ground, PokemonType.Grass) => 0.5f,
            (PokemonType.Ground, PokemonType.Bug) => 0.5f,
            (PokemonType.Ground, PokemonType.Flying) => 0f,
            (PokemonType.Ground, PokemonType.Fire) => 2f,
            (PokemonType.Ground, PokemonType.Electric) => 2f,
            (PokemonType.Ground, PokemonType.Poison) => 2f,
            (PokemonType.Ground, PokemonType.Rock) => 2f,
            (PokemonType.Ground, PokemonType.Steel) => 2f,

            // Flying
            (PokemonType.Flying, PokemonType.Electric) => 0.5f,
            (PokemonType.Flying, PokemonType.Rock) => 0.5f,
            (PokemonType.Flying, PokemonType.Steel) => 0.5f,
            (PokemonType.Flying, PokemonType.Grass) => 2f,
            (PokemonType.Flying, PokemonType.Fighting) => 2f,
            (PokemonType.Flying, PokemonType.Bug) => 2f,

            // Psychic
            (PokemonType.Psychic, PokemonType.Psychic) => 0.5f,
            (PokemonType.Psychic, PokemonType.Steel) => 0.5f,
            (PokemonType.Psychic, PokemonType.Dark) => 0f,
            (PokemonType.Psychic, PokemonType.Fighting) => 2f,
            (PokemonType.Psychic, PokemonType.Poison) => 2f,

            // Bug
            (PokemonType.Bug, PokemonType.Fire) => 0.5f,
            (PokemonType.Bug, PokemonType.Fighting) => 0.5f,
            (PokemonType.Bug, PokemonType.Flying) => 0.5f,
            (PokemonType.Bug, PokemonType.Ghost) => 0.5f,
            (PokemonType.Bug, PokemonType.Steel) => 0.5f,
            (PokemonType.Bug, PokemonType.Fairy) => 0.5f,
            (PokemonType.Bug, PokemonType.Grass) => 2f,
            (PokemonType.Bug, PokemonType.Psychic) => 2f,
            (PokemonType.Bug, PokemonType.Dark) => 2f,

            // Rock
            (PokemonType.Rock, PokemonType.Fighting) => 0.5f,
            (PokemonType.Rock, PokemonType.Ground) => 0.5f,
            (PokemonType.Rock, PokemonType.Steel) => 0.5f,
            (PokemonType.Rock, PokemonType.Fire) => 2f,
            (PokemonType.Rock, PokemonType.Ice) => 2f,
            (PokemonType.Rock, PokemonType.Flying) => 2f,
            (PokemonType.Rock, PokemonType.Bug) => 2f,

            // Ghost
            (PokemonType.Ghost, PokemonType.Normal) => 0f,
            (PokemonType.Ghost, PokemonType.Dark) => 0.5f,
            (PokemonType.Ghost, PokemonType.Ghost) => 2f,
            (PokemonType.Ghost, PokemonType.Psychic) => 2f,

            // Dragon
            (PokemonType.Dragon, PokemonType.Steel) => 0.5f,
            (PokemonType.Dragon, PokemonType.Fairy) => 0f,
            (PokemonType.Dragon, PokemonType.Dragon) => 2f,

            // Dark
            (PokemonType.Dark, PokemonType.Fighting) => 0.5f,
            (PokemonType.Dark, PokemonType.Dark) => 0.5f,
            (PokemonType.Dark, PokemonType.Fairy) => 0.5f,
            (PokemonType.Dark, PokemonType.Ghost) => 2f,
            (PokemonType.Dark, PokemonType.Psychic) => 2f,

            // Steel
            (PokemonType.Steel, PokemonType.Fire) => 0.5f,
            (PokemonType.Steel, PokemonType.Water) => 0.5f,
            (PokemonType.Steel, PokemonType.Electric) => 0.5f,
            (PokemonType.Steel, PokemonType.Steel) => 0.5f,
            (PokemonType.Steel, PokemonType.Ice) => 2f,
            (PokemonType.Steel, PokemonType.Rock) => 2f,
            (PokemonType.Steel, PokemonType.Fairy) => 2f,

            // Fairy
            (PokemonType.Fairy, PokemonType.Fire) => 0.5f,
            (PokemonType.Fairy, PokemonType.Poison) => 0.5f,
            (PokemonType.Fairy, PokemonType.Steel) => 0.5f,
            (PokemonType.Fairy, PokemonType.Fighting) => 2f,
            (PokemonType.Fairy, PokemonType.Dragon) => 2f,
            (PokemonType.Fairy, PokemonType.Dark) => 2f,

            _ => 1f
        };
    }

    public static string GetEffectivenessMessage(float multiplier)
    {
        return multiplier switch
        {
            0f => "It had no effect...",
            < 1f => "It's not very effective...",
            > 1f => "It's super effective!",
            _ => ""
        };
    }
}