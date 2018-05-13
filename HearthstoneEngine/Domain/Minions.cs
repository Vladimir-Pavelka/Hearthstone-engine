namespace Domain
{
    public static class Minions
    {
        // Neutral
        public static MinionCard GoldshireFootman = new MinionCard(1, 1, 2, status: Status.Taunt);
        public static MinionCard MurlocRaider = new MinionCard(1, 2, 1);
        public static MinionCard StonetuskBoar = new MinionCard(1, 1, 1, status: Status.Charge);
        public static MinionCard BloodfenRaptor = new MinionCard(2, 3, 2);
        public static MinionCard BluegillWarrior = new MinionCard(2, 2, 1, status: Status.Charge);
        public static MinionCard FrostwolfGrunt = new MinionCard(2, 2, 2, status: Status.Taunt);
        public static MinionCard RiverCrocolisk = new MinionCard(2, 2, 3);
        public static MinionCard IronfurGrizzly = new MinionCard(3, 3, 3, status: Status.Taunt);
        public static MinionCard MagmaRager = new MinionCard(3, 5, 1);
        public static MinionCard SilverbackPatriarch = new MinionCard(3, 1, 4, status: Status.Taunt);
        public static MinionCard Wolfrider = new MinionCard(3, 3, 1, status: Status.Charge);
        public static MinionCard ChillwindYeti = new MinionCard(4, 4, 5);
        public static MinionCard OasisSnapjaw = new MinionCard(4, 2, 7);
        public static MinionCard SenjinShieldmasta = new MinionCard(4, 3, 5, status: Status.Taunt);
        public static MinionCard StormwindKnight = new MinionCard(4, 2, 5, status: Status.Charge);
        public static MinionCard BootyBayBodyguard = new MinionCard(5, 5, 4, status: Status.Taunt);
        public static MinionCard BoulderfistOgre = new MinionCard(6, 6, 7);
        public static MinionCard LordOfTheArena = new MinionCard(6, 6, 5, status: Status.Taunt);
        public static MinionCard RecklessRocketeer = new MinionCard(6, 5, 2, status: Status.Charge);
        public static MinionCard CoreHound = new MinionCard(7, 9, 5);
        public static MinionCard WarGolem = new MinionCard(7, 7, 7);

        // Neutral Battlecry
        public static BattlecryMinionCard ElvenArcher = new BattlecryMinionCard(1, 1, 1, new Targetable(new DamageEffect(1)));
        public static BattlecryMinionCard VoodooDoctor = new BattlecryMinionCard(1, 2, 1, new Targetable(new HealEffect(2)));
        public static BattlecryMinionCard MurlocTidehunter = new BattlecryMinionCard(2, 2, 1, new Targetless(new SummonEffect(Uncollectible.MurlocScout), Target.FriendlyPlayer));
        public static BattlecryMinionCard NoviceEngineer = new BattlecryMinionCard(2, 1, 1, new Targetless(new DrawEffect(1), Target.FriendlyPlayer));
        public static BattlecryMinionCard EarthenRingFarseer = new BattlecryMinionCard(3, 3, 3, new Targetable(new HealEffect(3)));
        public static BattlecryMinionCard InjuredBlademaster = new BattlecryMinionCard(4, 4, 7, new Targetless(new DamageEffect(4), Target.Self));
        public static BattlecryMinionCard IronforgeRifleman = new BattlecryMinionCard(3, 2, 2, new Targetable(new DamageEffect(1)));
        public static BattlecryMinionCard RazorfenHunter = new BattlecryMinionCard(3, 2, 3, new Targetless(new SummonEffect(Uncollectible.Boar), Target.FriendlyPlayer));
        public static BattlecryMinionCard ShatteredSunCleric = new BattlecryMinionCard(3, 3, 2, new Targetable(new PermaBuffEffect(1, 1)));
        public static BattlecryMinionCard DefenderOfArgus = new BattlecryMinionCard(4, 2, 3, new Targetless(new Effect[] { new PermaBuffEffect(1, 1), new StatusEffect(Status.Taunt) }, Target.Adjacent));
        public static BattlecryMinionCard DragonlingMechanic = new BattlecryMinionCard(4, 2, 4, new Targetless(new SummonEffect(Uncollectible.MechanicalDragonling), Target.FriendlyPlayer));
        public static BattlecryMinionCard GnomishInventor = new BattlecryMinionCard(4, 2, 4, new Targetless(new DrawEffect(1), Target.FriendlyPlayer));
        public static BattlecryMinionCard Spellbreaker = new BattlecryMinionCard(4, 4, 3, new Targetable(new SilenceEffect(), Target.AllMinions));
        public static BattlecryMinionCard DarkscaleHealer = new BattlecryMinionCard(5, 4, 5, new Targetless(new HealEffect(2), Target.AllAllies));
        public static BattlecryMinionCard NightBlade = new BattlecryMinionCard(5, 4, 4, new Targetless(new DamageEffect(3), Target.EnemyFaces));
        public static BattlecryMinionCard StormpikeCommando = new BattlecryMinionCard(5, 4, 2, new Targetable(new DamageEffect(2)));
        public static BattlecryMinionCard FrostElemental = new BattlecryMinionCard(6, 5, 5, new Targetable(new StatusEffect(Status.Frozen)));

        // Neutral Ability
        public static AbilityMinionCard GurubashiBerserker = new AbilityMinionCard(5, 2, 7,
            new TriggeredAbility(new Event(EventType.DamageRecieved, Target.Self), new Targetless(new PermaBuffEffect(3, 0), Target.Self)));

        public static AbilityMinionCard WildPyromancer = new AbilityMinionCard(2, 3, 2,
            new TriggeredAbility(new Event(EventType.SpellCasted, Target.FriendlyPlayer), new Targetless(new DamageEffect(1), Target.AllMinions | Target.Self)));

        public static AbilityMinionCard AcolyteOfPain = new AbilityMinionCard(3, 1, 3,
            new TriggeredAbility(new Event(EventType.DamageRecieved, Target.Self), new Targetless(new DrawEffect(1), Target.FriendlyPlayer)));

        // Druid
        public static MinionCard IronbarkProtector = new MinionCard(8, 8, 8, Class.Druid, Status.Taunt);

        // Paladin
        public static BattlecryMinionCard ArgentProtector = new BattlecryMinionCard(2, 2, 2, new Targetable(new StatusEffect(Status.DivineShield), Target.FriendlyMinions), Class.Paladin);
        public static BattlecryMinionCard GuardianOfKings = new BattlecryMinionCard(7, 5, 6, new Targetless(new HealEffect(6), Target.FriendlyFaces), Class.Paladin);

        // Priest
        public static AbilityMinionCard NorthshireCleric = new AbilityMinionCard(1, 1, 3,
            new TriggeredAbility(new Event(EventType.HealRecieved, Target.AllMinions | Target.Self), new Targetless(new DrawEffect(1), Target.FriendlyPlayer)));

        // Shaman
        public static BattlecryMinionCard WindSpeaker = new BattlecryMinionCard(4, 3, 3, new Targetable(new StatusEffect(Status.Windfury), Target.FriendlyMinions), Class.Shaman);
        public static BattlecryMinionCard FireElemental = new BattlecryMinionCard(6, 6, 5, new Targetable(new DamageEffect(3)), Class.Shaman);

        // Warlock
        public static MinionCard VoidWalker = new MinionCard(1, 1, 3, Class.Warlock, Status.Taunt);
        public static BattlecryMinionCard DreadInfernal = new BattlecryMinionCard(6, 6, 6, new Targetless(new DamageEffect(1), Target.All), Class.Warlock);


        // Warrior
        public static MinionCard KorkronElite = new MinionCard(4, 4, 3, Class.Warrior, Status.Charge);
        // TODO: frothing

        public static class Uncollectible
        {
            public static MinionCard Frog = new MinionCard(1, 0, 1, status: Status.Taunt);
            public static MinionCard MirrorImage = new MinionCard(0, 0, 2, Class.Mage, Status.Taunt);
            public static MinionCard MurlocScout = new MinionCard(0, 1, 1);
            public static MinionCard Sheep = new MinionCard(0, 1, 1);
            public static MinionCard SearingTotem = new MinionCard(1, 1, 1, Class.Shaman);
            public static MinionCard SilverHandRecruit = new MinionCard(1, 1, 1, Class.Paladin);
            public static MinionCard StoneclawTotem = new MinionCard(1, 0, 2, Class.Shaman, Status.Taunt);
            public static MinionCard Boar = new MinionCard(1, 1, 1);
            public static MinionCard MechanicalDragonling = new MinionCard(1, 2, 1);
            public static MinionCard Huffer = new MinionCard(3, 4, 2, Class.Hunter, Status.Charge);
            public static MinionCard Misha = new MinionCard(3, 4, 4, Class.Hunter, Status.Taunt);
        }
    }
}