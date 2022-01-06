public interface IDamageable
{
    void ChangeHealth(float amount, ulong healthChangerNetworkId, int killWeapon = 0);
    void ChangeHealth(float amount, bool isToSet = false);
}
