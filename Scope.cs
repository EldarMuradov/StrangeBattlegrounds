using System.Collections;
using UnityEngine;

public class Scope : MonoCacheMultiplayer
{
    public Animator animator;
    public bool isScoped = false;
    public GameObject ScopeOverlay;
    public GameObject WeaponCamera;
    public Camera MainCamera;
    public GameObject img;

    private float normalFOV;
    public float ScopedFOV = 15f;


    public void Update()
    //public override void OnTick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isScoped = !isScoped;
            animator.SetBool("IsScoped", isScoped);

            if (isScoped)
            {
                StartCoroutine(OnScoped());
            }
            else 
            {
              StartCoroutine(OnUnScoped());
            }
        }
    }

    private IEnumerator OnScoped() 
    {
        yield return new WaitForSeconds(0.15f);
        ScopeOverlay.SetActive(true);
        WeaponCamera.SetActive(false);
        img.SetActive(false);

        normalFOV = MainCamera.fieldOfView;
        MainCamera.fieldOfView = ScopedFOV;
 
    }


    private IEnumerator OnUnScoped()
    {
        yield return new WaitForSeconds(0.15f);
        ScopeOverlay.SetActive(false);
        WeaponCamera.SetActive(true);
        img.SetActive(true);

        MainCamera.fieldOfView = normalFOV;
    }
}
