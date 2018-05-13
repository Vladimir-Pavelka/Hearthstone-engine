namespace Domain
{
    using System;

    [Flags]
    public enum Status
    {
        None = 0,
        Taunt = 1,
        Windfury = 2,
        MegaWindfury = 4,
        Charge = 8,
        Stealth = 16,
        Elusive = 32,
        Poison = 64,
        Frozen = 128,
        DivineShield = 256
    }
}