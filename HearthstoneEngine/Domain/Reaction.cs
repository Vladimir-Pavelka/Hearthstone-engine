namespace Domain
{
    using System.Collections.Generic;

    public class Reaction
    {
        public Minion ReactingMinion { get; private set; }
        public IEnumerable<Targetless> ReactionEffects { get; private set; }

        public Reaction(Minion reactingMinion, IEnumerable<Targetless> reactionEffects)
        {
            ReactingMinion = reactingMinion;
            ReactionEffects = reactionEffects;
        }
    }
}