using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public sealed class PlayerController : MonoCacheMultiplayer, IDamagable
{
    [Header("Player Vars")]
    [Tooltip("Jump sound when player jumps.")]
    public AudioClip _jumpSound;
    [Tooltip("Current players speed")]
    public float CurrentSpeed;
    private Vector2 _horizontalMovement;
    [Tooltip("The maximum speed you want to achieve")]
    public int MaxSpeed = 5;
    [Tooltip("The higher the number the faster it will stop")]
    public float DeaccelerationSpeed = 15.0f;
    [Tooltip("Force that is applied when moving forward or backward")]
    public float AccelerationSpeed = 8000.0f;
    private Vector3 _slowdownV;
    private bool _isCrouching = false;
    public Transform CrouchCamPos;
    public Transform StaticCamPos;
    public Animator Animator;
    public PostEffectsBase[] Base;
    private bool _useGraphicSettings = false;
    public TouchControllerMobile TouchControllerMobile;
    public GameObject Akown;
    public GameObject Snipeown;
    public GameObject Shotown;
    [SerializeField] private GameObject _cameraHolder;
    [SerializeField] private GameObject _canvas;
    public float MouseSensitivity, JumpForce, SmoothTime;
    public Image HealthBarImage;
    public GameObject UI;
    public AudioClip FootStepSound;
    public float FootStepDelay = 0.4f;
    private float _nextFootstep = 0f;
    private AudioSource _source;
    private Rigidbody _rb;
    public PlayerManager PlayerManager;
    public AnimsPlayerController Controller;
    public PhotonView PV;
    public AK Ak;
    public Snipe Snipe;
    public Shotgun Shotgun;
    [SerializeField] private Camera _deathCam;
    public string Name 
    { 
        get 
        {
            return PV.Owner.NickName;
        } 
    }
    private SafeFloat _maxHealth = 100f;
    private SafeFloat _currHealth;
    private float _verticalLookRotation;
    private bool _grounded;
    public FixedJoystick FixedJoystick;
    private float _horizontal;
    private float _vertical;
    private float _lerpJoystick = 7f;
    [SerializeField] private GameObject _cam;
    public SafeFloat GetHealth 
    { 
        get 
        { 
            return _currHealth; 
        } 
    }
    private bool _isKilled = false;
    public bool IsDead 
    {
        get 
        { 
            return _isKilled; 
        }
        private set 
        { 
            _isKilled = value; 
        }
    }
    public void SetGraphicSettings(bool value)
    {
        if (!value)
        {
            foreach (var b in Base)
                Destroy(b);
        }
    }
    private void Awake()
    {
        _currHealth = _maxHealth;
        _source = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        PlayerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }
    private void Start()
    {
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(_rb);
            Destroy(_canvas);
            Destroy(UI);
            Destroy(_cameraHolder);
        }
        if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            SetGraphicSettings(false);
        }
    }

    public override void OnTick()
    {
        if (!PV.IsMine || _isKilled)
            return;
            
        Look();

        PlayerMovementLogic();

    }
    public void Leave() => StartCoroutine(PlayerManager.LoadToMainLobby());

    //public Transform NewPos;
    
    private void Look()
    {
        if(TouchControllerMobile.TouchDist.x < 0)
            transform.Rotate(Vector3.up * TouchControllerMobile.TouchDist.x / 200 * MouseSensitivity);
        else if (TouchControllerMobile.TouchDist.x >= 0)
            transform.Rotate(Vector3.up * TouchControllerMobile.TouchDist.x / 2000 * MouseSensitivity);

        if (TouchControllerMobile.TouchDist.y < 0)
            _verticalLookRotation += TouchControllerMobile.TouchDist.y / 150 * MouseSensitivity;
        else if (TouchControllerMobile.TouchDist.y >= 0)
            _verticalLookRotation += TouchControllerMobile.TouchDist.y / 1500 * MouseSensitivity;
        _verticalLookRotation = Mathf.Clamp(_verticalLookRotation, -60f, 60f);
        _cam.transform.localEulerAngles = Vector3.left * _verticalLookRotation;// + Vector3.forward * 22f;
        //_cameraHolder.transform.position = Vector3.Lerp(transform.position, NewPos.position, 1f);
    }

    public void SetGroundedState(bool _grounded) => this._grounded = _grounded;

    public void TakeDamage(float damage, string killer) => PV.RPC("RPC_TakeDamage", RpcTarget.All, damage, killer);

    [PunRPC] private void RPC_TakeDamage(float damage, string killer)
    {
        if (!PV.IsMine) { return; }

        _currHealth -= damage;
        HealthBarImage.fillAmount = _currHealth / _maxHealth;

        if (_currHealth <= 0f)
        {
            Die(killer);
        }
    }

    private void Die(string killer) 
    {
        Snipe.ScopeOverlay.SetActive(false);
        if (!_isCrouching)
            Controller.PV.RPC("RPC_Die", RpcTarget.All);
        else
        {
            _cameraHolder.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            Controller.PV.RPC("RPC_CrouchDie", RpcTarget.All);
        }
        IsDead = true;
        StartCoroutine(DieCur(killer));
    }

    public GameObject GunHolder;
    private IEnumerator DieCur(string killer) 
    {
        Controller.Mesh.SetActive(true);
        GunHolder.SetActive(false);
        yield return new WaitForSeconds(2f);
        PlayerManager.onPlayerKilledCallback?.Invoke(killer);
    }

    public void Crouch() => _isCrouching = !_isCrouching;
    
    private void PlayerMovementLogic()
    {
        _horizontal = Mathf.Lerp(_horizontal, FixedJoystick.Horizontal, Time.deltaTime * _lerpJoystick);
        _vertical = Mathf.Lerp(_vertical, FixedJoystick.Vertical, Time.deltaTime * _lerpJoystick);
        CurrentSpeed = _rb.velocity.magnitude;
        //Debug.Log(_horizontal + " " + _vertical);
        _horizontalMovement = new Vector2(_rb.velocity.x, _rb.velocity.z);
        if (_horizontalMovement.magnitude > MaxSpeed)
        {
            _horizontalMovement = _horizontalMovement.normalized;
            _horizontalMovement *= MaxSpeed;
        }
        _rb.velocity = new Vector3
        (
            _horizontalMovement.x,
            _rb.velocity.y,
            _horizontalMovement.y
        );
        if (_grounded)
        {
            _rb.velocity = Vector3.SmoothDamp(_rb.velocity,
                new Vector3(0, _rb.velocity.y, 0),
                ref _slowdownV,
                DeaccelerationSpeed);
            _rb.AddRelativeForce(/*Input.GetAxis("Horizontal")*/_horizontal * AccelerationSpeed * Time.deltaTime, 0, /*Input.GetAxis("Vertical")*/_vertical * AccelerationSpeed * Time.deltaTime);
        }
        else
            _rb.AddRelativeForce(/*Input.GetAxis("Horizontal")*/ _horizontal * AccelerationSpeed / 2 * Time.deltaTime, 0, /*Input.GetAxis("Vertical")*/_vertical * AccelerationSpeed / 2 * Time.deltaTime);
        if (_isCrouching)
        {
            AccelerationSpeed = 3500f;
            //_cameraHolder.transform.position = CrouchCamPos.position;
            Controller.PV.RPC("RPC_CrouchIdle", RpcTarget.All);
            if (/*Input.GetAxis("Horizontal")*/Mathf.Abs(_horizontal) >= 0.1f || /*Input.GetAxis("Vertical")*/Mathf.Abs(_vertical) >= 0.1f)
            {
                DeaccelerationSpeed = 0.7f;
                _nextFootstep -= Time.deltaTime;

                if (_nextFootstep <= 0)
                {
                    _source.PlayOneShot(FootStepSound, 1f);
                    _nextFootstep += FootStepDelay;
                }
                Controller.PV.RPC("RPC_CrouchWalk", RpcTarget.All);
            }
            else
            {
                DeaccelerationSpeed = 0.1f;
                Controller.PV.RPC("RPC_CrouchIdle", RpcTarget.All);
            }
            if (Ak.Reloading || Shotgun.Reloading || Snipe.Reloading)
            {
                Controller.PV.RPC("RPC_CrouchReload", RpcTarget.All);
                return;
            }
        }
        else
        {
            //_cameraHolder.transform.position = StaticCamPos.position;
            Controller.PV.RPC("RPC_Idle", RpcTarget.All);
            if (/*Input.GetAxis("Horizontal")*/Mathf.Abs(_horizontal) >= 0.1f || /*Input.GetAxis("Vertical")*/Mathf.Abs(_vertical) >= 0.1f)
            {
                DeaccelerationSpeed = 0.5f;
                AccelerationSpeed = 6200f;
                _nextFootstep -= Time.deltaTime;

                if (_nextFootstep <= 0)
                {
                    _source.PlayOneShot(FootStepSound, 1f);
                    _nextFootstep += FootStepDelay;
                }
                if (Ak.Shooting || Shotgun.Shooting || Snipe.Shooting)
                {
                    Controller.PV.RPC("RPC_RunFire", RpcTarget.All);
                    return;
                }
                if (Ak.Reloading || Shotgun.Reloading || Snipe.Reloading)
                {
                    Controller.PV.RPC("RPC_RunReload", RpcTarget.All);
                    return;
                }
                Controller.PV.RPC("RPC_Run", RpcTarget.All);
            }
            else
            {
                DeaccelerationSpeed = 0.1f;
                if ((Shotgun.Shooting || Snipe.Shooting) && (Mathf.Abs(_horizontal) >= 0.1f || Mathf.Abs(_vertical) >= 0.1f))
                {
                    Controller.PV.RPC("RPC_RunFire", RpcTarget.All);
                    return;
                }
                else if ((Shotgun.Shooting || Snipe.Shooting) && (Mathf.Abs(_horizontal) < 0.1f || Mathf.Abs(_vertical) < 0.1f))
                {
                    Controller.PV.RPC("RPC_Fire", RpcTarget.All);
                    return;
                }
                if (Ak.Reloading || Shotgun.Reloading || Snipe.Reloading)
                {
                    Controller.PV.RPC("RPC_Reload", RpcTarget.All);
                    return;
                }
                Controller.PV.RPC("RPC_Idle", RpcTarget.All);
            }
        }
    }

    public void Jumping()
    {
        if (_grounded && !_isCrouching)
        {
            _rb.AddRelativeForce(Vector3.up * JumpForce);
            _source.PlayOneShot(_jumpSound, 0.1f);
        }
    } 
    [PunRPC] private void RPC_SetActiveSnipe()
    {
        Akown.SetActive(false);
        Shotown.SetActive(false);
        Snipeown.SetActive(true);
    }
    [PunRPC] private void RPC_SetActiveAk()
    {
        Akown.SetActive(true);
        Shotown.SetActive(false);
        Snipeown.SetActive(false);
    }
    [PunRPC] private void RPC_SetActiveShot()
    {
        Akown.SetActive(false);
        Shotown.SetActive(true);
        Snipeown.SetActive(false);
    }
}
