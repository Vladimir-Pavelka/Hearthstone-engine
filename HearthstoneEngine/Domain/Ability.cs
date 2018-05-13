namespace Domain
{
    public class Ability
    {
    }

    public class TriggeredAbility : Ability
    {
        public Event Trigger { get; private set; }
        public Targetless Effect { get; private set; }

        public TriggeredAbility(Event trigger, Targetless effect)
        {
            Trigger = trigger;
            Effect = effect;
        }

        public bool IsTriggeredBy(Event @event)
        {
            return Trigger.Type == @event.Type && Trigger.Target.HasFlag(@event.Target);
        }
    }

    public class Event
    {
        public EventType Type { get; private set; }
        public Target Target { get; private set; }

        public Event(EventType type, Target target)
        {
            Type = type;
            Target = target;
        }
    }

    public enum EventType
    {
        None,
        DamageRecieved,
        SpellCasted,
        HealRecieved
    }
}