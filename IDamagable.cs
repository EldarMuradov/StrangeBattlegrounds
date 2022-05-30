using Photon.Realtime;

public interface IDamagable 
{
    void TakeDamage(float damage, string killer);
}