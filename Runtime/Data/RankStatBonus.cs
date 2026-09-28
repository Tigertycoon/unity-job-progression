using UnityEngine;

namespace JobProgression.Data
{
    /// <summary>
    /// Stat bonuses granted when reaching a specific rank tier in a Life.
    /// Each Life has its own set of these — a Paladin's Fledgling bonus differs from a Tailor's.
    /// </summary>
    [System.Serializable]
    public class RankStatBonus
    {
        [Tooltip("Which rank tier these bonuses apply to (0=Novice, 1=Fledgling, etc.)")]
        [Range(0, 8)]
        public int rankTier;

        [Min(0)] public int STR;

        [Min(0)] public int VIT;

        [Min(0)] public int INT;

        [Min(0)] public int FOC;

        [Min(0)] public int DEX;

        [Min(0)] public int LUK;

        /// <summary>Total stat points gained at this rank.</summary>
        public int TotalPoints => STR + VIT + INT + FOC + DEX + LUK;

        /// <summary>Get a specific stat's bonus value.</summary>
        public int Get(CharacterStat stat) => stat switch
        {
            CharacterStat.STR => STR,
            CharacterStat.VIT => VIT,
            CharacterStat.INT => INT,
            CharacterStat.FOC => FOC,
            CharacterStat.DEX => DEX,
            CharacterStat.LUK => LUK,
            _ => 0
        };
    }
}
