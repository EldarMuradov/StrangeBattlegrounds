using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateManagerMultiplayer : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        for (int i = 0; i < MonoCacheMultiplayer.allUpdates.Count; i++)
            MonoCacheMultiplayer.allUpdates[i].Tick();
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < MonoCacheMultiplayer.allFixedUpdates.Count; i++)
            MonoCacheMultiplayer.allFixedUpdates[i].FixedTick();
    }
    private void LateUpdate()
    {
        for (int i = 0; i < MonoCacheMultiplayer.allLateUpdates.Count; i++)
            MonoCacheMultiplayer.allLateUpdates[i].LateTick();
    }
}
