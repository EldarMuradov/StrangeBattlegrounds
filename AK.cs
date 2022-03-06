using Photon.Pun;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public class AK : Gun
{
    #region Variables

    [SerializeField] private PlayerController pc;
    [SerializeField] private Camera cam;
    [SerializeField] private Animator animator;
    public TMP_Text text;
    [Header("Bullet properties")]
    [Tooltip("Preset value to tell with how many bullets will our waepon spawn aside.")]
    public float bulletsInTheGun = 30;
    [Tooltip("Preset value to tell how much bullets can one magazine carry.")]
    [SerializeField] private float amountOfBulletsPerLoad = 30;
    public bool reloading = false;
    [SerializeField] private float reloadChangeBulletsTime;
    [Tooltip("Array of muzzel flashes, randmly one will appear after each bullet.")]
    public AudioClip ReloadClip;
    public AudioClip FireClip;
    public AudioClip ReloadDrawClip;
    public AudioClip NoAmmoClip;
    private float recoilX = 2f;
    private float recoilY = 2f;
    public GameObject CamHolder;
    public GameObject gun;
    public GameObject Shell;
    public GameObject ShellSpawner;
    private float reloadTimer = 0f;
    private float reloadCooldown = 0.02f;
    [SerializeField] private GameObject pistol;
    [SerializeField] private GameObject BBullet;
    public bool shooting = false;
    private AudioSource _source;
    private PhotonView PV;
    [SerializeField] private Transform _mazzleFlashSpawner;
    [SerializeField] private GameObject _mazzleFlashe;

    #endregion

    #region UnityMethods

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        _source = pc.GetComponent<AudioSource>();
        SetText();
        animator = GetComponent<Animator>();
        Damage = 15f;
    }

    public override void OnTick()
    {

        if (Input.GetButton("Fire1") && gun.activeSelf && !pistol.activeSelf && !shooting)
            Use();
        if (Input.GetKey(KeyCode.R) && !pistol.activeSelf && !shooting)
            PV.RPC("RPC_Reload", RpcTarget.All);
                //StartCoroutine(ReloadCur());
        if (Input.GetKeyDown(KeyCode.Alpha3) && gun.activeSelf && !pistol.activeSelf && !shooting)
            StartCoroutine(HideGun());
        else if (Input.GetKeyDown(KeyCode.Alpha3) && !gun.activeSelf && !pistol.activeSelf && !shooting)
            StartCoroutine(DrawGun());
    }

    #endregion

    #region Methods

    public override void Use()
    {
        if (bulletsInTheGun > 0 && !reloading)
        {
            if (reloadTimer > 0)
                reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0)
            {
                reloadTimer = reloadCooldown;
                animator.SetBool("Shoot", true);
            }
        }
        else
            _source.PlayOneShot(NoAmmoClip);
    }
    public void SetText() => text.text = (bulletsInTheGun).ToString() + "/" + (bulletsIHave).ToString();
    private void Shoot()
    {
        CamHolder.transform.Rotate(new Vector3(Random.Range(-recoilX, recoilX), Random.Range(0, recoilY), 0));
        Ray ray = cam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        GameObject bulletShelltObj = Instantiate(Shell, ShellSpawner.transform.position, ShellSpawner.transform.rotation);
        Destroy(bulletShelltObj, 10f);
        ray.origin = cam.transform.position;
        bulletsInTheGun -= 1;
        SetText();
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
            hit.collider.gameObject.GetComponent<IDamagable>()?.TakeDamage(Damage, pc.Name);
            Impact_Shoot(hit.point, hit.normal, hit.collider.tag);
        }
        StartCoroutine(ShootAnimCur());
    }
    [PunRPC]
    private void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            if (tag == "Player")
            {
                GameObject bulletImpactObjB = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BulletBlood"), hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * BBullet.transform.rotation) as GameObject;
                bulletImpactObjB.transform.SetParent(colliders[0].transform);
                PhotonNetwork.Destroy(bulletImpactObjB.GetComponent<PhotonView>(), 1f);
            }
            else
            {
                GameObject bulletImpactObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BulletMetal"), hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * BulletImpactPrefab.transform.rotation) as GameObject;
                bulletImpactObj.transform.SetParent(colliders[0].transform);
                PhotonNetwork.Destroy(bulletImpactObj.GetComponent<PhotonView>(), 1f);
            }
        }
    }
    private void Impact_Shoot(Vector3 hitPosition, Vector3 hitNormal, string tag)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            if (tag == "Player")
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

    #endregion

    #region Coroutines

    public override IEnumerator HideGun()
    {
        animator.SetBool("Hide", true);
        animator.SetBool("Idle", false);
        animator.SetBool("Shoot", false);
        animator.SetBool("Draw", false);
        animator.SetBool("Reload", false);
        yield return new WaitForSeconds(0.4f);
        gun.SetActive(false);
        animator.SetBool("Hide", false);
        animator.SetBool("Idle", true);
        animator.SetBool("Shoot", false);
        animator.SetBool("Draw", false);
        animator.SetBool("Reload", false);
    }
    public IEnumerator IdleGun()
    {
        animator.SetBool("Hide", false);
        animator.SetBool("Shoot", false);
        animator.SetBool("Draw", false);
        animator.SetBool("Reload", false);
        animator.SetBool("Idle", true);
        yield return new WaitForSeconds(0.1f);
    }
    public override IEnumerator DrawGun()
    {
        SetText();
        gun.SetActive(true);
        animator.SetBool("Hide", false);
        animator.SetBool("Idle", false);
        animator.SetBool("Shoot", false);
        animator.SetBool("Draw", true);
        animator.SetBool("Reload", false);
        shooting = true;
        _source.PlayOneShot(ReloadDrawClip, 0.5f);
        yield return new WaitForSeconds(1.5f);
        animator.SetBool("Hide", false);
        animator.SetBool("Idle", true);
        animator.SetBool("Shoot", false);
        animator.SetBool("Draw", false);
        animator.SetBool("Reload", false);
        shooting = false;
    }
    private IEnumerator ShootAnimCur()
    {
        animator.SetBool("Hide", false);
        animator.SetBool("Idle", false);
        animator.SetBool("Shoot", true);
        animator.SetBool("Draw", false);
        animator.SetBool("Reload", false);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("Hide", false);
        animator.SetBool("Idle", true);
        animator.SetBool("Shoot", false);
        animator.SetBool("Draw", false);
        animator.SetBool("Reload", false);
    }
    private IEnumerator ReloadCur()
    {
        if (bulletsIHave > 0 && bulletsInTheGun < amountOfBulletsPerLoad && !reloading)
        {
            reloading = true;
            GameObject reloadSoundObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ReloadSound"), transform.position, transform.rotation) as GameObject;
            PhotonNetwork.Destroy(reloadSoundObj.GetComponent<PhotonView>(), 1f);
            PV.RPC("RPC_Reload", RpcTarget.All);
            animator.SetBool("Hide", false);
            animator.SetBool("Idle", false);
            animator.SetBool("Shoot", false);
            animator.SetBool("Draw", false);
            animator.SetBool("Reload", true);
            yield return new WaitForSeconds(2.7f);
            animator.SetBool("Hide", false);
            animator.SetBool("Idle", true);
            animator.SetBool("Shoot", false);
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
            {
                _source.PlayOneShot(NoAmmoClip);
                PV.RPC("RPC_NoAmmo", RpcTarget.All);
            }
            SetText();
            reloading = false;
        }
    }
    [PunRPC] private void RPC_Reload() 
    {
        StartCoroutine(ReloadCur());
        SetText();
    }
    [PunRPC]
    private void RPC_NoAmmo()
    {
        GameObject reloadSoundObjNoAmmo = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NoAmmoSound"), transform.position, transform.rotation) as GameObject;
        PhotonNetwork.Destroy(reloadSoundObjNoAmmo.GetComponent<PhotonView>(), 1f);
    }
    

    #endregion
}
