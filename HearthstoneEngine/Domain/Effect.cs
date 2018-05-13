namespace Domain
{
    using System;
    using System.Linq;

    [Flags]
    public enum Target
    {
        None = 0,
        FriendlyMinions = 1,
        FriendlyFaces = 2,
        EnemyMinions = 4,
        EnemyFaces = 8,
        All = FriendlyMinions | FriendlyFaces | EnemyMinions | EnemyFaces,
        AllMinions = FriendlyMinions | EnemyMinions,
        AllEnemies = EnemyMinions | EnemyFaces,
        AllAllies = FriendlyMinions | FriendlyFaces,
        Adjacent = 16,
        Self = 32,
        Others = 64,
        FriendlyPlayer = 128,
        EnemyPlayer = 256
    }

    public class Effect
    {
        protected Minion GuardAsMinion(ITarget<Character> target)
        {
            var minion = target as Minion;
            if (minion == null) throw new InvalidOperationException("Effect can only be applied on a Minion");
            return minion;
        }
    }

    public abstract class CharacterEffect : Effect
    {
        public abstract Character ApplyOn(ITarget<Character> target);

        public Character ApplyOn(Character target)
        {
            return ApplyOn((ITarget<Character>)target);
        }
    }

    public class HealEffect : CharacterEffect
    {
        private readonly int _amount;

        public HealEffect(int amount)
        {
            _amount = amount;
        }

        public override Character ApplyOn(ITarget<Character> target)
        {
            return target.RecieveHeal(_amount);
        }
    }

    public class DamageEffect : CharacterEffect
    {
        private readonly int _amount;

        public DamageEffect(int amount)
        {
            _amount = amount;
        }

        public override Character ApplyOn(ITarget<Character> target)
        {
            return target.RecieveDamage(_amount);
        }
    }

    public class StatusEffect : CharacterEffect
    {
        private readonly Status _status;

        public StatusEffect(Status status)
        {
            _status = status;
        }

        public override Character ApplyOn(ITarget<Character> target)
        {
            return target.AddStatus(_status);
        }
    }

    public class PermaBuffEffect : CharacterEffect
    {
        private readonly int _attack;
        private readonly int _health;

        public PermaBuffEffect(int attack, int health)
        {
            _attack = attack;
            _health = health;
        }

        public override Character ApplyOn(ITarget<Character> target)
        {
            var minion = GuardAsMinion(target);
            return minion.RecievePermaBuff(_attack, _health);
        }
    }

    public class SilenceEffect : CharacterEffect
    {
        public override Character ApplyOn(ITarget<Character> target)
        {
            var minion = GuardAsMinion(target);
            return minion.RecieveDispell();
        }
    }

    public class DestroyEffect : CharacterEffect
    {
        public override Character ApplyOn(ITarget<Character> target)
        {
            var minion = GuardAsMinion(target);
            return minion.RecieveDestroy();
        }
    }

    public class TransformEffect : CharacterEffect
    {
        private readonly MinionCard _transformInto;

        public TransformEffect(MinionCard transformInto)
        {
            _transformInto = transformInto;
        }

        public override Character ApplyOn(ITarget<Character> target)
        {
            return new Minion(_transformInto);
        }
    }

    public abstract class NonCharacterEffect : Effect
    {
        public abstract GameState ApplyOn(GameState gameState, Player player, Minion caster);
    }

    public class DrawEffect : NonCharacterEffect
    {
        private readonly int _amount;

        public DrawEffect(int amount)
        {
            _amount = amount;
        }

        public override GameState ApplyOn(GameState gameState, Player player, Minion unused)
        {
            return Enumerable.Range(0, _amount).Aggregate(gameState, (state, _) => state.DrawCard(player));
        }
    }

    public class SummonEffect : NonCharacterEffect
    {
        private readonly MinionCard _card;

        public SummonEffect(MinionCard card)
        {
            _card = card;
        }

        public override GameState ApplyOn(GameState gameState, Player player, Minion caster)
        {
            var playerGameState = gameState.Of(player);
            if (playerGameState.Minions.Count == GameEngine.MaxOnboardCount) return gameState;

            var summonAt = caster == null ? playerGameState.Minions.Count : playerGameState.Minions.IndexOf(caster) + 1;
            return gameState.With(playerGameState.With(minions: x => x.Insert(summonAt, new Minion(_card))));
        }
    }
}