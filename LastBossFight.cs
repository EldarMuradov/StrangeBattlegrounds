using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class LastBossFight : Enemy, IDamagableSolo
{
    #region Variables

    protected Animator _animator;
    protected NavMeshAgent _agent;
    [SerializeField] protected Transform _target;
    private bool _isRoaring = false;
    private bool _isDead = false;
    [SerializeField] private float _health = 220f;
    public AudioSource _sourceRoar;
    public AudioSource _sourcePunch;
    public AudioSource _sourceDie;
    private bool _isPunching = false;
    public Collider[] colls;
    private bool _roared = false;
    public SafeFloat Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health -= value;
            if (_health < 200f && !_isDead && !_roared)
            {
                _roared = true;
                _isRoaring = true;
                Roar();
            }

            if (_health <= 0f)
            {
                _isDead = true;
                Die();
            }
                
        }
    }

    #endregion
     
    
    #region UnityMethods

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        Damage = 30f;
    }
    private void Update()
    //public override void OnTick()
    {
        Punch();
        Seaking();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Explosion"))
        {
            _isRoaring = true;
            TakeDamage(10); //tripple damage

        }
        if (collision.transform.CompareTag("PlayerKnife"))
        {
            StartCoroutine(TakeDamageKnife(15, 1f));
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Explosion"))
        {
            _isRoaring = true;
            TakeDamage(10);

        }
        if (other.transform.CompareTag("PlayerKnife"))
        {
            StartCoroutine(TakeDamageKnife(15, 1f));
        }
    }

    #endregion

    #region Methods

    private void Punch()
    {
        if (Vector3.Distance(_target.transform.position, gameObject.transform.position) < 3.1f && !_isRoaring && !_isDead &&!_isPunching)
        {
            StartCoroutine(PunchCur());
        }
    }
    private void Seaking()
    {
        if (Vector3.Distance(_target.transform.position, gameObject.transform.position) < 50f && Vector3.Distance(_target.transform.position, gameObject.transform.position) >= 3.1f && !_isRoaring && !_isDead && !_isPunching)
        {
            StartCoroutine(SeakCur());
        }
    }
    private void Roar()
    {
            StartCoroutine(RoarCur());
    }
    private void Die()
    {
            StartCoroutine(DeathCur());    
    }
    public void TakeDamage(float damage) => Health = damage;


    #endregion

    #region Coroutines

    private IEnumerator DeathCur() 
    {
        foreach (Collider c in colls)
            c.enabled = false;
        _sourceDie.gameObject.SetActive(true);
        _agent.enabled = false;
        _animator.SetBool("Combo", false);
        _animator.SetBool("Roar", false);
        _animator.SetBool("Splash", false);
        _animator.SetBool("Combo_1", false);
        _animator.SetBool("Combo_2", false);
        _animator.SetBool("JumpAttack", false);
        _animator.SetBool("360Attack", false);
        _animator.SetBool("Walk", false);
        _animator.SetBool("Die", true);
        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
        gameObject.GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(3f);
    }
    private IEnumerator RoarCur()
    {
        _isRoaring = true;
        _sourceRoar.gameObject.SetActive(true);
        _animator.SetBool("Combo", false);
        _animator.SetBool("Die", false);
        _animator.SetBool("Splash", false);
        _animator.SetBool("Combo_1", false);
        _animator.SetBool("Combo_2", false);
        _animator.SetBool("JumpAttack", false);
        _animator.SetBool("360Attack", false);
        _animator.SetBool("Walk", false);
        _animator.SetBool("Roar", true);
        yield return new WaitForSeconds(5f);
        _sourceRoar.gameObject.SetActive(false);
        _isRoaring = false;
    }
    private IEnumerator SeakCur()
    {
        _agent.enabled = true;
        _agent.SetDestination(_target.transform.position);
        _animator.SetBool("Combo", false);
        _animator.SetBool("Roar", false);
        _animator.SetBool("Die", false);
        _animator.SetBool("Splash", false);
        _animator.SetBool("Combo_1", false);
        _animator.SetBool("Combo_2", false);
        _animator.SetBool("JumpAttack", false);
        _animator.SetBool("360Attack", false);
        _animator.SetBool("Walk", true);
        yield return new WaitForSeconds(3f);
    }
    private IEnumerator PunchCur()
    {
        _agent.enabled = false;
        int i = NewRange.GenerateRandonRange(0, 10);
        _isPunching = true;
        _sourcePunch.gameObject.SetActive(true);
        if (i >= 0 && i < 1)
        {
            _animator.SetBool("Combo", false);
            _animator.SetBool("Roar", false);
            _animator.SetBool("Die", false);
            _animator.SetBool("Combo_1", false);
            _animator.SetBool("Combo_2", false);
            _animator.SetBool("JumpAttack", false);
            _animator.SetBool("360Attack", false);
            _animator.SetBool("Walk", false);
            _animator.SetBool("Splash", true);
            yield return new WaitForSeconds(3.5f);
            transform.Translate(new Vector3(0, 0, 1.5f), Space.Self);
        }
        if (i >= 1 && i < 2)
        {
            _animator.SetBool("Combo", false);
            _animator.SetBool("Roar", false);
            _animator.SetBool("Die", false);
            _animator.SetBool("JumpAttack", false);
            _animator.SetBool("Walk", false);
            _animator.SetBool("Splash", false);
            _animator.SetBool("360Attack", false);
            _animator.SetBool("Combo_1", false);
            _animator.SetBool("Combo_2", true);
            yield return new WaitForSeconds(3.5f);
            transform.Translate(new Vector3(0, 0, 3f), Space.Self);
        }
        if (i >= 2 && i < 3)
        {
            _animator.SetBool("Combo", false);
            _animator.SetBool("Roar", false);
            _animator.SetBool("Die", false);
            _animator.SetBool("Combo_1", false);
            _animator.SetBool("Combo_2", false);
            _animator.SetBool("JumpAttack", false);
            _animator.SetBool("Walk", false);
            _animator.SetBool("Splash", false);
            _animator.SetBool("360Attack", true);
            yield return new WaitForSeconds(3.5f);
        }
        if (i >= 3 && i < 4)
        {
            _animator.SetBool("Combo", false);
            _animator.SetBool("Roar", false);
            _animator.SetBool("Die", false);
            _animator.SetBool("Combo_2", false);
            _animator.SetBool("JumpAttack", false);
            _animator.SetBool("Walk", false);
            _animator.SetBool("Splash", false);
            _animator.SetBool("360Attack", false);
            _animator.SetBool("Combo_1", true);
            yield return new WaitForSeconds(3.5f);
            transform.Translate(new Vector3(0, 0, 3f), Space.Self);
        }
        if (i >= 4 && i < 5)
        {
            _animator.SetBool("Roar", false);
            _animator.SetBool("Die", false);
            _animator.SetBool("Combo_1", false);
            _animator.SetBool("Combo_2", false);
            _animator.SetBool("Walk", false);
            _animator.SetBool("Splash", false);
            _animator.SetBool("360Attack", false);
            _animator.SetBool("JumpAttack", false);
            _animator.SetBool("Combo", true);
            yield return new WaitForSeconds(3.5f);
            transform.Translate(new Vector3(0, 0, 3f), Space.Self);
        }
        if (i >= 5 && i < 6)
        {
            _animator.SetBool("Combo", false);
            _animator.SetBool("Roar", false);
            _animator.SetBool("Die", false);
            _animator.SetBool("Combo_1", false);
            _animator.SetBool("Combo_2", false);
            _animator.SetBool("Walk", false);
            _animator.SetBool("Splash", false);
            _animator.SetBool("360Attack", false);
            _animator.SetBool("JumpAttack", true);
            yield return new WaitForSeconds(3.5f);
            transform.Translate(new Vector3(0,0,7f), Space.Self);  
        }
        _sourcePunch.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        _isPunching = false;
    }
    private IEnumerator TakeDamageKnife(SafeFloat damage, float time)
    {
        TakeDamage(damage);
        yield return new WaitForSeconds(time);
    }

    #endregion
}
