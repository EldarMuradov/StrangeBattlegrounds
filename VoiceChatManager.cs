using UnityEngine;
using agora_gaming_rtc;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class VoiceChatManager : MonoBehaviourPunCallbacks
{
    private readonly string appID = "8021b2d93f51442d9ad9c36ed96b039a";

    private IRtcEngine rtcEngine;

    public static VoiceChatManager Instance;
    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }
        else 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

    }

    private void Start()
    {
        rtcEngine = IRtcEngine.GetEngine(appID);
        rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccess;
        rtcEngine.OnLeaveChannel += OnLeaveChannel;
        rtcEngine.OnError += OnError;

        rtcEngine.EnableSoundPositionIndication(true);
    }

    private void OnError(int error, string msg)
    {
        Debug.LogError("Error with Agora: " + msg);
    }

    private void OnLeaveChannel(RtcStats stats)
    {
        Debug.Log("Leave channel with duration " + stats.duration);
    }

    private void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        Debug.Log("Joined channel: " + channelName);
        Hashtable hash = new Hashtable();
        hash.Add("agoraID", uid.ToString());
        PhotonNetwork.SetPlayerCustomProperties(hash);
    }

    public override void OnJoinedRoom()
    {
        rtcEngine.JoinChannel(PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnLeftRoom()
    {
        rtcEngine.LeaveChannel();
    }

    private void OnDestroy()
    {
        IRtcEngine.Destroy();
    }

    public IRtcEngine GetRtcEngine() 
    {
        return rtcEngine;
    }
}
