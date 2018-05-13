namespace Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public partial class PlayerGameState
    {
        public PlayerGameState(Player owner, Face face, ImmutableList<Minion> minions, int remainingMana, Hand hand, Deck deck)
        {
            Owner = owner;
            Face = face;
            Minions = minions;
            RemainingMana = remainingMana;
            Hand = hand;
            Deck = deck;
        }

        public Player Owner { get; private set; }
        public Face Face { get; private set; }
        public ImmutableList<Minion> Minions { get; private set; }
        public int RemainingMana { get; private set; }
        public Hand Hand { get; private set; }
        public Deck Deck { get; private set; }

        public IEnumerable<ITarget<Character>> AllTargets
        {
            get
            {
                foreach (var @char in Minions) yield return @char;
                yield return Face;
            }
        }
    }

    public partial class PlayerGameState
    {
        public static PlayerGameState Empty
        {
            get { return new PlayerGameState(Player.Empty, Face.Empty, ImmutableList<Minion>.Empty, 0, Hand.Empty, Deck.Empty); }
        }

        public PlayerGameState With(Player owner = null, Func<Face, Face> face = null, Func<ImmutableList<Minion>, ImmutableList<Minion>> minions = null,
            Func<int, int> remainingMana = null, Func<Hand, Hand> hand = null, Func<Deck, Deck> deck = null)
        {
            return new PlayerGameState(owner ?? Owner, (face ?? (x => x))(Face),
                (minions ?? (x => x))(Minions),
                (remainingMana ?? (x => x))(RemainingMana),
                (hand ?? (x => x))(Hand),
                (deck ?? (x => x))(Deck));
        }
    }
}