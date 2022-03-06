using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    [SerializeField] private GameObject Canvas;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text text;
    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private GameObject roomListPrefab;
    [SerializeField] private GameObject playerListPrefab;
    [SerializeField] private Transform roomListContent;
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject startButton;
    private AsyncOperation loadingSceneOperation;
    public GameObject UpdateManager;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        DontDestroyOnLoad(Canvas);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        MenuManager.Instance.OpenMenu("login");
        Debug.Log("Connecting to master");
        PhotonNetwork.ConnectUsingSettings();
    }
    public void OnLoggedIn() 
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
        //TODO: authorization

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("title");
        Debug.Log("Joined lobby");
    }

    public void CreateRoom() 
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent) 
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        startButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void StartGame() 
    {
        PhotonNetwork.LoadLevel(1);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ExitGame()
    {
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu("error");
    }
 
    public void JoinRoom(RoomInfo info) 
    {

        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("loading");
    }
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(0);
        MenuManager.Instance.OpenMenu("title");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(roomListPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }

    public IEnumerator LoadSceneCur(int _sceneID)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(1f);
        loadingSceneOperation = SceneManager.LoadSceneAsync(_sceneID);
        while (!loadingSceneOperation.isDone)
        {
            text.text = "Loading... " + loadingSceneOperation.progress.ToString();
            yield return 0;
        }
        panel.SetActive(false);
        text.text = "";
        yield return new WaitForSeconds(1f);
    }

    public void CheckLevel(int i) 
    {
        FindObjectOfType<GameFactory>().OnGameStarted(i);
    }

    public TMP_Text textleaders;
    private IEnumerator GetUsers()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://localhost/ServerPHPCodeBase/Leaderboard.php"))
        {
            yield return www.SendWebRequest();
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                textleaders.text = www.downloadHandler.text;
                Debug.Log(www.downloadHandler.text);
                byte[] results = www.downloadHandler.data;
            }
        }
    }

    public void GetLeaders() 
    {
        MenuManager.Instance.OpenMenu("leaderboard");
        StartCoroutine(GetUsers());
    }
}
