namespace Domain
{
    public static class Spells
    {
        private const int MaxHeal = int.MaxValue;

        public static SpellCard TheCoin = new SpellCard(0, new Targetless(new DrawEffect(0), Target.FriendlyPlayer), Class.Neutral); // TODO

        // Druid
        public static SpellCard Moonfire = new SpellCard(0, new Targetable(new DamageEffect(1)), Class.Druid);
        public static SpellCard MarkOfTheWild = new SpellCard(2, new Targetable(new CharacterEffect[] { new PermaBuffEffect(2, 2), new StatusEffect(Status.Taunt) }, Target.AllMinions), Class.Druid);
        public static SpellCard HealingTouch = new SpellCard(3, new Targetable(new HealEffect(8)), Class.Druid);
        public static SpellCard Starfire = new SpellCard(6, new EffectApplier[] { new Targetable(new DamageEffect(5)), new Targetless(new DrawEffect(1), Target.FriendlyPlayer) }, Class.Druid);

        // Hunter
        public static SpellCard ArcaneShot = new SpellCard(1, new Targetable(new DamageEffect(2)), Class.Hunter);

        // Mage
        public static SpellCard MirrorImage = new SpellCard(1, new Targetless(new Effect[] { new SummonEffect(Minions.Uncollectible.MirrorImage), new SummonEffect(Minions.Uncollectible.MirrorImage) }, Target.FriendlyPlayer), Class.Mage);
        public static SpellCard ArcaneExplosion = new SpellCard(2, new Targetless(new DamageEffect(1), Target.EnemyMinions), Class.Mage);
        public static SpellCard FrostBolt = new SpellCard(2, new Targetable(new CharacterEffect[] { new DamageEffect(3), new StatusEffect(Status.Frozen) }), Class.Mage);
        public static SpellCard ArcaneIntellect = new SpellCard(3, new Targetless(new DrawEffect(2), Target.FriendlyPlayer), Class.Mage);
        public static SpellCard FrostNova = new SpellCard(3, new Targetless(new StatusEffect(Status.Frozen), Target.EnemyMinions), Class.Mage);
        public static SpellCard FireBall = new SpellCard(4, new Targetable(new DamageEffect(6)), Class.Mage);
        public static SpellCard Polymorph = new SpellCard(4, new Targetable(new TransformEffect(Minions.Uncollectible.Sheep)), Class.Mage);
        public static SpellCard FlameStrike = new SpellCard(7, new Targetless(new DamageEffect(4), Target.EnemyMinions), Class.Mage);

        // Paladin
        public static SpellCard BlessingOfMight = new SpellCard(1, new Targetable(new PermaBuffEffect(3, 0), Target.AllMinions), Class.Paladin);
        public static SpellCard HandOfProtection = new SpellCard(1, new Targetable(new StatusEffect(Status.DivineShield), Target.AllMinions), Class.Paladin);
        public static SpellCard HolyLight = new SpellCard(2, new Targetable(new HealEffect(6)), Class.Paladin);
        public static SpellCard BlessingOfKings = new SpellCard(4, new Targetable(new PermaBuffEffect(4, 4), Target.AllMinions), Class.Paladin);
        public static SpellCard Consecration = new SpellCard(4, new Targetless(new DamageEffect(2), Target.AllEnemies), Class.Paladin);
        public static SpellCard HammerOfWrath = new SpellCard(4, new EffectApplier[] { new Targetable(new DamageEffect(3)), new Targetless(new DrawEffect(1), Target.FriendlyPlayer) }, Class.Paladin);

