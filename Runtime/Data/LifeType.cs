namespace JobProgression.Data
{
    /// <summary>
    /// Broad category a Life/Job belongs to.
    /// Determines which stats scale, which tools are used, and how challenges are structured.
    /// </summary>
    public enum LifeType
    {
        Combat,     // Paladin, Mercenary, Hunter, Magician
        Gathering,  // Woodcutter, Miner, Angler
        Crafting    // Blacksmith, Carpenter, Tailor, Alchemist, Cook
    }
}
