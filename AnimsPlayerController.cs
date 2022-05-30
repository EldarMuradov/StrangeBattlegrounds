using Photon.Pun;
using System.Collections;
using UnityEngine;

public class AnimsPlayerController : MonoBehaviour
{
    private Animator _animator;
    public PhotonView PV;
    public bool CanChangeState = true;
    public GameObject Mesh;
    public GameObject GunHandler;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        _animator = GetComponent<Animator>();
    }
    private void Start()
    {
        if (PV.IsMine)
        {
            Mesh.SetActive(false);
            GunHandler.SetActive(false);
        }
    }
    [PunRPC] public void RPC_Idle() 
    {
        _animator.SetBool("Die", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("RunFire", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("RunReload", false);
        _animator.SetBool("Idle", true);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", false);
        _animator.SetBool("CrouchIdle", false);
        _animator.SetBool("CrouchDie", false);
        _animator.SetBool("CrouchReload", false);
        _animator.SetBool("CrouchOff", false);
    }

    [PunRPC]
    public void RPC_Run()
    {
        _animator.SetBool("RunFire", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("RunReload", false);
        _animator.SetBool("Die", false);
        _animator.SetBool("Run", true);
        _animator.SetBool("Idle", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", false);
        _animator.SetBool("CrouchIdle", false);
        _animator.SetBool("CrouchDie", false);
        _animator.SetBool("CrouchReload", false);
        _animator.SetBool("CrouchOff", false);
    }

    [PunRPC]
    public void RPC_Die()
    {
        _animator.SetBool("RunFire", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("RunReload", false);
        _animator.SetBool("Die", true);
        _animator.SetBool("Run", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", false);
        _animator.SetBool("CrouchIdle", false);
        _animator.SetBool("CrouchDie", false);
        _animator.SetBool("CrouchReload", false);
        _animator.SetBool("CrouchOff", false);
    }

    [PunRPC]
    public void RPC_RunReload()
    {
        _animator.SetBool("Die", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("RunFire", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("RunReload", true);
        _animator.SetBool("Idle", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", false);
        _animator.SetBool("CrouchIdle", false);
        _animator.SetBool("CrouchDie", false);
        _animator.SetBool("CrouchReload", false);
        _animator.SetBool("CrouchOff", false);
    }
    [PunRPC]
    public void RPC_Reload()
    {
        _animator.SetBool("Die", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("RunFire", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Reload", true);
        _animator.SetBool("RunReload", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", false);
        _animator.SetBool("CrouchIdle", false);
        _animator.SetBool("CrouchDie", false);
        _animator.SetBool("CrouchReload", false);
        _animator.SetBool("CrouchOff", false);
    }
    [PunRPC]
    public void RPC_Fire()
    {
        _animator.SetBool("Die", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("RunFire", false);
        _animator.SetBool("Fire", true);
        _animator.SetBool("Reload", false);
        _animator.SetBool("RunReload", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", false);
        _animator.SetBool("CrouchIdle", false);
        _animator.SetBool("CrouchDie", false);
        _animator.SetBool("CrouchReload", false);
        _animator.SetBool("CrouchOff", false);
    }
    [PunRPC]
    public void RPC_RunFire()
    {
        _animator.SetBool("Die", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("RunFire", true);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("RunReload", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", false);
        _animator.SetBool("CrouchIdle", false);
        _animator.SetBool("CrouchDie", false);
        _animator.SetBool("CrouchReload", false);
        _animator.SetBool("CrouchOff", false);
    }
    [PunRPC]
    public void RPC_CrouchOff()
    {
        _animator.SetBool("Die", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("RunFire", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("RunReload", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", false);
        _animator.SetBool("CrouchIdle", false);
        _animator.SetBool("CrouchDie", false);
        _animator.SetBool("CrouchReload", false);
        _animator.SetBool("CrouchOff", true);
    }
    [PunRPC]
    public void RPC_CrouchReload()
    {
        _animator.SetBool("Die", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("RunFire", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("RunReload", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", false);
        _animator.SetBool("CrouchIdle", false);
        _animator.SetBool("CrouchDie", false);
        _animator.SetBool("CrouchReload", true);
        _animator.SetBool("CrouchOff", false);
    }
    [PunRPC]
    public void RPC_CrouchWalk()
    {
        _animator.SetBool("Die", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("RunFire", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("RunReload", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", true);
        _animator.SetBool("CrouchIdle", false);
        _animator.SetBool("CrouchDie", false);
        _animator.SetBool("CrouchReload", false);
        _animator.SetBool("CrouchOff", false);
    }
    [PunRPC]
    public void RPC_CrouchIdle()
    {
        _animator.SetBool("Die", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("RunFire", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("RunReload", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", false);
        _animator.SetBool("CrouchIdle", true);
        _animator.SetBool("CrouchDie", false);
        _animator.SetBool("CrouchReload", false);
        _animator.SetBool("CrouchOff", false);
    }
    private IEnumerator C_I() 
    {
        CanChangeState = false;
        _animator.SetBool("Die", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("RunFire", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("RunReload", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", false);
        _animator.SetBool("CrouchIdle", true);
        _animator.SetBool("CrouchDie", false);
        _animator.SetBool("CrouchReload", false);
        _animator.SetBool("CrouchOff", false);
        yield return new WaitForSeconds(1.5f);
        CanChangeState = true;
    }
    [PunRPC]
    public void RPC_CrouchDie()
    {
        _animator.SetBool("Die", false);
        _animator.SetBool("Run", false);
        _animator.SetBool("RunFire", false);
        _animator.SetBool("Fire", false);
        _animator.SetBool("Reload", false);
        _animator.SetBool("RunReload", false);
        _animator.SetBool("Idle", false);
        _animator.SetBool("CrouchOn", false);
        _animator.SetBool("CrouchWalk", false);
        _animator.SetBool("CrouchIdle", false);
        _animator.SetBool("CrouchDie", true);
        _animator.SetBool("CrouchReload", false);
        _animator.SetBool("CrouchOff", false);
    }
}
