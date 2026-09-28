using System.Collections.Generic;
using UnityEngine;

namespace JobProgression.Data
{
    /// <summary>
    /// Defines a Life/Job (e.g. Paladin, Miner, Blacksmith).
    /// Central ScriptableObject for the Life system.
    /// Each Life has its own stat bonus distribution per rank tier.
    /// </summary>
    [CreateAssetMenu(menuName = "Job Progression/Life Definition", fileName = "Life_New")]
    public class LifeDefinition : ScriptableObject
    {
        // ─── Identity ─────────────────────────────────────────────────
        public Sprite icon;

        public string lifeName;

        public LifeType lifeType;

        [TextArea(2, 4)]
        public string description;

        // ─── Rank Progression ─────────────────────────────────────────
        [Tooltip("Shared ranks with contiguous tier numbers starting at zero")]
        public List<RankDefinition> ranks = new();

        // ─── Stat Growth ──────────────────────────────────────────────
        [Tooltip("Stat bonuses per rank tier — a Paladin gets STR+VIT, a Tailor gets DEX+FOC")]
        public List<RankStatBonus> rankStatBonuses = new();

        // ─── Stat Affinities (display/UI) ─────────────────────────────
        [Tooltip("Primary stat this Life focuses on (for UI display)")]
        public CharacterStat primaryStat;

        [Tooltip("Secondary stat (for UI display)")]
        public CharacterStat secondaryStat;

        // ─── API ──────────────────────────────────────────────────────

        /// <summary>Get the rank definition for a given tier index.</summary>
        public RankDefinition GetRank(int tier)
        {
            return ranks?.Find(r => r != null && r.rankTier == tier);
        }

        /// <summary>Get the next rank after the given tier.</summary>
        public RankDefinition GetNextRank(int currentTier)
        {
            return GetRank(currentTier + 1);
        }

        /// <summary>Check if stars are enough for a specific rank tier.</summary>
        public bool CanReachRank(int tier, int currentStars)
        {
            var rank = GetRank(tier);
            return rank != null && currentStars >= rank.requiredStars;
        }

        /// <summary>
        /// Get total accumulated stat bonuses from rank 0 up to (and including) the given tier.
        /// This is the sum of all rank bonuses the player has earned.
        /// </summary>
        public RankStatBonus GetAccumulatedBonuses(int upToTier)
        {
            var total = new RankStatBonus();
            if (rankStatBonuses == null) return total;
            foreach (var bonus in rankStatBonuses)
            {
                if (bonus != null && bonus.rankTier >= 0 && bonus.rankTier <= upToTier)
                {
                    total.STR += bonus.STR;
                    total.VIT += bonus.VIT;
                    total.INT += bonus.INT;
                    total.FOC += bonus.FOC;
                    total.DEX += bonus.DEX;
                    total.LUK += bonus.LUK;
                }
            }
            return total;
        }

        /// <summary>Get stat bonus for a specific rank tier only.</summary>
        public RankStatBonus GetBonusForTier(int tier)
        {
            return rankStatBonuses?.Find(b => b != null && b.rankTier == tier);
        }
    }
}
