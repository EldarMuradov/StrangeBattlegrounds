using System.Collections;
using UnityEngine;

public abstract class GunParent : MonoCache
{
    public SafeFloat Damage;
    public float bulletsIHave;
    public bool Reloading;
    public bool Shooting = false;
    public abstract void Use();
    public abstract IEnumerator HideGun();
    public abstract IEnumerator DrawGun();
    public abstract void Reload();
    public abstract void Throw();
    public abstract void Put();
    public GameObject BulletImpactPrefab;
}
