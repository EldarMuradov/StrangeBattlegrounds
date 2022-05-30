using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginNameManager : MonoBehaviourPunCallbacks
{
    public static LoginNameManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        //DontDestroyOnLoad(gameObject);
        Debug.Log("Connecting to master");
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = PlayerPrefs.GetString("username");
        MenuManager.Instance.OpenMenu("login");
    }
    public void OnLoggedIn()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        PhotonNetwork.NickName = PlayerPrefs.GetString("username");
    }
}
