using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class MonoCacheMultiplayer : MonoBehaviourPunCallbacks
{
    public static List<MonoCacheMultiplayer> allUpdates = new List<MonoCacheMultiplayer>(10001);
    public static List<MonoCacheMultiplayer> allFixedUpdates = new List<MonoCacheMultiplayer>(10001);
    public static List<MonoCacheMultiplayer> allLateUpdates = new List<MonoCacheMultiplayer>(10001);

    public override void OnEnable()
    {
        allUpdates.Add(this);
        allFixedUpdates.Add(this);
        allLateUpdates.Add(this);
    }
    public override void OnDisable()
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
