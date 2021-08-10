using Newtonsoft.Json;
using System.Collections.Generic;

namespace DGP.Genshin.Models.MiHoYo.Record.Activity
{
    public class ActivitiesInfo
    {
        [JsonProperty("effigy")] public Activity Effigy { get; set; }
        [JsonProperty("mechanicus")] public Activity Mechanicus { get; set; }
        [JsonProperty("fleur_fair")] public FleurFair FleurFair { get; set; }
        [JsonProperty("channeller_slab")] public Activity ChannellerSlab { get; set; }
        [JsonProperty("martial_legend")] public Activity MartialLegend { get; set; }

        //ignore a duplicated activities node
    }

    public class FleurFair
    {
        [JsonProperty("fleur_fair_gallery")]public Activity FleurFairGallery { get; set; }
        [JsonProperty("fleur_fair_dungeon")] public Activity FleurFairDungeon { get; set; }
        [JsonProperty("is_sub_activity")] public bool IsSubActivity { get; set; }
    }

    public class Activity
    {
        [JsonProperty("start_time")] public long StartTime { get; set; }
        [JsonProperty("end_time")] public long EndTime { get; set; }
        [JsonProperty("total_score")] public int TotalScore { get; set; }
        [JsonProperty("total_times")] public int TotalTimes { get; set; }
        [JsonProperty("game_collections")] public List<GameWrapper> GameCollections { get; set; }
        [JsonProperty("gears")] public List<Gear> Gears { get; set; }//only in Mechanicus
        [JsonProperty("records")] public List<Challenge> Records { get; set; }
        [JsonProperty("exists_data")] public bool ExistsData { get; set; }
    }

    public class GameWrapper
    {
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("games")] public List<Game> Games { get; set; }
    }

    public class Game
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("records")] public List<MaxScoreDifficulty> Records { get; set; }
    }

    public class MaxScoreDifficulty
    {
        [JsonProperty("max_score")] public int MaxScore { get; set; }
        [JsonProperty("difficulty")] public int Difficulty { get; set; }
    }

    public class Challenge : MaxScoreDifficulty
    {
        [JsonProperty("avatars")] public List<Avatar> Avatars { get; set; }
        [JsonProperty("is_multiplayer_online")] public bool IsMultiplayerOnline { get; set; }
        [JsonProperty("challenge_id")] public int ChallengeId { get; set; }
        [JsonProperty("challenge_name")] public string ChallengeName { get; set; }
        [JsonProperty("limit_conditions")] public List<Condition> LimitConditions { get; set; }
        [JsonProperty("score_multiple")] public double ScoreMultiple { get; set; }
        [JsonProperty("score")] public int Score { get; set; }//only in FleurFair
        [JsonProperty("stage_names")] public List<string> StageNames { get; set; }//only in FleurFair
        [JsonProperty("settle_time")] public long SettleTime { get; set; }
    }

    public class Avatar
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
    }

    public class Condition
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("desc")] public string Description { get; set; }
        [JsonProperty("score")] public int Score { get; set; }
    }
    public class Gear
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
        [JsonProperty("element")] public string Element { get; set; }
        [JsonProperty("is_unlocked")] public bool IsUnlocked { get; set; }
    }

}
