namespace JobProgression.Data
{
    /// <summary>
    /// Six base stats used by the RPG progression prototype.
    ///
    /// Base stats feed into derived stats via formulas:
    ///   HP         = BaseHP + (VIT * 3) + (Level * 5)
    ///   SP         = BaseSP + (FOC * 2) + (Level * 3)
    ///   Attack     = floor(STR * 1.5) + WeaponAtk
    ///   Defense    = floor(VIT * 1.0) + ArmorDef
    ///   MagicAtk   = floor(INT * 1.5) + StaffAtk
    ///   MagicDef   = floor(FOC * 1.0) + RobeDef
    ///   CritRate   = 1% per 5 LUK (cap 50%)
    ///   CraftBonus = DEX affects crafting quality rolls
    /// </summary>
    public enum CharacterStat
    {
        STR,  // Strength    — Physical attack, gathering power
        VIT,  // Vitality    — HP, physical defense
        INT,  // Intelligence— Magic attack power
        FOC,  // Focus       — SP, magic defense
        DEX,  // Dexterity   — Crafting quality, speed, crit rate
        LUK   // Luck        — Drop rates, item quality, crit chance
    }
}
