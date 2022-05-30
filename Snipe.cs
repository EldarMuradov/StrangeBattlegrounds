using Photon.Pun;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public class Snipe : Gun
{
    [SerializeField] private PlayerController _pc;
    private PhotonView PV;
    public bool IsScoped = false;
    public GameObject ScopeOverlay;
    public Camera MainCamera;
    public GameObject Img;
    [SerializeField] private AK _ak;
    [SerializeField] private Shotgun _shotgun;
    private float _normalFOV;
    public float ScopedFOV = 15f;

    [SerializeField] private Animator _animator;
    public TMP_Text BulletText;
    [Header("Bullet properties")]
    [Tooltip("Preset value to tell with how much bullets will our waepon spawn inside rifle.")]
    public float BulletsInTheGun = 10;
    [Tooltip("Preset value to tell how much bullets can one magazine carry.")]
    [SerializeField] private float _amountOfBulletsPerLoad = 5;
    public bool Reloading = false;
    [SerializeField] private float _reloadChangeBulletsTime;
    [Tooltip("Array of muzzel flashes, randmly one will appear after each bullet.")]

    public AudioClip ReloadClip;
    public AudioClip FireClip;
    public AudioClip NoAmmoClip;
    public GameObject CamHolder;
    private float _recoilX = 0.2f;
    private float _recoilY = 0.2f;
    public GameObject Gun;
    public GameObject Shell;
    public GameObject ShellSpawner;
    public bool Shooting = false;
    [SerializeField] private GameObject BBullet;
    private AudioSource _source;
    public GameObject ScopeButton;

    public void Equip() 
    {
        StartCoroutine(DrawGun());
        _pc.Animator.Play("PutAndGrab");
    }
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        _source = _pc.GetComponent<AudioSource>();
        SetText();
        _animator = GetComponent<Animator>();
        Damage = 100f;
    }
    public void Scope() 
    {
        if (Gun.activeSelf && !_shotgun.Gun.activeSelf && !_ak.gun.activeSelf)
        {
            IsScoped = !IsScoped;
            _animator.SetBool("Scoped", IsScoped);
            _animator.SetBool("Hide", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("Shoot", false);
            _animator.SetBool("Draw", false);
            _animator.SetBool("Reload", false);

            if (IsScoped)
                StartCoroutine(OnScoped());
            else
                StartCoroutine(OnUnScoped());
        }
    }
    public void ButtonReload()
    {
        if (Gun.activeSelf)
            PV.RPC("RPC_Reload", RpcTarget.All);
    }

    public void UseAk()
    {
        if (Gun.activeSelf && !_shotgun.Gun.activeSelf && !_ak.gun.activeSelf && !Shooting)
        {
            StartCoroutine(HideGun());
            _pc.PV.RPC("RPC_SetActiveAk", RpcTarget.All);
            _ak.Equip();
        }
    }
    public void UseShot()
    {
        if (Gun.activeSelf && !_ak.gun.activeSelf && !_shotgun.Gun.activeSelf && !Shooting)
        {
            StartCoroutine(HideGun());
            _pc.PV.RPC("RPC_SetActiveShot", RpcTarget.All);
            _shotgun.Equip();
        }
    }
    public override IEnumerator HideGun()
    {
        _animator.SetBool("Hide", true);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("Scoped", false);
        yield return new WaitForSeconds(0.4f);
        Gun.SetActive(false);
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", true);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("Scoped", false);
    }
    public override IEnumerator DrawGun()
    {
        SetText();
        Gun.SetActive(true);
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", true);
        _animator.SetBool("Scoped", false);
        _animator.SetBool("Reload", false);
        Shooting = true;
        yield return new WaitForSeconds(0.2f);
        Shooting = false;
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", true);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("Scoped", false);
    }

    public override void Use()
    {
        if (BulletsInTheGun > 0 && !Reloading && !Shooting)
            Shoot();
        else
            _source.PlayOneShot(NoAmmoClip);
    }

    public void SetText() => BulletText.text = (BulletsInTheGun).ToString() + "/" + (bulletsIHave).ToString();

    private void Shoot()
    {
        SetText();
        StartCoroutine(ShootAnimCur());
        MainCamera.transform.Rotate(new Vector3(Random.Range(-_recoilX, _recoilX), Random.Range(0, _recoilY), 0));
        Ray ray = MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        GameObject bulletShelltObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "shelllarge"), ShellSpawner.transform.position, ShellSpawner.transform.rotation);
        //PhotonNetwork.Destroy(bulletShelltObj.GetComponent<PhotonView>(), 5f);
        //bulletShelltObj.GetComponent<PhotonView>().RPC("RPC_DestroyShell", RpcTarget.All, bulletShelltObj);
        ray.origin = MainCamera.transform.position;
        _source.PlayOneShot(FireClip);
        BulletsInTheGun -= 1;
        SetText();
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
            hit.collider.gameObject.GetComponent<IDamagable>()?.TakeDamage(Damage, PV.Owner.NickName);
            Impact_Shoot(hit.point, hit.normal, hit.collider.tag);
        }

    }
    [PunRPC]
    private void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            if (CompareTag("Player"))
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
    private IEnumerator ShootAnimCur()
    {
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", true);
        _animator.SetBool("Scoped", false);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
        Shooting = true;
        yield return new WaitForSeconds(0.2f);
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Shoot", true);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Scoped", false);
        _animator.SetBool("Reload", false);
        yield return new WaitForSeconds(1.4f);
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", true);
        _animator.SetBool("Scoped", false);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
        Shooting = false;
    }
    private IEnumerator ReloadCur()
    {
        if (bulletsIHave > 0 && BulletsInTheGun < _amountOfBulletsPerLoad && !Reloading)
        {
            Reloading = true;
            GameObject reloadSoundObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ReloadSound"), transform.position, transform.rotation) as GameObject;
            PhotonNetwork.Destroy(reloadSoundObj.GetComponent<PhotonView>(), 1f);
            PV.RPC("RPC_Reload", RpcTarget.All);
            _source.PlayOneShot(ReloadClip);
            _animator.SetBool("Hide", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("Shoot", false);
            _animator.SetBool("Scoped", false);
            _animator.SetBool("Draw", false);
            _animator.SetBool("Reload", true);
            yield return new WaitForSeconds(1.6f);
            _animator.SetBool("Hide", false);
            _animator.SetBool("Idle", true);
            _animator.SetBool("Shoot", false);
            _animator.SetBool("Scoped", false);
            _animator.SetBool("Draw", false);
            _animator.SetBool("Reload", false);
            if (bulletsIHave - _amountOfBulletsPerLoad >= 0)
            {
                bulletsIHave -= _amountOfBulletsPerLoad - BulletsInTheGun;
                BulletsInTheGun = _amountOfBulletsPerLoad;
            }
            else if (bulletsIHave - _amountOfBulletsPerLoad < 0)
            {
                float valueForBoth = _amountOfBulletsPerLoad - BulletsInTheGun;
                if (bulletsIHave - valueForBoth < 0)
                {
                    BulletsInTheGun += bulletsIHave;
                    bulletsIHave = 0;
                }
                else
                {
                    bulletsIHave -= valueForBoth;
                    BulletsInTheGun += valueForBoth;
                }
            }
            else if (bulletsIHave <= 0)
            {
                _source.PlayOneShot(NoAmmoClip);
                PV.RPC("RPC_NoAmmo", RpcTarget.All);
            }
            SetText();
            Reloading = false;
        }
    }

    [PunRPC]
    private void RPC_Reload()
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
    
    private IEnumerator OnUnScoped()
    {
        yield return new WaitForSeconds(0.15f);
        ScopeOverlay.SetActive(false);
        //WeaponCamera.SetActive(true);
        Img.SetActive(true);

        MainCamera.fieldOfView = _normalFOV;
    }
    private IEnumerator OnScoped()
    {
        yield return new WaitForSeconds(0.15f);
        ScopeOverlay.SetActive(true);
        //WeaponCamera.SetActive(false);
        Img.SetActive(false);

        _normalFOV = MainCamera.fieldOfView;
        MainCamera.fieldOfView = ScopedFOV;

    }
}
