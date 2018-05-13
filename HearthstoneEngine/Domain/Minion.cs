namespace Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;

    public partial class Minion : Character, ITarget<Minion>
    {
        private readonly int _attack;
        private readonly int _attackBuff;
        private readonly int _healthMax;
        private readonly int _healthCurrent;
        private readonly int _healthBuffMax;
        private readonly int _healthBuffCurrent;
        private readonly Status _status;
        private readonly TriggeredAbility _ability;
        private readonly IEnumerable<EventType> _raisedEvents;
        private readonly IEnumerable<Targetless> _reactionEffects;
        private readonly int _id;

        public Minion(int attack, int health, Status status = Status.None, int? id = null, TriggeredAbility ability = null, IEnumerable<EventType> raisedEvents = null, IEnumerable<Targetless> reactionEffects = null, int attackBuff = 0, int? healthCurrent = null, int healthBuffMax = 0, int healthBuffCurrent = 0)
        {
            _id = id ?? IdProvider.GetNext();
            _attack = attack;
            _attackBuff = attackBuff;
            _healthMax = health;
            _healthCurrent = healthCurrent ?? health;
            _healthBuffMax = healthBuffMax;
            _healthBuffCurrent = healthBuffCurrent;
            _status = status;
            _ability = ability;
            _raisedEvents = raisedEvents ?? Enumerable.Empty<EventType>();
            _reactionEffects = reactionEffects ?? Enumerable.Empty<Targetless>();
        }

        public Minion(MinionCard card)
            : this(card.Attack, card.Health, card.Status, null, card is AbilityMinionCard ? ((AbilityMinionCard)card).Ability : null)
        {
        }

        public int Id
        {
            get { return _id; }
        }

        public bool IsJustSummoned { get; set; }

        public int AttackedTimes { get; set; }

        public override int Health
        {
            get { return _healthCurrent + _healthBuffCurrent; }
        }

        public override int Attack
        {
            get { return _attack + _attackBuff; }
        }

        public Minion RecieveDamage(int amount)
        {
            var withEventRaised = RaiseEvent(EventType.DamageRecieved);
            var withEvent = withEventRaised.EventOccured(new Event(EventType.DamageRecieved, Target.Self));

            if (_healthBuffCurrent > amount) return withEvent.With(healthBuffCurrent: _healthBuffCurrent - amount);

            var damageRemaining = (amount - _healthBuffCurrent);

            return withEvent.With(healthCurrent: _healthCurrent - damageRemaining, healthBuffCurrent: 0);
        }

        public Minion RecieveHeal(int amount)
        {
            var withEventRaised = RaiseEvent(EventType.HealRecieved);
            var withEvent = withEventRaised.EventOccured(new Event(EventType.HealRecieved, Target.Self));

            if (_healthCurrent + amount <= _healthMax) return withEvent.With(healthCurrent: _healthCurrent + amount);

            var healRemaining = amount - (_healthMax - _healthCurrent);
            if (_healthBuffCurrent + healRemaining <= _healthBuffMax)
                return withEvent.With(healthCurrent: _healthMax, healthBuffCurrent: _healthBuffCurrent + healRemaining);

            return withEvent.With(healthCurrent: _healthMax, healthBuffCurrent: _healthBuffMax);
        }

        public Minion AddStatus(Status status)
        {
            return With(status: _status | status);
        }

        public Minion RecievePermaBuff(int attack, int health)
        {
            return With(attackBuff: _attackBuff + attack, healthBuffMax: _healthBuffMax + health, healthBuffCurrent: _healthBuffCurrent + health);
        }

        public Minion RecieveDispell()
        {
            return With(status: Status.None, attackBuff: 0, healthBuffMax: 0, healthBuffCurrent: 0);
        }

        public Minion RecieveDestroy()
        {
            return With(healthCurrent: 0);
        }

        public bool HasStatus(Status status)
        {
            return _status.HasFlag(status);
        }

        public Minion RaiseEvent(EventType eventType)
        {
            return With(raisedEvents: _raisedEvents.Concat(eventType.Yield()));
        }

        public IEnumerable<EventType> RaisedEvents
        {
            get { return _raisedEvents; }
        }

        public Minion ClearRaisedEvents()
        {
            return With(raisedEvents: Enumerable.Empty<EventType>());
        }

        public Minion EventOccured(Event @event)
        {
            if (_ability != null && _ability.IsTriggeredBy(@event))
                return With(reactionEffects: _reactionEffects.Concat(_ability.Effect.Yield()));

            return this;
        }

        public IEnumerable<Targetless> ReactionEffects
        {
            get { return _reactionEffects; }
        }

        public Minion ClearReactionEffects()
        {
            return With(reactionEffects: Enumerable.Empty<Targetless>());
        }

        public override bool IsDead
        {
            get { return _healthCurrent <= 0; }
        }

        public override bool IsDamaged
        {
            get { return _healthCurrent < _healthMax || _healthBuffCurrent < _healthBuffMax; }
        }

        public bool CanAttack
        {
            get
            {
                return Attack > 0
                       && !HasStatus(Status.Frozen)
                       && (!IsJustSummoned || HasStatus(Status.Charge))
                       && (AttackedTimes < 1
                           || AttackedTimes < 2 && HasStatus(Status.Windfury)
                           || AttackedTimes < 4 && HasStatus(Status.MegaWindfury));
            }
        }
    }

    public partial class Minion
    {
        public static Minion Empty
        {
            get { return new Minion(0, 0); }
        }

        public Minion With(int? attack = null, int? health = null, Status? status = null, TriggeredAbility ability = null, IEnumerable<EventType> raisedEvents = null, IEnumerable<Targetless> reactionEffects = null, int? attackBuff = null, int? healthCurrent = null, int? healthBuffMax = null, int? healthBuffCurrent = null)
        {
            return new Minion(attack ?? _attack, health ?? _healthMax, status ?? _status, _id, ability ?? _ability, raisedEvents ?? _raisedEvents,
                reactionEffects ?? _reactionEffects, attackBuff ?? _attackBuff, healthCurrent ?? _healthCurrent,
                healthBuffMax ?? _healthBuffMax, healthBuffCurrent ?? _healthBuffCurrent);
        }

        //public override bool Equals(object obj)
        //{
        //    return Equals(obj as Minion);
        //}

        //public bool Equals(Minion minion2)
        //{
        //    if (minion2 == null) return false;
        //    return Id == minion2.Id;
        //}

        //public override int GetHashCode()
        //{
        //    return Id;
        //}
    }
}