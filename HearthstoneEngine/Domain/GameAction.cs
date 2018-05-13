namespace Domain
{
    using System;
    using System.Linq;
    using Infrastructure;

    public class GameAction
    {
        public GameAction(Player owner)
        {
            Owner = owner;
        }

        public Player Owner { get; private set; }
    }

    public class AttackAction : GameAction
    {
        public AttackAction(Player owner, Minion attacker, ITarget<Character> target)
            : base(owner)
        {
            Attacker = attacker;
            Target = target;
        }

        public Minion Attacker { get; private set; }
        public ITarget<Character> Target { get; private set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as AttackAction);
        }

        public bool Equals(AttackAction action2)
        {
            if (action2 == null) return false;

            return Attacker == action2.Attacker && Target == action2.Target;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

    public class SummonAction : GameAction
    {
        public SummonAction(Player owner, MinionCard card, int desiredBoardPosition = 0)
            : base(owner)
        {
            Card = card;
            DesiredBoardPosition = desiredBoardPosition;
        }

        public MinionCard Card { get; private set; }
        public int DesiredBoardPosition { get; private set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as SummonAction);
        }

        public bool Equals(SummonAction action2)
        {
            if (action2 == null) return false;

            return Card == action2.Card && DesiredBoardPosition == action2.DesiredBoardPosition;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

    public class SummonBattlecryTargetable : SummonAction
    {
        private readonly Targetable _applier;
        public Character Target { get; private set; }


        public GameState ApplyEffect(GameState gameState, Player player)
        {
            return _applier.ApplyOn(gameState, player, Target);
        }

        public SummonBattlecryTargetable(Player owner, BattlecryMinionCard card, Character target, int desiredBoardPosition = 0)
            : base(owner, card, desiredBoardPosition)
        {
            _applier = (Targetable)card.EffectApplier;
            Target = target;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SummonBattlecryTargetable);
        }

        public bool Equals(SummonBattlecryTargetable action2)
        {
            if (action2 == null) return false;
            return base.Equals(action2) && Target == action2.Target;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

    public class SummonBattlecryTargetless : SummonAction
    {
        private readonly Targetless _targetless;

        public GameState ApplyBattlecry(GameState gameState, Player owner, Minion caster)
        {
            return _targetless.ApplyOn(gameState, owner, caster);
        }

        public SummonBattlecryTargetless(Player owner, BattlecryMinionCard card, int desiredBoardPosition = 0)
            : base(owner, card, desiredBoardPosition)
        {
            _targetless = (Targetless)card.EffectApplier;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SummonBattlecryTargetable);
        }

        public bool Equals(SummonBattlecryTargetable action2)
        {
            if (action2 == null) return false;
            return base.Equals(action2);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class CastSpell : GameAction
    {
        protected CastSpell(Player owner, SpellCard card)
            : base(owner)
        {
            Card = card;
        }

        public SpellCard Card { get; private set; }

        public abstract GameState ApplyEffect(GameState gameState, Player owner);

        public override bool Equals(object obj)
        {
            return Equals(obj as CastSpell);
        }

        public bool Equals(CastSpell action2)
        {
            if (action2 == null) return false;
            return Card == action2.Card;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

    public class CastSpellTargetable : CastSpell
    {
        public Character Target { get; private set; }

        public CastSpellTargetable(Player owner, SpellCard card, Character target)
            : base(owner, card)
        {
            Target = target;
        }

        public override GameState ApplyEffect(GameState gameState, Player owner)
        {
            return Card.EffectAppliers.Aggregate(gameState, (state, appl) =>
                TypeSwitchExpr.On<EffectApplier, GameState>(appl)
                    .Case<Targetable>(applier => applier.ApplyOn(state, owner, Target))
                    .Case<Targetless>(applier => applier.ApplyOn(state, owner, null))
                    .ElseThrow());
        }
    }

    public class CastSpellTargetless : CastSpell
    {
        public CastSpellTargetless(Player owner, SpellCard card)
            : base(owner, card)
        {
        }

        public override GameState ApplyEffect(GameState gameState, Player owner)
        {
            return Card.EffectAppliers.Cast<Targetless>()
                .Aggregate(gameState, (state, applier) => applier.ApplyOn(state, owner, null));
        }
    }
}