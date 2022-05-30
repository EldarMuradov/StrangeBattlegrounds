using Photon.Pun;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviourPunCallbacks, ITargetFinder, IDamagable
{
    public GameObject _target;
    private Animator _animator;
    private NavMeshAgent _agent;
    public Collider Coll;
    public Camera CamMain;
    private bool _isFiring = false;
    public float _health = 100f;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private GameObject _bulletImpactPrefab;
    private PhotonView PV;
    public GameObject CamHolder;
    public GameObject Shell;
    public GameObject ShellSpawner;
    public float Damage;
    public float BulletsInTheGun = 30;
    private float _amountOfBulletsPerLoad = 30;
    public bool Reloading = false;
    private float _bulletsIHave = 100000;
    public EnemyManager _botManager;
    public float Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
        }
    }
    #region UnityMethods

    private void Awake()
    {
        Damage = 20f;
        PV = GetComponent<PhotonView>();
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _botManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<EnemyManager>();
    }

    [PunRPC] public void Attemp() => _target = RoomManager.Instance.obj.GetComponent<PlayerManager>().controller;
    

    private void Update()
    {
        if (_target == null)
            PV.RPC("Attemp", RpcTarget.All);
        else
        {
            if (Vector3.Distance(transform.position, _target.transform.position) >= 200f && !_isFiring && _health > 0 && _target != null
                || Vector3.Distance(transform.position, _target.transform.position) < 200f && !Pointer() && _health > 0 && _target != null)
                PV.RPC("RPC_Run", RpcTarget.All);
            if (Vector3.Distance(transform.position, _target.transform.position) < 200f && Pointer() && !_isFiring && _health > 0 && _target != null)
                StartCoroutine(ShootCur());
        }
    }

    private bool Pointer()
    {
        Ray ray = CamMain.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = CamMain.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.CompareTag("Player"))
                return true;
            else
                return false;
        }
        return false;
    }
    private bool IsBehind(Vector3 position)
    {
        Vector3 forward = CamMain.transform.TransformDirection(Vector3.forward);
        Vector3 toOther = position - CamMain.transform.position;
        if (Vector3.Dot(forward, toOther) < 0)
            return true;
        else
            return false;
    }

    public void TakeDamage(float damage, string killer) => PV.RPC("RPC_TakeDamage", RpcTarget.All, damage, killer);

    [PunRPC] private void RPC_TakeDamage(float damage, string killer)
    {
        if (!PV.IsMine) { return; }

        Health -= damage;

        if (Health <= 0f)
        {
            Die(killer);
        }
    }

    private void Die(string killer)
    {
        PV.RPC("RPC_Die", RpcTarget.All);
        StartCoroutine(DieCur(killer));
    }

    private IEnumerator DieCur(string killer)
    {
        yield return new WaitForSeconds(2f);
        _botManager.onBotKilledCallback?.Invoke(killer);
    }

    #endregion

    #region Coroutines
    private IEnumerator ShootCur()
    {
        _agent.enabled = false;
        transform.LookAt(_target.transform.position);
        _isFiring = true;
        Shoot();
        yield return new WaitForSeconds(0.3f);
        _isFiring = false;
    }
    [PunRPC] private void RPC_Run()
    {
        _agent.enabled = true;
        _agent.SetDestination(_target.transform.position);
        _animator.SetBool("Reload", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Run", true);
    }
    [PunRPC] private void RPC_Die()
    {
        _agent.enabled = false;
        _animator.SetBool("Reload", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("Die", true);

    }
    [PunRPC] private void RPC_Idle()
    {
        _agent.enabled = false;
        _animator.SetBool("Reload", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("Idle", true);
    }

    #endregion


    #region FireEvents

    private IEnumerator ReloadCur()
    {
        if (_bulletsIHave > 0 && BulletsInTheGun < _amountOfBulletsPerLoad && !Reloading)
        {
            Reloading = true;
            GameObject reloadSoundObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "ReloadSound"), transform.position, transform.rotation) as GameObject;
            PhotonNetwork.Destroy(reloadSoundObj.GetComponent<PhotonView>(), 1f);

            yield return new WaitForSeconds(2.7f);

            if (_bulletsIHave - _amountOfBulletsPerLoad >= 0)
            {
                _bulletsIHave -= _amountOfBulletsPerLoad - BulletsInTheGun;
                BulletsInTheGun = _amountOfBulletsPerLoad;
            }
            else if (_bulletsIHave - _amountOfBulletsPerLoad < 0)
            {
                float valueForBoth = _amountOfBulletsPerLoad - BulletsInTheGun;
                if (_bulletsIHave - valueForBoth < 0)
                {
                    BulletsInTheGun += _bulletsIHave;
                    _bulletsIHave = 0;
                }
                else
                {
                    _bulletsIHave -= valueForBoth;
                    BulletsInTheGun += valueForBoth;
                }
            }
            else if (_bulletsIHave <= 0)
            {
                PV.RPC("RPC_NoAmmo", RpcTarget.All);
            }
            Reloading = false;
        }
    }
    private void Shoot()
    {
        if (BulletsInTheGun <= 0)
            PV.RPC("RPC_Reload", RpcTarget.All);
        else
        {
            Ray ray = CamMain.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            GameObject bulletShelltObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "shellmedium"), ShellSpawner.transform.position, ShellSpawner.transform.rotation);
            ray.origin = CamMain.transform.position;
            BulletsInTheGun -= 1;
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PV.RPC("RPC_Idle", RpcTarget.All);
                PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
                hit.collider.gameObject.GetComponent<IDamagable>()?.TakeDamage(Damage, _botManager.gameObject.name);
            }
        }
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
                GameObject bulletImpactObj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BulletMetal"), hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * _bulletImpactPrefab.transform.rotation) as GameObject;
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
                GameObject bulletImpactObj = Instantiate(_bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * _bulletImpactPrefab.transform.rotation);
                bulletImpactObj.transform.SetParent(colliders[0].transform);
            }

        }
    }

    [PunRPC] private void RPC_Reload()
    {
        _agent.enabled = false;
        _animator.SetBool("Reload", true);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("Idle", false);
        StartCoroutine(ReloadCur());
    }

    [PunRPC] private void RPC_NoAmmo()
    {
        GameObject reloadSoundObjNoAmmo = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NoAmmoSound"), transform.position, transform.rotation) as GameObject;
        PhotonNetwork.Destroy(reloadSoundObjNoAmmo.GetComponent<PhotonView>(), 1f);
    }
    #endregion
}
