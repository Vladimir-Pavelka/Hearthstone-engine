namespace Domain
{
    public interface ITarget<out T>
    {
        bool HasStatus(Status status);
        T RecieveDamage(int amount);
        T RecieveHeal(int amount);
        T AddStatus(Status status);
    }
}