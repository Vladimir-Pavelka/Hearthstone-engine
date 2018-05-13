namespace Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;

    public class Card
    {
        public Card(int manaCost, Class cardClass)
        {
            ManaCost = manaCost;
            Class = cardClass;
        }

        public int ManaCost { get; private set; }
        public Class Class { get; set; }
    }

    public class MinionCard : Card
    {
        public int Attack { get; private set; }
        public int Health { get; private set; }
        public Status Status { get; private set; }

        public MinionCard(int manaCost, int attack, int health, Class cardClass = Class.Neutral, Status status = Status.None)
            : base(manaCost, cardClass)
        {
            Attack = attack;
            Health = health;
            Status = status;
        }

        public Minion AsOnboard()
        {
            return new Minion(this);
        }
    }

    public class BattlecryMinionCard : MinionCard
    {
        public EffectApplier EffectApplier { get; private set; }

        public BattlecryMinionCard(int manaCost, int attack, int health, EffectApplier applier, Class cardClass = Class.Neutral, Status status = Status.None)
            : base(manaCost, attack, health, cardClass, status)
        {
            EffectApplier = applier;
        }
    }

    public class AbilityMinionCard : MinionCard
    {
        public TriggeredAbility Ability { get; private set; }

        public AbilityMinionCard(int manaCost, int attack, int health, TriggeredAbility ability, Class cardClass = Class.Neutral, Status status = Status.None)
            : base(manaCost, attack, health, cardClass, status)
        {
            Ability = ability;
        }
    }

    public class SpellCard : Card
    {
        public IEnumerable<EffectApplier> EffectAppliers { get; private set; }
        public bool IsTargetable
        {
            get { return EffectAppliers.First() is Targetable; }
        }


        public SpellCard(int manaCost, EffectApplier applier, Class cardClass)
            : this(manaCost, applier.Yield(), cardClass)
        {
        }

        public SpellCard(int manaCost, IEnumerable<EffectApplier> appliers, Class cardClass)
            : base(manaCost, cardClass)
        {
            EffectAppliers = appliers;
        }
    }
}