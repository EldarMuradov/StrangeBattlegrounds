using Photon.Pun;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class AK : Gun
{
    #region Variables

    [Header("Gun properties")]
    [SerializeField] private PlayerController _pc;
    [SerializeField] private Camera _cam;
    [SerializeField] private Animator _animator;
    [SerializeField] private Snipe Sniper;
    [SerializeField] private Shotgun Shotgun;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private Transform _mazzleFlashSpawner;
    [SerializeField] private GameObject _mazzleFlashe;
    private readonly float _recoilX = 2f;
    private readonly float _recoilY = 2f;
    private float _reloadTimer = 0f;
    private float _reloadCooldown = 0.02f;
    private AudioSource _source;
    private PhotonView PV;
    public TMP_Text BulletText;
    [Tooltip("Preset value to tell with how many bullets will our waepon spawn aside.")]
    public float BulletsInTheGun = 30;
    [Tooltip("Preset value to tell how much bullets can one magazine carry.")]
    [SerializeField] private float _amountOfBulletsPerLoad = 30;
    public bool Reloading = false;
    [SerializeField] private float _reloadChangeBulletsTime;
    [Tooltip("Array of muzzel flashes, randmly one will appear after each bullet.")]
    public AudioClip ReloadClip;
    public AudioClip FireClip;
    public AudioClip ReloadDrawClip;
    public AudioClip NoAmmoClip;
    public GameObject CamHolder;
    public GameObject gun;
    public GameObject Shell;
    public GameObject ShellSpawner;
    public bool Shooting = false;
    public bool ShootB = false;

    #endregion

    #region UnityMethods

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        _source = _pc.GetComponent<AudioSource>();
        SetText();
        _animator = GetComponent<Animator>();
        Damage = 27f;
    }
    public void Equip()
    {
        StartCoroutine(DrawGun());
        _pc.Animator.Play("PutAndGrab");
    }
    public void ButtonReload() 
    {
        if(gun.activeSelf)
        PV.RPC("RPC_Reload", RpcTarget.All);
    }

    public override void OnTick()
    {
        if (ShootB)
            Use();
    }

    #endregion

    #region Methods

    public override void Use()
    {
        if (BulletsInTheGun > 0 && !Reloading && !Shooting)
        {
            if (_reloadTimer > 0)
                _reloadTimer -= Time.deltaTime;
            if (_reloadTimer <= 0)
            {
                _reloadTimer = _reloadCooldown;
                _animator.SetBool("Shoot", true);
            }
        }
        else
            _source.PlayOneShot(NoAmmoClip);
    }
    public void UseShot() 
    {
        if (gun.activeSelf && !Sniper.Gun.activeSelf && !Shotgun.Gun.activeSelf && !Shooting)
        {
            StartCoroutine(HideGun());
            _pc.PV.RPC("RPC_SetActiveShot", RpcTarget.All);
            Shotgun.Equip();
        }
    }
    public void UseSnipe() 
    {
        if (gun.activeSelf && !Sniper.Gun.activeSelf && !Shotgun.Gun.activeSelf && !Shooting)
        {
            StartCoroutine(HideGun());
            _pc.PV.RPC("RPC_SetActiveSnipe", RpcTarget.All);
            Sniper.Equip();
        }
    }

    public void SetText() => BulletText.text = (BulletsInTheGun).ToString() + "/" + (bulletsIHave).ToString();
    private void Shoot()
    {
        _cam.transform.Rotate(new Vector3(Random.Range(-_recoilX, _recoilX), Random.Range(0, _recoilY), 0));
        Ray ray = _cam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        GameObject bulletShelltObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "shellmedium"), ShellSpawner.transform.position, ShellSpawner.transform.rotation);
        ray.origin = _cam.transform.position;
        BulletsInTheGun -= 1;
        SetText();
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
            hit.collider.gameObject.GetComponent<IDamagable>()?.TakeDamage(Damage, PV.Owner.NickName);
            Impact_Shoot(hit.point, hit.normal, hit.collider.tag);
        }
        StartCoroutine(ShootAnimCur());
    }

    [PunRPC] private void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            if (CompareTag("Player"))
            {
                GameObject bulletImpactObjB = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BulletBlood"), hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * _bullet.transform.rotation) as GameObject;
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
                GameObject bulletImpactObjB = Instantiate(_bullet, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * _bullet.transform.rotation);
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
        _animator.SetBool("Hide", true);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
        yield return new WaitForSeconds(0.4f);
        gun.SetActive(false);
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", true);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
    }
    public IEnumerator IdleGun()
    {
        _animator.SetBool("Hide", false);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("Idle", true);
        yield return new WaitForSeconds(0.1f);
    }
    public override IEnumerator DrawGun()
    {
        SetText();
        gun.SetActive(true);
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", true);
        _animator.SetBool("Reload", false);
        Shooting = true;
        _source.PlayOneShot(ReloadDrawClip, 0.5f);
        yield return new WaitForSeconds(1.5f);
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", true);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
        Shooting = false;
    }
    private IEnumerator ShootAnimCur()
    {
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Shoot", true);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
        yield return new WaitForSeconds(0.1f);
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", true);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
    }
    private IEnumerator ReloadCur()
    {
        if (bulletsIHave > 0 && BulletsInTheGun < _amountOfBulletsPerLoad && !Reloading)
        {
            Reloading = true;
            GameObject reloadSoundObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ReloadSound"), transform.position, transform.rotation) as GameObject;
            PhotonNetwork.Destroy(reloadSoundObj.GetComponent<PhotonView>(), 1f);
            PV.RPC("RPC_Reload", RpcTarget.All);
            _animator.SetBool("Hide", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("Shoot", false);
            _animator.SetBool("Draw", false);
            _animator.SetBool("Reload", true);
            yield return new WaitForSeconds(2.7f);
            _animator.SetBool("Hide", false);
            _animator.SetBool("Idle", true);
            _animator.SetBool("Shoot", false);
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

    [PunRPC] private void RPC_Reload() 
    {
        StartCoroutine(ReloadCur());
        SetText();
    }

    [PunRPC] private void RPC_NoAmmo()
    {
        GameObject reloadSoundObjNoAmmo = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NoAmmoSound"), transform.position, transform.rotation) as GameObject;
        PhotonNetwork.Destroy(reloadSoundObjNoAmmo.GetComponent<PhotonView>(), 1f);
    }


    #endregion
}
