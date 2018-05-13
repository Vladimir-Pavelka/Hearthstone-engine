namespace Domain
{
    using System;

    public sealed partial class Face : Character, ITarget<Face>
    {
        private readonly int _attack;
        private readonly int _health;

        public Face(int attack, int health, Status status)
        {
            _attack = attack;
            _health = health;
            Status = status;
        }

        public override int Attack
        {
            get { return _attack; }
        }

        public override int Health
        {
            get { return _health; }
        }

        public Status Status { get; private set; }

        public override bool IsDead
        {
            get { return Health <= 0; }
        }

        public override bool IsDamaged
        {
            get { throw new NotImplementedException(); }
        }

        public Face RecieveDamage(int amount)
        {
            return With(health: Health - amount);
        }

        public Face RecieveHeal(int amount)
        {
            return With(health: Health + amount);
        }

        public Face AddStatus(Status status)
        {
            return With(status: Status | status);
        }

        public bool HasStatus(Status status)
        {
            return Status.HasFlag(status);
        }
    }

    public partial class Face
    {
        public static Face Empty
        {
            get { return new Face(0, 30, Status.None); }
        }

        public Face With(int? attack = null, int? health = null, Status? status = null)
        {
            return new Face(attack ?? Attack, health ?? Health, status ?? Status);
        }
    }
}
