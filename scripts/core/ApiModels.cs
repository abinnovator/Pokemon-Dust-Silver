using System.Collections.Generic;
using Newtonsoft.Json;

namespace Game.Core
{
    public class ApiResource
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    #region Move API Models
    public class EffectEntry
    {
        [JsonProperty("effect")]
        public string Effect { get; set; }
        [JsonProperty("short_effect")]
        public string ShortEffect { get; set; }
        [JsonProperty("language")]
        public ApiResource Language { get; set; }
        [JsonProperty("version_group")]
        public ApiResource VersionGroup { get; set; }
    }

    public class MoveMeta
    {
        [JsonProperty("ailment")]
        public ApiResource Ailment { get; set; }
        [JsonProperty("category")]
        public ApiResource Category { get; set; }
        [JsonProperty("crit_rate")]
        public int? CritRate { get; set; }
        [JsonProperty("drain")]
        public int? Drain { get; set; }
        [JsonProperty("flinch_chance")]
        public int? FlinchChance { get; set; }
        [JsonProperty("healing")]
        public int? Healing { get; set; }
        [JsonProperty("max_hits")]
        public int? MaxHits { get; set; }
        [JsonProperty("max_turns")]
        public int? MaxTurns { get; set; }
        [JsonProperty("min_hits")]
        public int? MinHits { get; set; }
        [JsonProperty("min_turns")]
        public int? MinTurns { get; set; }
        [JsonProperty("stat_chance")]
        public int? StatChance { get; set; }
        [JsonProperty("ailment_chance")]
        public int? AilmentChance { get; set; }
    }

    public class StatChangeEntry
    {
        [JsonProperty("stat")]
        public ApiResource Stat { get; set; }
        [JsonProperty("change")]
        public int Change { get; set; }
    }

    public class MoveApiResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("accuracy")]
        public int? Accuracy { get; set; }
        [JsonProperty("effect_chance")]
        public int? EffectChance { get; set; }
        [JsonProperty("pp")]
        public int? Pp { get; set; }
        [JsonProperty("priority")]
        public int Priority { get; set; }
        [JsonProperty("power")]
        public int? Power { get; set; }
        [JsonProperty("target")]
        public ApiResource Target { get; set; }
        [JsonProperty("damage_class")]
        public ApiResource DamageClass { get; set; }
        [JsonProperty("generation")]
        public ApiResource Generation { get; set; }
        [JsonProperty("meta")]
        public MoveMeta Meta { get; set; }
        [JsonProperty("stat_changes")]
        public List<StatChangeEntry> StatChanges { get; set; }
        [JsonProperty("type")]
        public ApiResource Type { get; set; }
        [JsonProperty("effect_entries")]
        public List<EffectEntry> EffectEntries { get; set; }
    }
    #endregion

    #region Pokemon API Models
    public class TypeSlot
    {
        [JsonProperty("slot")]
        public int Slot { get; set; }
        [JsonProperty("type")]
        public ApiResource Type { get; set; }
    }

    public class StatSlot
    {
        [JsonProperty("base_stat")]
        public int BaseStat { get; set; }
        [JsonProperty("effort")]
        public int Effort { get; set; }
        [JsonProperty("stat")]
        public ApiResource Stat { get; set; }
    }

    public class SpeciesRef
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class VersionGroupDetail
    {
        [JsonProperty("level_learned_at")]
        public int LevelLearnedAt { get; set; }
        [JsonProperty("move_learn_method")]
        public ApiResource MoveLearnMethod { get; set; }
        [JsonProperty("version_group")]
        public ApiResource VersionGroup { get; set; }
    }

    public class PokemonMoveEntry
    {
        [JsonProperty("move")]
        public ApiResource Move { get; set; }
        [JsonProperty("version_group_details")]
        public List<VersionGroupDetail> VersionGroupDetails { get; set; }
    }
    public class PokemonSprites
    {
        [JsonProperty("front_default")]
        public string front_default { get; set; }
        [JsonProperty("front_shiny")]
        public string front_shiny { get; set; }
        [JsonProperty("back_default")]
        public string back_default { get; set; }
        [JsonProperty("back_shiny")]
        public string back_shiny { get; set; }
    }

    public class PokemonApiResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("weight")]
        public int Weight { get; set; }
        [JsonProperty("base_experience")]
        public int BaseExperience { get; set; }
        [JsonProperty("types")]
        public List<TypeSlot> Types { get; set; }
        [JsonProperty("stats")]
        public List<StatSlot> Stats { get; set; }
        [JsonProperty("species")]
        public ApiResource Species { get; set; }
        [JsonProperty("moves")]
        public List<PokemonMoveEntry> Moves { get; set; }
        [JsonProperty("sprites")]
        public PokemonSprites Sprites { get; set; }
    }

    public class FlavorTextEntry
    {
        [JsonProperty("flavor_text")]
        public string FlavorText { get; set; }
        [JsonProperty("language")]
        public ApiResource Language { get; set; }
        [JsonProperty("version")]
        public ApiResource Version { get; set; }
    }

    public class PokemonSpeciesResponse
    {
        [JsonProperty("flavor_text_entries")]
        public List<FlavorTextEntry> FlavorTextEntries { get; set; }
    }
    #endregion
}
