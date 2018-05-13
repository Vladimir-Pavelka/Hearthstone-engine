namespace Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;

    public partial class GameState
    {
        private readonly IEnumerable<PlayerGameState> _playerGameStates;

        public GameState(params PlayerGameState[] playerGameStates)
        {
            if (playerGameStates.Length > 2) throw new InvalidOperationException("Max 2 players in this game");

            _playerGameStates = playerGameStates;
        }

        public Player OpponentOf(Player player)
        {
            var opponentGameState = _playerGameStates.FirstOrDefault(x => x.Owner != player);
            if (opponentGameState == null)
                throw new InvalidOperationException("This player was not found in this game");

            return opponentGameState.Owner;
        }

        public PlayerGameState Of(Player player)
        {
            var returnValue = _playerGameStates.FirstOrDefault(x => x.Owner == player);
            if (returnValue == null)
                throw new InvalidOperationException("This player was not found in this game");

            return returnValue;
        }

        public PlayerGameState OfOpponent(Player player)
        {
            var returnValue = _playerGameStates.FirstOrDefault(x => x.Owner != player);
            if (returnValue == null)
                throw new InvalidOperationException("This player was not found in this game");

            return returnValue;
        }

        public Player GetOwnerOf(Minion minion)
        {
            var returnValue = _playerGameStates.Where(x => x.Minions.Contains(minion)).Select(x => x.Owner).FirstOrDefault();

            if (returnValue == null)
                throw new InvalidOperationException("Owner of this minion was not found in this game");

            return returnValue;
        }

        public GameState UpdateCharacter(Character oldChar, Character updatedChar)
        {
            return
                TypeSwitchExpr.On<Character, GameState>(oldChar)
                    .Case<Face>(oldFace =>
                    {
                        var updatedFace = (Face)updatedChar;
                        var updatedPlayerState = _playerGameStates.First(x => x.Face == oldFace).With(face: x => updatedFace);

                        return With(updatedPlayerState);
                    })
                    .Case<Minion>(oldMinion =>
                    {
                        var updatedMinion = (Minion)updatedChar;
                        var updatedPlayerState =
                            _playerGameStates.First(x => x.Minions.Contains(oldMinion)).With(minions: x => x.Replace(oldMinion, updatedMinion));

                        return With(updatedPlayerState);
                    })
                    .ElseThrow();
        }


        public GameState UpdateCharacters(IEnumerable<Character> oldChars, IEnumerable<Character> newChars)
        {
            return oldChars.Zip(newChars, (oldChar, newChar) => new { OldChar = oldChar, NewChar = newChar })
                .Where(couple => couple.OldChar != couple.NewChar)
                .Aggregate(this,
                    (gameState, couple) => gameState.UpdateCharacter(couple.OldChar, couple.NewChar));
        }

        public GameState RemoveDeadBodies()
        {
            return With(RemoveDeadBodies(_playerGameStates.First()), RemoveDeadBodies(_playerGameStates.Last()));
        }

        private static PlayerGameState RemoveDeadBodies(PlayerGameState playerGameState)
        {
            return playerGameState.Minions.Aggregate(playerGameState,
                (state, minion) => minion.IsDead ? state.With(minions: x => x.Remove(minion)) : state);
        }

        public GameState DrawCard(Player player)
        {
            var playerGameState = Of(player);
            var tuple = playerGameState.Deck.Draw();
            var updatedDeck = tuple.Item1;
            var drawnCard = tuple.Item2;

            return With(playerGameState.With(hand: x => x.AddCard(drawnCard), deck: x => updatedDeck));
        }

        public GameState PlayerEventOccured(Event @event, Player triggeringPlayer)
        {
            if (@event.Target == Target.FriendlyPlayer)
            {
                var oldFriendlyMinions = Of(triggeringPlayer).Minions.Where(x => !x.IsDead).ToArray();
                var notifiedFriendlyMinions = oldFriendlyMinions.Select(x => x.EventOccured(@event));
                var oldEnemyMinions = OfOpponent(triggeringPlayer).Minions.Where(x => !x.IsDead).ToArray();
                var notifiedEnemyMinions = oldEnemyMinions.Select(x => x.EventOccured(new Event(@event.Type, Target.EnemyPlayer)));

                return UpdateCharacters(oldFriendlyMinions, notifiedFriendlyMinions)
                    .UpdateCharacters(oldEnemyMinions, notifiedEnemyMinions);
            }

            // TODO: EnemyPlayer ? (reverse logic)

            var oldMinions = _playerGameStates.SelectMany(x => x.Minions).ToArray();
            var notifiedMinions = oldMinions.Select(x => x.EventOccured(@event));

            return UpdateCharacters(oldMinions, notifiedMinions);
        }

        public GameState MinionEventOccured(Event @event, Minion triggeringMinion)
        {
            var oldFriendlyMinions = Of(GetOwnerOf(triggeringMinion)).Minions.Where(x => !x.IsDead).Where(x => x != triggeringMinion).ToArray();
            var notifiedFriendlyMinions = oldFriendlyMinions.Select(x => x.EventOccured(@event));
            var oldEnemyMinions = OfOpponent(GetOwnerOf(triggeringMinion)).Minions.Where(x => !x.IsDead).ToArray();
            var notifiedEnemyMinions = oldEnemyMinions.Select(x => x.EventOccured(new Event(@event.Type, Target.EnemyMinions)));

            return UpdateCharacters(oldFriendlyMinions, notifiedFriendlyMinions)
                .UpdateCharacters(oldEnemyMinions, notifiedEnemyMinions);
        }

        public IEnumerable<Reaction> Reactions
        {
            get
            {
                return _playerGameStates.SelectMany(x => x.Minions).Where(minion => minion.ReactionEffects.Any())
                    .Select(minion => new Reaction(minion, minion.ReactionEffects));
            }
        }

        public GameState NotifyAllAboutRaisedMinionEvents()
        {
            return _playerGameStates.SelectMany(x => x.Minions)
                .Where(minion => minion.RaisedEvents.Any())
                .Aggregate(this, NotifyAllAboutMinionEvents);
        }

        private static GameState NotifyAllAboutMinionEvents(GameState gameState, Minion triggeringMinion)
        {
            gameState = triggeringMinion.RaisedEvents.Aggregate(gameState,
                (state, raisedEvent) =>
                    state.MinionEventOccured(new Event(raisedEvent, Target.FriendlyMinions), triggeringMinion));

            return gameState.UpdateCharacter(triggeringMinion, triggeringMinion.ClearRaisedEvents());
        }
    }

    public partial class GameState
    {
        public static GameState Empty
        {
            get { return new GameState(PlayerGameState.Empty, PlayerGameState.Empty); }
        }

        public GameState With(params PlayerGameState[] playerGameStates)
        {
            var isOnlyOneNew = playerGameStates.Length == 1;
            var wasEmptyGameState = _playerGameStates.All(x => x.Owner != playerGameStates.First().Owner);

            if (wasEmptyGameState)
                return isOnlyOneNew
                    ? new GameState(playerGameStates[0], PlayerGameState.Empty)
                    : new GameState(playerGameStates);


            return isOnlyOneNew
                ? new GameState(_playerGameStates.Where(x => x.Owner != playerGameStates.First().Owner)
                    .Concat(playerGameStates)
                    .ToArray())
                : new GameState(playerGameStates);
        }
    }
}