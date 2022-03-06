using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public sealed class PlayerController : MonoCacheMultiplayer, IDamagable
{
    public Animator animator;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private GameObject Canvas;
    public float mouseSensitivity, jumpForce, smoothTime;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private GameObject ui;
    public AudioClip footStepSound;
    public float footStepDelay = 0.65f;
    private float nextFootstep = 0f;
    private AudioSource _source;
    private Rigidbody rb;
    public PlayerManager playerManager;
    public AnimsPlayerController controller;
    private PhotonView PV;
    public AK ak;
    public string Name 
    { 
        get 
        {
            return PV.Owner.NickName;
        } 
    }
    private SafeFloat maxHealth = 100f;
    private SafeFloat currHealth;
    private float verticalLookRotation;
    private bool grounded;

    public SafeFloat GetHealth 
    { 
        get 
        { 
            return currHealth; 
        } 
    }

    private bool IsKilled = false;
    public bool IsDead 
    {
        get 
        { 
            return IsKilled; 
        }
        private set 
        { 
            IsKilled = value; 
        }
    }

    private void Awake()
    {
        currHealth = maxHealth;
        _source = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }
    private void Start()
    {
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
            Destroy(Canvas);
            Destroy(ui);
        }
            
    }

    public override void OnTick()
    //public void Update()
    {
        if (!PV.IsMine || IsKilled)
            return;
            
        Look();

        Jumping();

        PlayerMovementLogic();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(FindObjectOfType<UpdateManagerMultiplayer>()?.gameObject);
            Destroy(FindObjectOfType<DiedPanel>()?.gameObject);
            Destroy(FindObjectOfType<GameFactory>()?.gameObject);
            Debug.Log("back");
            PhotonNetwork.Disconnect();
            SceneManager.LoadSceneAsync(0);
        }
    }
    
    private void Look()
    {
        //if (!PV.IsMine) { return; }
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);
        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    public void SetGroundedState(bool _grounded) => grounded = _grounded;

    public void TakeDamage(float damage, string killer)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage, killer);
    }
        
    
    [PunRPC]
    private void RPC_TakeDamage(float damage, string _killer)
    {
        if (!PV.IsMine) { return; }

        currHealth -= damage;
        healthBarImage.fillAmount = currHealth / maxHealth;

        if (currHealth <= 0f)
        {
            Die(_killer);
        }
    }



    private void Die(string killer) 
    {
        if (!_isCrouching)
            controller.PV.RPC("RPC_Die", RpcTarget.All);
        else
            controller.PV.RPC("RPC_CrouchDie", RpcTarget.All);
        IsDead = true;
        StartCoroutine(DieCur(killer));

    }
    private IEnumerator DieCur(string _killer) 
    {
        yield return new WaitForSeconds(2f);
        playerManager.Die(_killer);
    }

    [Header("Player SOUNDS")]
    [Tooltip("Jump sound when player jumps.")]
    public AudioClip _jumpSound;

    [Header("PlayerSens")]
    [Tooltip("Current players speed")]
    public float currentSpeed;
    private Vector2 horizontalMovement;

    [Tooltip("The maximum speed you want to achieve")]
    public int maxSpeed = 5;
    [Tooltip("The higher the number the faster it will stop")]
    public float deaccelerationSpeed = 15.0f;


    [Tooltip("Force that is applied when moving forward or backward")]
    public float accelerationSpeed = 50000.0f;

    private Vector3 slowdownV;
    private LayerMask ignoreLayer;
    private bool _isCrouching = false;
    public Transform CrouchCamPos;
    public Transform StaticCamPos;

    private void PlayerMovementLogic()
    {
        //if (!PV.IsMine) { return; }
        currentSpeed = rb.velocity.magnitude;
        horizontalMovement = new Vector2(rb.velocity.x, rb.velocity.z);
        if (horizontalMovement.magnitude > maxSpeed)
        {
            horizontalMovement = horizontalMovement.normalized;
            horizontalMovement *= maxSpeed;
        }
        rb.velocity = new Vector3
        (
            horizontalMovement.x,
            rb.velocity.y,
            horizontalMovement.y
        );
        if (grounded)
        {
            rb.velocity = Vector3.SmoothDamp(rb.velocity,
                new Vector3(0, rb.velocity.y, 0),
                ref slowdownV,
                deaccelerationSpeed);
            rb.AddRelativeForce(Input.GetAxis("Horizontal") * accelerationSpeed * Time.deltaTime, 0, Input.GetAxis("Vertical") * accelerationSpeed * Time.deltaTime);
        }
        else
        {
            rb.AddRelativeForce(Input.GetAxis("Horizontal") * accelerationSpeed / 2 * Time.deltaTime, 0, Input.GetAxis("Vertical") * accelerationSpeed / 2 * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.C))
            _isCrouching = !_isCrouching;
        if (_isCrouching)
        {
            accelerationSpeed = 3500f;
            cameraHolder.transform.position = CrouchCamPos.position;
            controller.PV.RPC("RPC_CrouchIdle", RpcTarget.All);
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                deaccelerationSpeed = 0.7f;
                nextFootstep -= Time.deltaTime;

                if (nextFootstep <= 0)
                {
                    _source.PlayOneShot(footStepSound, 1f);
                    nextFootstep += footStepDelay;
                }
                controller.PV.RPC("RPC_CrouchWalk", RpcTarget.All);
            }
            else
            {
                deaccelerationSpeed = 0.1f;
                if (ak.reloading)
                {
                    controller.PV.RPC("RPC_CrouchReload", RpcTarget.All);
                    return;
                }
                controller.PV.RPC("RPC_CrouchIdle", RpcTarget.All);
                //PV.RPC("RPC_IdleCur", RpcTarget.All);
            }
        }
        else
        {
            cameraHolder.transform.position = StaticCamPos.position;
            controller.PV.RPC("RPC_Idle", RpcTarget.All);
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                deaccelerationSpeed = 0.5f;
                nextFootstep -= Time.deltaTime;

                if (nextFootstep <= 0)
                {
                    _source.PlayOneShot(footStepSound, 1f);
                    nextFootstep += footStepDelay;
                }
                if (ak.shooting)
                {
                    controller.PV.RPC("RPC_RunFire", RpcTarget.All);
                    return;
                }
                if (ak.reloading)
                {
                    controller.PV.RPC("RPC_RunReload", RpcTarget.All);
                    return;
                }
                controller.PV.RPC("RPC_Run", RpcTarget.All);
            }
            else
            {
                deaccelerationSpeed = 0.1f;
                if (ak.shooting)
                {
                    controller.PV.RPC("RPC_Fire", RpcTarget.All);
                    return;
                }
                if (ak.reloading)
                {
                    controller.PV.RPC("RPC_Reload", RpcTarget.All);
                    return;
                }
                controller.PV.RPC("RPC_Idle", RpcTarget.All);
            }
        }
        

        if (Input.GetKey(KeyCode.LeftShift) && !_isCrouching)
        {
            accelerationSpeed = 8500f;
            footStepDelay = 0.4f;
        }
        else
        {
            accelerationSpeed = 5000f;
            footStepDelay = 0.65f;
        }

    }

    private void Jumping()
    {
        //if (!PV.IsMine) { return; }
        if (Input.GetKeyDown(KeyCode.Space) && grounded && !_isCrouching)
        {
            rb.AddRelativeForce(Vector3.up * jumpForce);
            _source.PlayOneShot(_jumpSound, 0.1f);
        }
    }
}
