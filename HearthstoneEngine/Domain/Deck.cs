namespace Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;

    public class Deck
    {
        private readonly IEnumerable<Card> _cards;

        public Deck(IEnumerable<Card> cards)
        {
            _cards = cards;
        }

        public Tuple<Deck, Card> Draw()
        {
            // TODO: fatigue
            var drawnCard = _cards.First();
            var updatedDeck = With(_cards.Except(drawnCard.Yield()));
            return new Tuple<Deck, Card>(updatedDeck, drawnCard);
        }

        public bool IsEmpty
        {
            get { return !_cards.Any(); }
        }

        public static Deck Empty
        {
            get { return new Deck(Enumerable.Empty<Card>()); }
        }

        public Deck With(IEnumerable<Card> cards = null)
        {
            return new Deck(cards ?? _cards);
        }
    }
}