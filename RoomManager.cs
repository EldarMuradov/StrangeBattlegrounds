using UnityEngine;
using Photon.Pun;
using System.IO;
using System.Collections.Generic;
using Photon.Realtime;

public enum Teams 
{
    RED,
    BLUE,
    GREEN,
    YELLOW,
    UNSIGNED
}
public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    public GameObject obj;
    public GameObject obj2;
    public GameObject obj3;
    private PhotonView PV;

    
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        if (Instance) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        obj = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity) as GameObject;
        if (GameStateManager.Instance.Mode == RoomMode.Solo)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject obj1 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BotManager"), Vector3.zero, Quaternion.identity) as GameObject;
            }
            //obj2 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BotManager"), Vector3.zero, Quaternion.identity) as GameObject;
            //obj3 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BotManager"), Vector3.zero, Quaternion.identity) as GameObject;
            /*if (LevenManager.Instance.Mode == RoomMode.TDM)
            {
                RPC_SetHandler();
                //PV.RPC("RPC_SetPlayer", RpcTarget.All);
            }*/
        }
    }
}
