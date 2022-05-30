using Photon.Pun;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public class Shotgun : Gun
{
    [SerializeField] private PlayerController _pc;
    [SerializeField] private AK _ak;
    [SerializeField] private Snipe _snipe;
    [SerializeField] private Camera _cam;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _reloadChangeBulletsTime;
    [SerializeField] private float _amountOfBulletsPerLoad = 5;
    [SerializeField] private GameObject _bullet;
    private readonly float _recoilX = 2f;
    private readonly float _recoilY = 2f;
    private PhotonView PV;
    private AudioSource _source;
    public TMP_Text BulletText;
    public float BulletsInTheGun = 10;
    public bool Reloading = false;
    public AudioClip ReloadClip;
    public AudioClip ReloadDrawClip;
    public AudioClip FireClip;
    public AudioClip NoAmmoClip;
    public GameObject CamHolder;
    public GameObject Gun;
    public GameObject Shell;
    public GameObject ShellSpawner;
    public bool Shooting = false;

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
        Damage = 12.5f;
    }

    public void ButtonReload()
    {
        if (Gun.activeSelf)
            PV.RPC("RPC_Reload", RpcTarget.All);
    }

    public void UseAk() 
    {
        if (Gun.activeSelf && !_snipe.Gun.activeSelf && !_ak.gun.activeSelf && !Shooting)
        {
            StartCoroutine(HideGun());
            _pc.PV.RPC("RPC_SetActiveAk", RpcTarget.All);
            _ak.Equip();
        }
    }
    public void UseSnipe() 
    {
        if (Gun.activeSelf && !_snipe.Gun.activeSelf && !_ak.gun.activeSelf && !Shooting)
        {
            StartCoroutine(HideGun());
            _pc.PV.RPC("RPC_SetActiveSnipe", RpcTarget.All);
            _snipe.Equip();
        }
    }
    public override IEnumerator HideGun()
    {
        _animator.SetBool("Hide", true);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
        yield return new WaitForSeconds(0.4f);
        Gun.SetActive(false);
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", true);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
    }
    public override IEnumerator DrawGun()
    {
        SetText();
        Gun.SetActive(true);
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", true);
        _animator.SetBool("Reload", false);
        Shooting = true;
        _source.PlayOneShot(ReloadDrawClip);
        yield return new WaitForSeconds(0.3f);
        Shooting = false;
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", true);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
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
        StartCoroutine(ShootAnimCur());
        for (int i = 0; i < 8; i++)
        {
            _cam.transform.Rotate(new Vector3(Random.Range(-_recoilX, _recoilX), Random.Range(0, _recoilY), 0));
            Ray ray = _cam.ViewportPointToRay(new Vector3(0.477f, 0.5f));
            ray.origin = _cam.transform.position;
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Impact_Shoot(hit.point, hit.normal, hit.collider.tag);
                hit.collider.gameObject.GetComponent<IDamagable>()?.TakeDamage(Damage, PV.Owner.NickName);
            }
        }
        GameObject bulletShelltObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "slug"), ShellSpawner.transform.position, ShellSpawner.transform.rotation);
        _source.PlayOneShot(FireClip);
        BulletsInTheGun -= 1;
        SetText();
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

    private IEnumerator ShootAnimCur()
    {
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", true);
        _animator.SetBool("Shoot", false);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
        Shooting = true;
        yield return new WaitForSeconds(0.2f);
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Shoot", true);
        _animator.SetBool("Draw", false);
        _animator.SetBool("Reload", false);
        yield return new WaitForSeconds(0.6f);
        _animator.SetBool("Hide", false);
        _animator.SetBool("Idle", true);
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
            _animator.SetBool("Hide", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("Shoot", false);
            _animator.SetBool("Draw", false);
            _animator.SetBool("Reload", true);
            yield return new WaitForSeconds(1.6f);
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
}
