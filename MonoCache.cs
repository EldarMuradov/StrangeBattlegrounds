using System.Collections.Generic;
using UnityEngine;

public class MonoCache : MonoBehaviour
{
    public static List<MonoCache> allUpdates = new List<MonoCache>(10001);
    public static List<MonoCache> allFixedUpdates = new List<MonoCache>(10001);
    public static List<MonoCache> allLateUpdates = new List<MonoCache>(10001);

    private void OnEnable() 
    { 
        allUpdates.Add(this);
        allFixedUpdates.Add(this);
        allLateUpdates.Add(this);
    }
    private void OnDisable() 
    {
        allUpdates.Remove(this);
        allFixedUpdates.Remove(this);
        allLateUpdates.Remove(this);
    }
    private void OnDestroy() 
    { 
        allUpdates.Remove(this);
        allFixedUpdates.Remove(this);
        allLateUpdates.Remove(this);
    }
    public void Tick() => OnTick();
    public virtual void OnTick() { }
    public void FixedTick() => OnFixedTick();
    public virtual void OnFixedTick() { }
    public void LateTick() => OnLateTick();
    public virtual void OnLateTick() { }

}
