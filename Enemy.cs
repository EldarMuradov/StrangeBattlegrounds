using UnityEngine;

public abstract class Enemy : MonoCache
{
    public float Damage;
    public override string ToString() => "Enemy";
    public float MaxHealth;
    public virtual void Create(Level level) {}
    public void OnEnable()
    {
        GameFactory.Enemies.Add(this);
        foreach (Enemy enemy in GameFactory.Enemies)
        {
            enemy.Create(GameFactory.level);
        }
    }

    public void OnDisable()
    {
        GameFactory.Enemies.Remove(this);
    }
}
public enum Level
{
    Easy,
    Medium,
    Hard,
    Impossible
}