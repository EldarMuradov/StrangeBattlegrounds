using Photon.Pun;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class EnemyManager : MonoBehaviourPunCallbacks
{
    public delegate void OnBotKilledCallback(string setup);
    public OnBotKilledCallback onBotKilledCallback;
    public PhotonView PV;
    private GameObject _controller;
    public Teams Team;
    private TMP_Text _text;
    public bool IsMatchEnded = false;
    //public override void OnEnable() => PhotonNetwork.AddCallbackTarget(this);
    //public override void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

    [PunRPC]
    private void Awake()
    {
        onBotKilledCallback += Die;
        _text = GameObject.Find("Canvas/TextKilled").GetComponent<TMP_Text>();
        PV = GetComponent<PhotonView>();
        gameObject.name = Random.Range(0, 10000).ToString();
        LevenManager.Instance.Players++;
        GiveTeam();
    }
    private void Start()
    {
        if (PV.IsMine)
            Create();
    }
    public void Create()
    {
        Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint(Team, SceneManager.GetActiveScene().buildIndex);
        if (Team == Teams.RED)
            _controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BotControllerRed"), spawnPoint.position, spawnPoint.rotation, 0, new object[] { PV.ViewID });
        if (Team == Teams.BLUE)
            _controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BotControllerBlue"), spawnPoint.position, spawnPoint.rotation, 0, new object[] { PV.ViewID });
    }

    [PunRPC] private void GiveBM() => _controller.GetComponent<EnemyController>()._botManager = this;

    public void Die(string killer)
    {
        PhotonNetwork.Destroy(_controller);
        Create();
        if (Team == Teams.RED)
        {
            PV.RPC("RPC_OnKilled", RpcTarget.All, killer, Teams.BLUE);
            Debug.Log("TD");
        }
        else if (Team == Teams.BLUE)
        {
            PV.RPC("RPC_OnKilled", RpcTarget.All, killer, Teams.RED);
            Debug.Log("TD");
        }
    }

    [PunRPC] private void RPC_OnKilled(string killer, Teams teams)
    {
        StartCoroutine(TextKillFeedCur(killer));
        if (GameObject.Find(killer) && GameObject.Find(killer).GetComponent<PlayerManager>() != null)
        {
            GameObject.Find(killer).GetComponent<PlayerManager>().Kills++;
            if (teams == Teams.RED)
                LevenManager.Instance.RedKills++;
            else
                LevenManager.Instance.BlueKills++;
            LevenManager.Instance.CheckHandler(exit: out IsMatchEnded);
            Debug.Log("TK");
        }
        else 
        {
            if (teams == Teams.RED)
                LevenManager.Instance.RedKills++;
            else
                LevenManager.Instance.BlueKills++;
            LevenManager.Instance.CheckHandler(exit: out IsMatchEnded);
            Debug.Log("TK");
        }
    }
    private IEnumerator TextKillFeedCur(string killer)
    {
        _text.text = killer + " killed  " + gameObject.name;
        yield return new WaitForSeconds(3.5f);
        _text.text = "";
    }
    private void GiveTeam() => Team = Teams.RED;
}
