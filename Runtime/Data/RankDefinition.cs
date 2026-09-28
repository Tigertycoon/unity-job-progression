using UnityEngine;

namespace JobProgression.Data
{
    /// <summary>
    /// Defines a rank tier shared across all Lives (e.g. Novice, Fledgling...).
    /// Contains only universal data: name, tier, star cost, skill cap.
    /// Per-Life stat bonuses are on LifeDefinition instead.
    /// </summary>
    [CreateAssetMenu(menuName = "Job Progression/Rank Definition", fileName = "Rank_New")]
    public class RankDefinition : ScriptableObject
    {
        [Tooltip("Display name (e.g. 'Novice', 'Fledgling')")]
        public string rankName;

        [Tooltip("Tier index (0 = Novice ... 8 = God/Creator)")]
        [Range(0, 8)]
        public int rankTier;

        [Tooltip("Total stars required to reach this rank")]
        [Min(0)]
        public int requiredStars;

        [Tooltip("Skill level cap at this rank (base 15, max 20)")]
        [Range(15, 20)]
        public int skillCap = 15;
    }
}
