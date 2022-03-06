using System.Collections;
using TMPro;
using UnityEngine;

public class SniperMethod : GunParent
{
    public bool isScoped = false;
    public GameObject ScopeOverlay;
    public GameObject WeaponCamera;
    public Camera MainCamera;
    public GameObject img;

    private float normalFOV;
    public float ScopedFOV = 15f;

    [SerializeField] private Camera cam;
    [SerializeField] private Animator animator;
    public TMP_Text text;
    public SolderPlayerController controller;
    [Header("Bullet properties")]
    [Tooltip("Preset value to tell with how much bullets will our waepon spawn inside rifle.")]
    public float bulletsInTheGun = 10;
    [Tooltip("Preset value to tell how much bullets can one magazine carry.")]
    [SerializeField] private float amountOfBulletsPerLoad = 5;
    [SerializeField] private bool reloading = false;
    [SerializeField] private float reloadChangeBulletsTime;
    [Tooltip("Array of muzzel flashes, randmly one will appear after each bullet.")]
    
    public AudioClip ReloadClip;
    public AudioClip FireClip;
    public AudioClip NoAmmoClip;
    public GameObject CamHolder;
    private float recoilX = 0.2f;
    private float recoilY = 0.2f;
    public GameObject gun;
    [SerializeField] private GameObject knife;
    public GameObject Shell;
    public GameObject ShellSpawner;
    private bool shooting = false;
    [SerializeField] private GameObject BBullet;
    private AudioSource _source;
    private void Awake()
    {
        _source = controller.GetComponent<AudioSource>();
        Damage = 10f;
    }
    //public override void OnTick()4
    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && gun.activeSelf && !knife.activeSelf)
        {
            isScoped = !isScoped;
            animator.SetBool("Scoped", isScoped);
            animator.SetBool("Hide", false);
            animator.SetBool("Idle", false);
            animator.SetBool("Shoot", false);
            animator.SetBool("Draw", false);
            animator.SetBool("Reload", false);

            if (isScoped)
            StartCoroutine(OnScoped());
            else
            StartCoroutine(OnUnScoped());
        }

        if (Input.GetMouseButtonDown(0) && gun.activeSelf && !knife.activeSelf && !shooting)
        Use();

        if (Input.GetKey(KeyCode.R) && gun.activeSelf && !knife.activeSelf && !shooting)
        StartCoroutine(ReloadCur());

        if (Input.GetKeyDown(KeyCode.Alpha2) && gun.activeSelf && !knife.activeSelf && !shooting)
        StartCoroutine(HideGun());
        else if (Input.GetKeyDown(KeyCode.Alpha2) && !gun.activeSelf && !knife.activeSelf && !shooting)
        StartCoroutine(DrawGun());
    }
    public override IEnumerator HideGun()
    {
        animator.SetBool("Hide", true);
        animator.SetBool("Idle", false);
        animator.SetBool("Shoot", false);
        animator.SetBool("Draw", false);
        animator.SetBool("Reload", false);
        animator.SetBool("Scoped", false);
        yield return new WaitForSeconds(0.4f);
        gun.SetActive(false);
        animator.SetBool("Hide", false);
        animator.SetBool("Idle", true);
        animator.SetBool("Shoot", false);
        animator.SetBool("Draw", false);
        animator.SetBool("Reload", false);
        animator.SetBool("Scoped", false);
    }
    public override IEnumerator DrawGun()
    {
        SetText();
        gun.SetActive(true);
        animator.SetBool("Hide", false);
        animator.SetBool("Idle", false);
        animator.SetBool("Shoot", false);
        animator.SetBool("Draw", true);
        animator.SetBool("Scoped", false);
        animator.SetBool("Reload", false);
        shooting = true;
        yield return new WaitForSeconds(0.2f);
        shooting = false;
        animator.SetBool("Hide", false);
        animator.SetBool("Idle", true);
        animator.SetBool("Shoot", false);
        animator.SetBool("Draw", false);
        animator.SetBool("Reload", false);
        animator.SetBool("Scoped", false);
    }

    public override void Use()
    {
        if (bulletsInTheGun > 0 && !reloading)
            Shoot();

        else
            _source.PlayOneShot(NoAmmoClip);
    }
    public override void Reload()
    {
        StartCoroutine(ReloadCur());
        SetText();
    }

    public void SetText()
    {
        text.text = (bulletsInTheGun).ToString() + "/" + (bulletsIHave).ToString();
    }

    private void Shoot()
    {
        SetText();
        StartCoroutine(ShootAnimCur());
        CamHolder.transform.Rotate(new Vector3(Random.Range(-recoilX, recoilX), Random.Range(0, recoilY), 0));
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        GameObject bulletShelltObj = Instantiate(Shell, ShellSpawner.transform.position, ShellSpawner.transform.rotation);
      
        Destroy(bulletShelltObj, 10f);
        ray.origin = cam.transform.position;
        
        Debug.Log("shoot");
        _source.PlayOneShot(FireClip);
        bulletsInTheGun -= 1;
        SetText();
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //TODO: Some Effects
            hit.collider.gameObject.GetComponent<IDamagableSolo>()?.TakeDamage(Damage);
            Impact_Shoot(hit.point, hit.normal, hit.collider.tag);
        }

    }

    private void Impact_Shoot(Vector3 hitPosition, Vector3 hitNormal, string tag)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            if (tag == "Enemy" || tag == "BossPunch")
            {
                GameObject bulletImpactObjB = Instantiate(BBullet, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * BBullet.transform.rotation);
                bulletImpactObjB.transform.SetParent(colliders[0].transform);
            }
            else
            {
                GameObject bulletImpactObj = Instantiate(BulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * BulletImpactPrefab.transform.rotation);
                bulletImpactObj.transform.SetParent(colliders[0].transform);
            }

        }
    }
    private IEnumerator ShootAnimCur()
    {
        animator.SetBool("Hide", false);
        animator.SetBool("Idle", true);
        animator.SetBool("Scoped", false);
        animator.SetBool("Shoot", false);
        animator.SetBool("Draw", false);
        animator.SetBool("Reload", false);
        shooting = true;
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("Hide", false);
        animator.SetBool("Idle", false);
        animator.SetBool("Shoot", true);
        animator.SetBool("Draw", false);
        animator.SetBool("Scoped", false);
        animator.SetBool("Reload", false);
        yield return new WaitForSeconds(1.4f);
        animator.SetBool("Hide", false);
        animator.SetBool("Idle", true);
        animator.SetBool("Scoped", false);
        animator.SetBool("Shoot", false);
        animator.SetBool("Draw", false);
        animator.SetBool("Reload", false);
        shooting = false;
    }
    private IEnumerator ReloadCur()
    {
        if (bulletsIHave > 0 && bulletsInTheGun < amountOfBulletsPerLoad && !reloading)
        {
            reloading = true;
            _source.PlayOneShot(ReloadClip);
            animator.SetBool("Hide", false);
            animator.SetBool("Idle", false);
            animator.SetBool("Shoot", false);
            animator.SetBool("Scoped", false);
            animator.SetBool("Draw", false);
            animator.SetBool("Reload", true);
            yield return new WaitForSeconds(1.6f);
            animator.SetBool("Hide", false);
            animator.SetBool("Idle", true);
            animator.SetBool("Shoot", false);
            animator.SetBool("Scoped", false);
            animator.SetBool("Draw", false);
            animator.SetBool("Reload", false);
            if (bulletsIHave - amountOfBulletsPerLoad >= 0)
            {
                bulletsIHave -= amountOfBulletsPerLoad - bulletsInTheGun;
                bulletsInTheGun = amountOfBulletsPerLoad;
            }
            else if (bulletsIHave - amountOfBulletsPerLoad < 0)
            {
                float valueForBoth = amountOfBulletsPerLoad - bulletsInTheGun;
                if (bulletsIHave - valueForBoth < 0)
                {
                    bulletsInTheGun += bulletsIHave;
                    bulletsIHave = 0;
                }
                else
                {
                    bulletsIHave -= valueForBoth;
                    bulletsInTheGun += valueForBoth;
                }
            }
            else if (bulletsIHave <= 0)
            _source.PlayOneShot(NoAmmoClip);
            SetText();
            reloading = false;
        }
    }

    public override void Put()
    {

    }

    public override void Throw()
    {

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
