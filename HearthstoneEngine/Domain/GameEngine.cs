namespace Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;

    public class GameEngine
    {
        public const int MaxOnboardCount = 7;

        public IEnumerable<GameAction> GetAllPossibleActions(GameState gameState, Player player)
        {
            var attackActions = GetAllAttackActions(gameState, player);
            var summonActions = GetAllSummonActions(gameState, player);
            var spellCastActions = GetAllSpellCastActions(gameState, player);

            return attackActions.Concat(summonActions).Concat(spellCastActions);
        }

        public IEnumerable<GameAction> GetAllAttackActions(GameState gameState, Player player)
        {
            var validPlayerAttackers = gameState.Of(player).Minions.Where(x => x.CanAttack);
            var opponentChars = gameState.OfOpponent(player).AllTargets;
            var validOpponentTargets = GetValidAttackTargets(opponentChars);

            return validPlayerAttackers.SelectMany(attacker => validOpponentTargets.Select(target => new AttackAction(player, attacker, target)));
        }

        private static IEnumerable<ITarget<Character>> GetValidAttackTargets(IEnumerable<ITarget<Character>> candidates)
        {
            var nonStealthChars = candidates.Where(x => !x.HasStatus(Status.Stealth)).ToArray();
            var nonStealthTauntChars = nonStealthChars.Where(x => x.HasStatus(Status.Taunt)).ToArray();
            return nonStealthTauntChars.Any() ? nonStealthTauntChars : nonStealthChars;
        }

        public IEnumerable<GameAction> GetAllSummonActions(GameState gameState, Player player)
        {
            var charsCount = gameState.Of(player).Minions.Count;
            if (charsCount == MaxOnboardCount)
                return Enumerable.Empty<SummonAction>();

            var possibleBoardPositions = Enumerable.Range(0, charsCount + 1);
            var playableMinionCards = GetManaPlayableCards(gameState, player).OfType<MinionCard>().ToArray();

            var playableAsTargetable = playableMinionCards
                .OfType<BattlecryMinionCard>()
                .Where(card => ValidBattlecryTargets(card, gameState, player).Any()).ToArray();

            var targetableSummonActions =
                playableAsTargetable.SelectMany(
                    card =>
                        ValidBattlecryTargets(card, gameState, player)
                            .CartesianProduct(possibleBoardPositions,
                                (target, position) => new SummonBattlecryTargetable(player, card, target, position)));

            var otherSummonActions =
                playableMinionCards.Except(playableAsTargetable)
                    .SelectMany(card => possibleBoardPositions.Select(position => new SummonAction(player, card, position)));

            return targetableSummonActions.Concat(otherSummonActions);
        }

        private static IEnumerable<Character> ValidBattlecryTargets(BattlecryMinionCard card, GameState gameState, Player player)
        {
            var applier = card.EffectApplier as Targetable;
            return applier == null ?
                Enumerable.Empty<Character>() : applier.GetValidTargets(gameState, player);
        }

        private static IEnumerable<Character> ValidSpellTargets(SpellCard card, GameState gameState, Player player)
        {
            var applier = card.EffectAppliers.OfType<Targetable>().FirstOrDefault();
            return applier == null ?
                Enumerable.Empty<Character>() : applier.GetValidTargets(gameState, player);
        }

        private static IEnumerable<Card> GetManaPlayableCards(GameState gameState, Player player)
        {
            return gameState.Of(player).Hand.Where(card => card.ManaCost <= gameState.Of(player).RemainingMana);
        }

        public IEnumerable<GameAction> GetAllSpellCastActions(GameState gameState, Player player)
        {
            var playableSpellCards = GetManaPlayableCards(gameState, player).OfType<SpellCard>().ToArray();
            var targetlessActions = playableSpellCards.Where(spellCard => !spellCard.IsTargetable).Select(spell => new CastSpellTargetless(player, spell));
            var targetableActions = playableSpellCards.Where(spellCard => spellCard.IsTargetable)
                    .SelectMany(spellCard => ValidSpellTargets(spellCard, gameState, player).Select(target => new CastSpellTargetable(player, spellCard, target)));

            return targetlessActions.Concat((IEnumerable<CastSpell>)targetableActions);
        }

        public GameState ApplyAction(GameState gameState, GameAction gameAction)
        {
            var newGameState = TypeSwitchExpr.On<GameAction, GameState>(gameAction)
                .Case<SummonBattlecryTargetable>(action =>
                {
                    var withMinionSummoned = SummonMinnion(gameState, action);
                    return action.ApplyEffect(withMinionSummoned, action.Owner);
                })
                .Case<SummonBattlecryTargetless>(action =>
                {
                    var minionToSummon = new Minion(action.Card);
                    var withMinionSummoned = SummonMinnion(gameState, action, minionToSummon);
                    return action.ApplyBattlecry(withMinionSummoned, action.Owner, minionToSummon);
                })
                .Case<SummonAction>(action => SummonMinnion(gameState, action))
                .Case<CastSpell>(action =>
                {
                    var handManaUpdated = CardPlayUpdateHandMana(gameState, action.Card, action.Owner);
                    var withSpellApplied = action.ApplyEffect(handManaUpdated, action.Owner);
                    return withSpellApplied.PlayerEventOccured(new Event(EventType.SpellCasted, Target.FriendlyPlayer), action.Owner);
                })
                .Case<AttackAction>(action =>
                    TypeSwitchExpr.On<ITarget<Character>, GameState>(action.Target)
                    .Case<Minion>(target =>
                    {
                        var updatedAttacker = action.Attacker.RecieveDamage(target.Attack);
                        var updatedTarget = target.RecieveDamage(action.Attacker.Attack);
                        var withAttackerUpdated = gameState.UpdateCharacter(action.Attacker, updatedAttacker);
                        var withBothUpdated = withAttackerUpdated.UpdateCharacter(target, updatedTarget);
                        return withBothUpdated;
                    })
                    .Case<Face>(target =>
                        gameState.With(gameState.OfOpponent(action.Owner).With(face: x => x.RecieveDamage(action.Attacker.Attack))))
                    .ElseThrow())
                .ElseThrow();

            newGameState = ProcessChainReactions(newGameState);

            return newGameState.RemoveDeadBodies();
        }

        private static GameState ProcessChainReactions(GameState gameState)
        {
            gameState = gameState.NotifyAllAboutRaisedMinionEvents();

            while (gameState.Reactions.Any())
            {
                gameState = gameState.Reactions.Aggregate(gameState, (state, reaction) =>
                {
                    var owner = state.GetOwnerOf(reaction.ReactingMinion);
                    var reactingMinionCleared = reaction.ReactingMinion.ClearReactionEffects();
                    var withMinionReactionsCleared = state.UpdateCharacter(reaction.ReactingMinion, reactingMinionCleared);

                    return reaction.ReactionEffects.Aggregate(withMinionReactionsCleared,
                        (state2, reactionEffect) => reactionEffect.ApplyOn(state2, owner, reactingMinionCleared));
                });

                gameState = gameState.NotifyAllAboutRaisedMinionEvents();
            }

            return gameState;
        }

        private static GameState SummonMinnion(GameState gameState, SummonAction action, Minion minion = null)
        {
            var handManaUpdatedState = CardPlayUpdateHandMana(gameState, action.Card, action.Owner);
            return gameState.With(handManaUpdatedState.Of(action.Owner)
                        .With(minions: x => x.Insert(action.DesiredBoardPosition, minion ?? new Minion(action.Card))));
        }

        private static GameState CardPlayUpdateHandMana(GameState gameState, Card card, Player owner)
        {
            var currentPlayerState = gameState.Of(owner);
            var newPlayerState = currentPlayerState.With(
                hand: x => x.RemoveCard(card),
                remainingMana: x => x - card.ManaCost);

            return gameState.With(newPlayerState);
        }
    }
}