namespace Domain
{
    public abstract class Character
    {
        public abstract int Health { get; }
        public abstract int Attack { get; }
        public abstract bool IsDead { get; }
        public abstract bool IsDamaged { get; }
    }
}