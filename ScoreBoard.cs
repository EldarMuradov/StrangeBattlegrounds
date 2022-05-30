using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class ScoreBoard : MonoBehaviourPunCallbacks
{
    /*[SerializeField] private Transform _container;
    [SerializeField] private GameObject _scoreBoardItemPrefab;

    private Dictionary<Player, ScoreBoardItem> _scoreboardItem = new Dictionary<Player, ScoreBoardItem>();

    private void Start()
    {
        foreach (Player player in PhotonNetwork.PlayerList) 
        {
            AddScoreBoardItem(player);        
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddScoreBoardItem(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RemoveScoreBoardItem(otherPlayer);
    }
    private void AddScoreBoardItem(Player player)
    {
        ScoreBoardItem item = Instantiate(_scoreBoardItemPrefab, _container).GetComponent<ScoreBoardItem>();
        item.Initialize(player);
        _scoreboardItem[player] = item;
    }

    private void RemoveScoreBoardItem(Player player) 
    {
        Destroy(_scoreboardItem[player].gameObject);
        _scoreboardItem.Remove(player);
    }*/
}
