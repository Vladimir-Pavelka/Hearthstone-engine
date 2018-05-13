namespace Domain
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class Hand : IEnumerable<Card>
    {
        private const int CardsMaxCount = 10;

        private readonly ImmutableList<Card> _cards;

        public Hand(IEnumerable<Card> cards)
        {
            _cards = ImmutableList.Create(cards.ToArray());
        }

        public Hand AddCard(Card card)
        {
            return _cards.Count < CardsMaxCount ? With(_cards.Add(card)) : this;
        }

        public Hand RemoveCard(Card card)
        {
            return new Hand(_cards.Except(new[] { card }));
        }

        public Card this[int i]
        {
            get { return _cards[i]; }
        }

        public IEnumerator<Card> GetEnumerator()
        {
            return _cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cards.GetEnumerator();
        }

        public static Hand Empty
        {
            get { return new Hand(Enumerable.Empty<Card>()); }
        }

        public Hand With(IEnumerable<Card> cards)
        {
            return new Hand(cards);
        }
    }
}