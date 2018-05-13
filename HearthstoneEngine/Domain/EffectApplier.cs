namespace Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;

    public abstract class EffectApplier
    {
        private readonly Func<Character, bool> _isTargetValid;

        protected EffectApplier(IEnumerable<Effect> effects, Target target, Func<Character, bool> isTargetValid)
        {
            _isTargetValid = isTargetValid;
            Effects = effects;
            Target = target;
        }

        public IEnumerable<Effect> Effects { get; private set; }
        public Target Target { get; private set; }

        public IEnumerable<Character> GetValidTargets(GameState gameState, Player player)
        {
            var returnValue = new List<Character>();
            if (Target.HasFlag(Target.FriendlyMinions)) returnValue.AddRange(gameState.Of(player).Minions);
            if (Target.HasFlag(Target.FriendlyFaces)) returnValue.Add(gameState.Of(player).Face);
            if (Target.HasFlag(Target.EnemyMinions)) returnValue.AddRange(gameState.OfOpponent(player).Minions);
            if (Target.HasFlag(Target.EnemyFaces)) returnValue.Add(gameState.OfOpponent(player).Face);

            return returnValue.Where(x => _isTargetValid(x)).Where(x => !x.IsDead);
        }
    }

    public class Targetable : EffectApplier
    {
        private readonly IEnumerable<CharacterEffect> _effects;


        public Targetable(CharacterEffect effect, Target target = Target.All, Func<Character, bool> isTargetValid = null)
            : this(new[] { effect }, target, isTargetValid)
        {
        }

        public Targetable(CharacterEffect[] effects, Target target = Target.All, Func<Character, bool> isTargetValid = null)
            : base(effects, target, isTargetValid ?? (x => true))
        {
            _effects = effects;
        }

        public GameState ApplyOn(GameState gameState, Player player, ITarget<Character> target)
        {
            var updatedTarget = _effects.Aggregate((Character)target, (t, effect) => effect.ApplyOn(t));
            return gameState.UpdateCharacter((Character)target, updatedTarget);
        }


        public GameState ApplyOn(GameState gameState, Player player, Character target)
        {
            return ApplyOn(gameState, player, (ITarget<Character>)target);
        }
    }

    public class Targetless : EffectApplier
    {
        public Targetless(Effect effect, Target target)
            : this(new[] { effect }, target)
        {
        }

        public Targetless(IEnumerable<Effect> effects, Target target)
            : base(effects, target, x => true)
        {
        }

        public GameState ApplyOn(GameState gameState, Player owner, Minion caster)
        {
            return Effects.Aggregate(gameState, (aggregState, eff) =>
                TypeSwitchExpr.On<Effect, GameState>(eff)
                    .Case<CharacterEffect>(effect =>
                    {
                        var targets = GetValidTargets(aggregState, owner, caster).ToArray();
                        var updatedChars = targets.Select(x => effect.ApplyOn((ITarget<Character>)x));
                        return aggregState.UpdateCharacters(targets, updatedChars);
                    })
                    .Case<NonCharacterEffect>(effect =>
                    {
                        if (Target.HasFlag(Target.FriendlyPlayer)) aggregState = effect.ApplyOn(aggregState, owner, caster);
                        if (Target.HasFlag(Target.EnemyPlayer)) aggregState = effect.ApplyOn(aggregState, aggregState.OpponentOf(owner), caster);
                        return aggregState;
                    })
                    .ElseThrow());
        }

        public IEnumerable<Character> GetValidTargets(GameState gameState, Player player, Minion caster)
        {
            if (Target.HasFlag(Target.Adjacent)) return GetAdjacentMinions(gameState.Of(player).Minions, caster);

            return Target.HasFlag(Target.Self) ?
                GetValidTargets(gameState, player).Concat(caster.Yield()).Distinct() : GetValidTargets(gameState, player).Except(caster.Yield());
        }

        private static IEnumerable<Character> GetAdjacentMinions(IEnumerable<Minion> minions, Minion caster)
        {
            IList<Minion> allMinions = minions as List<Minion> ?? minions.ToList();
            var indexOfCaster = allMinions.IndexOf(caster);

            if (caster != allMinions.First()) yield return allMinions[indexOfCaster - 1];
            if (caster != allMinions.Last()) yield return allMinions[indexOfCaster + 1];
        }
    }
}