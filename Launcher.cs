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
    #region Vars
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
    public int SceneNum = 1;
    private AsyncOperation loadingSceneOperation;
    public GameObject UpdateManager;
    public RoomMode Mode;
    public string Name;
    public TMP_Text NameTxt;
    public TMP_Text InfoTxt;
    public GameObject MainMenuObj;
    public GameObject StatsObj;
    public GameObject Skins;
    public GameObject UserProfile;
    #endregion
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        Name = PlayerPrefs.GetString("username");
        PhotonNetwork.Disconnect();
        Debug.Log("Connecting to master");
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = Name;
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        MainMenuObj.SetActive(true);
        panel.SetActive(false);
    }
    public void CreateRoom() 
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        if (Mode == RoomMode.Solo)
        {
            GameStateManager.Instance.Mode = Mode;
            PhotonNetwork.CreateRoom(roomNameInputField.text);
        }

        if (Mode == RoomMode.TDM)
        {
            GameStateManager.Instance.Mode = Mode;
            PhotonNetwork.CreateRoom(roomNameInputField.text);
        }

        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        if (Mode == RoomMode.Solo)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
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
        PhotonNetwork.CurrentRoom.IsVisible = false;
        panel.SetActive(true);
        text.text = "Loading...";
        PhotonNetwork.LoadLevel(SceneNum);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    public void ExitToLog()
    {
        PhotonNetwork.Disconnect();
        StartCoroutine(LoadSceneCur(0));
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
        PhotonNetwork.LoadLevel(1);
        MenuManager.Instance.OpenMenu("title");
    }
    private GameObject _item;
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
            _item = Instantiate(roomListPrefab, roomListContent) as GameObject;
            _item.GetComponent<RoomListItem>().SetUp(roomList[i]);
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

            text.text = "Loading... " + ((int)loadingSceneOperation.progress * 100).ToString();
            yield return 0;
        }
        //panel.SetActive(false);
        //text.text = "";
        //yield return new WaitForSeconds(1f);
    }

    public void ChangeMap(int id) 
    {
        SceneNum = id;
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

    public IEnumerator GetUserInfo()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", Name);
        using (UnityWebRequest www = UnityWebRequest.Post($"http://localhost/ServerPHPCodeBase/GetUserInfo.php", form))
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
                InfoTxt.text = www.downloadHandler.text;
            }
        }
    }
}
