namespace DemHoiDenLong.Gameplay
{
    /// <summary>
    /// IDamageable interface defines the contract for any entity
    /// that can receive damage and track health status (Player, Enemies, Bosses).
    /// </summary>
    public interface IDamageable
    {
        float CurrentHp { get; }
        float MaxHp { get; }
        bool IsDead { get; }

        void TakeDamage(float amount);
        void Heal(float amount);
    }
}
