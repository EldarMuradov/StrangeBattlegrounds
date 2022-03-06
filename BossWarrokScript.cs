using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossWarrokScript : Enemy, IDamagableSolo
{
    #region Variables
    public Collider[] colls;
    private bool _isPunching = false;
    private Animator _animator;
    private NavMeshAgent _agent;
    public AudioSource _sourceDie;
    public AudioSource _sourceRoar;
    public AudioSource _sourcePunch;
    [SerializeField] private Transform _target;
    private bool _isRoaring = false;
    private bool _isDead = false;
    private SafeFloat _health;
    public SafeFloat Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health -= value;
            if (_health % 50 == 0)
            {
                _isRoaring = true;
                Roar();
            }
            if (_health <= 0f)
            {
                _isDead = true;
                StartCoroutine(DieAnimation());
            }
        }
    }
    public delegate void Chaising();
    public Chaising chaising;
    public event Chaising Fight;

    #endregion

    #region UnityMethods

    private void Awake()
    {
        _health = MaxHealth;
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        chaising = Seaking;
        chaising += Punch;
        Fight += chaising;
    }
    //public override void OnTick() => 
    private void Update()=>    
        Fight?.Invoke();
    public override void Create(Level level)
    {
        if (level == Level.Easy)
        {
            MaxHealth = 150f;//30
            _health = MaxHealth;
        }
        if (level == Level.Medium)
        {
            MaxHealth = 225f;//30
            _health = MaxHealth;
        }
        if (level == Level.Hard)
        {
            MaxHealth = 300f;//30
            _health = MaxHealth;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Explosion")
        {
            _isRoaring = true;
            TakeDamage(10); //tripple damage

        }
        if (collision.transform.tag == "PlayerKnife")
        {
            StartCoroutine(TakeDamageKnife(15, 1f));
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Explosion")
        {
            _isRoaring = true;
            TakeDamage(10); //tripple damage

        }
        if (other.transform.tag == "PlayerKnife")
        {
            StartCoroutine(TakeDamageKnife(15, 1f));
        }
    }

    #endregion

    #region Methods

    private void Punch()
    {
        if (Vector3.Distance(_target.transform.position, gameObject.transform.position) < 4f && !_isRoaring && !_isPunching)
            StartCoroutine(PunchCur());
    }
    private void Seaking()
    {
        if (Vector3.Distance(_target.transform.position, gameObject.transform.position) < 100f && Vector3.Distance(_target.transform.position, gameObject.transform.position) >= 4f && !_isRoaring && !_isPunching && !_isDead)
            StartCoroutine(SeakCur());
    }

    private void Roar()
    {
            StartCoroutine(RoarCur());
    }
    public void TakeDamage(float damage) => Health = damage;


    #endregion

    #region Coroutines

    private IEnumerator RoarCur()
    {
        _agent.enabled = false;
        _isRoaring = true;
        _sourceRoar.gameObject.SetActive(true);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Punch", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("PunchA", false);
        _animator.SetBool("PunchB", false);
        _animator.SetBool("Die", false);
        _animator.SetBool("Roar", true);
        yield return new WaitForSeconds(4.5f);
        _sourceRoar.gameObject.SetActive(false);
        _isRoaring = false;
    }
    private IEnumerator PunchCur()
    {
        _agent.enabled = false;
        int i = NewRange.GenerateRandonRange(0,10);
        _sourcePunch.gameObject.SetActive(true);
        _isPunching = true;
        if (i >= 0 && i < 5)
        {
            _animator.SetBool("Run", false);
            _animator.SetBool("Roar", false);
            _animator.SetBool("PunchA", false);
            _animator.SetBool("PunchB", false);
            _animator.SetBool("Punch", true);
            yield return new WaitForSeconds(2.2f);
        }
        if (i >= 5 && i < 9)
        {
            _animator.SetBool("Run", false);
            _animator.SetBool("Roar", false);
            _animator.SetBool("Punch", false);
            _animator.SetBool("PunchB", false);
            _animator.SetBool("PunchA", true);
            yield return new WaitForSeconds(2.5f);
        }
        if (i >= 9 && i < 10)
        {
            _animator.SetBool("Run", false);
            _animator.SetBool("Roar", false);
            _animator.SetBool("Punch", false);
            _animator.SetBool("PunchA", false);
            _animator.SetBool("PunchB", true);
            yield return new WaitForSeconds(3.5f);
            transform.Translate(new Vector3(0, 0, 7f), Space.Self);
        }
        _sourcePunch.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        _isPunching = false;
    }
    private IEnumerator SeakCur()
    {
        _agent.enabled = true;
        _agent.SetDestination(_target.transform.position);
        _animator.SetBool("Idle", false);
        _animator.SetBool("Punch", false);
        _animator.SetBool("Roar", false);
        _animator.SetBool("Die", false);
        _animator.SetBool("PunchA", false);
        _animator.SetBool("PunchB", false);
        _animator.SetBool("Run", true);
        yield return new WaitForSeconds(2f);
    }
    private IEnumerator DieAnimation()
    {
        _agent.enabled = false;
        _sourceDie.gameObject.SetActive(true);
        foreach (Collider c in colls)
            c.enabled = false;
        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
        gameObject.GetComponent<Collider>().enabled = false;
        _animator.SetBool("Idle", false);
        _animator.SetBool("Punch", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("Roar", false);
        _animator.SetBool("PunchA", false);
        _animator.SetBool("PunchB", false);
        _animator.SetBool("Die", true);
        yield return new WaitForSeconds(3f);
    }
    private IEnumerator TakeDamageKnife(SafeFloat damage, float time)
    {
        TakeDamage(damage);
        yield return new WaitForSeconds(time);
    }

    #endregion
}
