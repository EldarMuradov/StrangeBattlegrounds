using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class PlayerInfo 
{
    public ProfileData profile;
    public int actor;
    public short kills;
    public short deaths;

    public PlayerInfo(ProfileData p, int a, short k, short d) 
    {
        this.profile = p;
        this.kills = k;
        this.actor = a;
        this.deaths = d;

    }
}
public enum GameState
{
    Waiting = 0,
    Starting = 1,
    Playing = 2,
    Ending = 3
}

public enum EventCodes : byte
{
    NewPlayer,
    ChangeStat,
    NewMatch,
    RefreshTimer
}
public class PlayerManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public delegate void OnPlayerKilledCallback(string player, string setup);
    public OnPlayerKilledCallback onPlayerKilledCallback;
    private PhotonView PV;
    private TMP_Text text;
    private GameObject controller;
    public SafeInt Kills;
    public SafeInt Deaths;

    public static PlayerManager Instance;

    public override void OnEnable()
    {
        text = GameObject.Find("Canvas/TextKilled").GetComponent<TMP_Text>();
        PhotonNetwork.AddCallbackTarget(this);
    }
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return;

        EventCodes e = (EventCodes)photonEvent.Code;
        object[] o = (object[])photonEvent.CustomData;

        switch (e)
        {
            case EventCodes.NewPlayer:
                CreateController();
                break;

            case EventCodes.ChangeStat:
                //ChangeStat_R(o);
                break;

            case EventCodes.NewMatch:
                Launcher.Instance.StartGame();
                break;

            case EventCodes.RefreshTimer:
                //RefreshTimer_R(o);
                break;
        }
    }

    private void Awake()
    {
        Instance = this;
        PV = GetComponent<PhotonView>();
    }
    private void Start()
    {
        if (PV.IsMine)
            CreateController();
    }
    private void CreateController() 
    {
       Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint();
       controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnPoint.position, spawnPoint.rotation, 0, new object[] {PV.ViewID });
    }
    internal void Die(string s)
    {
        //onPlayerKilledCallback?.Invoke(killer, PV.Owner.NickName);
        PV.RPC("RPC_OnKilled", RpcTarget.All, s);
        PhotonNetwork.Destroy(controller);
        CreateController();
    }

    [PunRPC] private void RPC_OnKilled(string killer) 
    {
        StartCoroutine(TextKillFeedCur(killer));
    }
    private IEnumerator TextKillFeedCur(string killer) 
    {
        text.text = killer + " killed  " + PV.Owner.NickName;
        yield return new WaitForSeconds(3.5f);
        text.text = "";
    }
}