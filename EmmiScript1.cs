using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EmmiScript1 : MonoCache
{
    [SerializeField] protected GameObject _gun;
    [SerializeField] protected GameObject[] _target;
    protected Animator _animator;
    protected NavMeshAgent _agent;
    protected AudioSource _source;
    public Camera cam;
    [SerializeField] protected AudioClip _fireClip;
    [SerializeField] protected GameObject BBullet;
    [SerializeField] protected GameObject BulletImpactPrefab;
    public GameObject Enemy;
    public NaitScript Nait;
    public VardoWarehouse1 vardo;
    public MikleScriptWarehouse Mikle;

    #region UnityMethods

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _source = GetComponent<AudioSource>();
    }

    private void Start() => StartCoroutine(WalkWithPistol(0));
    

    private void OnTriggerEnter(Collider other)
    {
        /*if (other.transform.tag == "EmmiTarget2")
        {
            StartCoroutine(Aim());
            Destroy(other.gameObject);
        }*/
        if (other.transform.tag == "DeallerTriggerE")
            StartCoroutine(Aim());
    }

    #endregion

    #region Methods

    public void OnStart() => StartCoroutine(WalkWithPistol(2));
    public void OnSecondStart() => StartCoroutine(WalkWithPistol(3));
    public void OnNaitKilled() => StartCoroutine(PistolIdle());

    #endregion

    #region Coroutines

    protected IEnumerator ShootFromAngleCur() 
    {
        _agent.enabled = false;
        _animator.SetBool("Walk", false);
        _animator.SetBool("Aim", false);
        _animator.SetBool("PistolIdle", false);
        _animator.SetBool("ShootFromAngle", true);
        yield return new WaitForSeconds(2.5f);
    }
    protected IEnumerator WalkWithPistol(int i)
    {
        yield return new WaitForSeconds(0.3f);
        _agent.enabled = true;
        _agent.SetDestination(_target[i].transform.position);
        _animator.SetBool("Aim", false);
        _animator.SetBool("PistolIdle", false);
        _animator.SetBool("ShootFromAngle", false);
        _animator.SetBool("Walk", true);
        yield return new WaitForSeconds(2.5f);
    }
    protected IEnumerator Aim()
    {
        _agent.enabled = false;
        _animator.SetBool("PistolIdle", false);
        _animator.SetBool("ShootFromAngle", false);
        _animator.SetBool("Walk", false);
        _animator.SetBool("Aim", true);
        yield return new WaitForSeconds(2.5f);
    }
    protected IEnumerator PistolIdle()
    {
        _agent.enabled = false;
        _animator.SetBool("ShootFromAngle", false);
        _animator.SetBool("Walk", false);
        _animator.SetBool("Aim", false);
        _animator.SetBool("PistolIdle", true);
        yield return new WaitForSeconds(2.5f);
    }

    #endregion

    #region FireEvents

    protected void Fire()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;
        _source.PlayOneShot(_fireClip);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hit.collider.gameObject.GetComponent<IDamagableSolo>()?.TakeDamage(1);
            Impact_Shoot(hit.point, hit.normal, hit.collider.gameObject.tag);
        }
    }
    protected void Impact_Shoot(Vector3 hitPosition, Vector3 hitNormal, string tag)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            if (tag == "Enemy")
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
}