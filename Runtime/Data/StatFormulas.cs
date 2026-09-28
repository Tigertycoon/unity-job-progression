using UnityEngine;

namespace JobProgression.Data
{
    /// <summary>
    /// All derived stat formulas in one place.
    /// Converts base stats into derived combat/crafting values.
    /// Tweak constants here to rebalance the entire game.
    /// </summary>
    public static class StatFormulas
    {
        // ─── Base Constants ───────────────────────────────────────────
        public const int BaseHP = 50;
        public const int BaseSP = 30;
        public const float HPPerVIT = 3f;
        public const float HPPerLevel = 5f;
        public const float SPPerFOC = 2f;
        public const float SPPerLevel = 3f;

        // ─── Derived Stat Formulas ────────────────────────────────────

        /// <summary>Max HP = BaseHP + (VIT * 3) + (Level * 5)</summary>
        public static int CalculateHP(int vit, int level)
        {
            return BaseHP + Mathf.FloorToInt(vit * HPPerVIT) + Mathf.FloorToInt(level * HPPerLevel);
        }

        /// <summary>Max SP = BaseSP + (FOC * 2) + (Level * 3)</summary>
        public static int CalculateSP(int foc, int level)
        {
            return BaseSP + Mathf.FloorToInt(foc * SPPerFOC) + Mathf.FloorToInt(level * SPPerLevel);
        }

        /// <summary>Physical Attack = floor(STR * 1.5) + weaponAtk</summary>
        public static int CalculateAttack(int str, int weaponAtk = 0)
        {
            return Mathf.FloorToInt(str * 1.5f) + weaponAtk;
        }

        /// <summary>Physical Defense = floor(VIT * 1.0) + armorDef</summary>
        public static int CalculateDefense(int vit, int armorDef = 0)
        {
            return vit + armorDef;
        }

        /// <summary>Magic Attack = floor(INT * 1.5) + staffAtk</summary>
        public static int CalculateMagicAttack(int intel, int staffAtk = 0)
        {
            return Mathf.FloorToInt(intel * 1.5f) + staffAtk;
        }

        /// <summary>Magic Defense = floor(FOC * 1.0) + robeDef</summary>
        public static int CalculateMagicDefense(int foc, int robeDef = 0)
        {
            return foc + robeDef;
        }

        /// <summary>Crit Rate = 1% per 5 LUK, cap 50%</summary>
        public static float CalculateCritRate(int luk)
        {
            float raw = luk / 5f;
            return Mathf.Clamp(raw, 0f, 50f);
        }

        /// <summary>
        /// Crafting quality bonus from DEX (0–100 scale).
        /// Higher DEX → higher chance of Good/Great/Top quality.
        /// </summary>
        public static float CalculateCraftBonus(int dex)
        {
            return Mathf.Clamp(dex * 0.5f, 0f, 80f);
        }

        // ─── Damage Formula ───────────────────────────────────────────

        /// <summary>
        /// Damage formula:
        /// raw = (Atk * skillMult) - (Def * defMult)
        /// final = floor(raw * rand(0.95..1.05) * critMult * elementMult * buffMult)
        /// Minimum damage is always 1.
        /// </summary>
        public static int CalculateDamage(
            int attack,
            int defense,
            float skillMultiplier = 1f,
            float defenseMultiplier = 0.5f,
            bool isCritical = false,
            float critMultiplier = 1.5f,
            float elementMultiplier = 1f,
            float buffMultiplier = 1f)
        {
            float raw = (attack * skillMultiplier) - (defense * defenseMultiplier);
            raw = Mathf.Max(raw, 1f); // minimum 1 raw damage

            float variance = Random.Range(0.95f, 1.05f);
            float crit = isCritical ? critMultiplier : 1f;

            float finalDmg = raw * variance * crit * elementMultiplier * buffMultiplier;
            return Mathf.Max(1, Mathf.FloorToInt(finalDmg));
        }
    }
}