        // Priest
        public static SpellCard CircleOfHealing = new SpellCard(0, new Targetless(new HealEffect(4), Target.AllMinions), Class.Priest);
        public static SpellCard HolySmite = new SpellCard(1, new Targetable(new DamageEffect(2)), Class.Priest);
        public static SpellCard PowerWordShield = new SpellCard(1, new EffectApplier[] { new Targetable(new PermaBuffEffect(0, 2), Target.FriendlyMinions), new Targetless(new DrawEffect(1), Target.FriendlyPlayer) }, Class.Priest);
        public static SpellCard MindBlast = new SpellCard(2, new Targetless(new DamageEffect(5), Target.EnemyFaces), Class.Priest);
        public static SpellCard ShadowWordPain = new SpellCard(2, new Targetable(new DestroyEffect(), Target.AllMinions, x => x.Attack <= 3), Class.Priest);
        public static SpellCard ShadowWordDeath = new SpellCard(3, new Targetable(new DestroyEffect(), Target.AllMinions, x => x.Attack >= 5), Class.Priest);
        public static SpellCard HolyNova = new SpellCard(5, new[] { new Targetless(new DamageEffect(2), Target.AllEnemies), new Targetless(new HealEffect(2), Target.AllAllies) }, Class.Priest);

        // Rogue
        public static SpellCard Backstab = new SpellCard(0, new Targetable(new DamageEffect(2), Target.AllMinions, x => !x.IsDamaged), Class.Rogue);
        public static SpellCard SinisterStrike = new SpellCard(1, new Targetless(new DamageEffect(3), Target.EnemyFaces), Class.Rogue);
        public static SpellCard Shiv = new SpellCard(2, new EffectApplier[] { new Targetable(new DamageEffect(1)), new Targetless(new DrawEffect(1), Target.FriendlyPlayer) }, Class.Rogue);
        public static SpellCard FanOfKnives = new SpellCard(3, new[] { new Targetless(new DamageEffect(1), Target.EnemyMinions), new Targetless(new DrawEffect(1), Target.FriendlyPlayer) }, Class.Rogue);
        public static SpellCard Assassinate = new SpellCard(5, new Targetable(new DestroyEffect(), Target.EnemyMinions), Class.Rogue);
        public static SpellCard Sprint = new SpellCard(7, new Targetless(new DrawEffect(4), Target.FriendlyPlayer), Class.Rogue);


        // Shaman
        public static SpellCard AncestralHealing = new SpellCard(0, new Targetable(new CharacterEffect[] { new HealEffect(MaxHeal), new StatusEffect(Status.Taunt) }, Target.AllMinions), Class.Shaman);
        public static SpellCard FrostShock = new SpellCard(1, new Targetable(new CharacterEffect[] { new DamageEffect(1), new StatusEffect(Status.Frozen) }, Target.AllEnemies), Class.Shaman);
        public static SpellCard WindFury = new SpellCard(2, new Targetable(new StatusEffect(Status.Windfury), Target.AllMinions), Class.Shaman);
        public static SpellCard Hex = new SpellCard(3, new Targetable(new TransformEffect(Minions.Uncollectible.Frog), Target.AllMinions), Class.Shaman);

        // Warlock
        public static SpellCard DrainLife = new SpellCard(3, new EffectApplier[] { new Targetable(new DamageEffect(2)), new Targetless(new HealEffect(2), Target.FriendlyFaces) }, Class.Warlock);
        public static SpellCard ShadowBolt = new SpellCard(3, new Targetable(new DamageEffect(4), Target.AllMinions), Class.Warlock);
        public static SpellCard Hellfire = new SpellCard(4, new Targetless(new DamageEffect(3), Target.All), Class.Warlock);

        // Warrior
        public static SpellCard Execute = new SpellCard(1, new Targetable(new DestroyEffect(), Target.EnemyMinions, x => x.IsDamaged), Class.Warrior);
        public static SpellCard Whirlwind = new SpellCard(1, new Targetless(new DamageEffect(1), Target.AllMinions), Class.Warrior);
        public static SpellCard Charge = new SpellCard(3, new Targetable(new CharacterEffect[] { new PermaBuffEffect(2, 0), new StatusEffect(Status.Charge) }, Target.FriendlyMinions), Class.Warrior);
    }
}