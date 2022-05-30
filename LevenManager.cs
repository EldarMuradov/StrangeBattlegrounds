using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevenManager : MonoBehaviour
{
    #region Vars
    public RoomMode Mode;
    public delegate void OnTeamMatchDelegates();
    public OnTeamMatchDelegates onTeamMatchDelegates;
    public event OnTeamMatchDelegates OnTeamMatchEvent;
    public static List<PlayerManager> Managers = new List<PlayerManager>(16);
    
    public delegate void OnKilledDelegate(Teams team);
    public OnKilledDelegate onKilledDelegate;

    public static LevenManager Instance;
    public  int RedKills = 0;
    public  int BlueKills = 0;
    public Teams VictoryTeam;
    public int Players = 0;
    [SerializeField] private GameObject PanelBlue;
    [SerializeField] private GameObject PanelRed;
    [SerializeField] private TMP_Text _BWB;
    [SerializeField] private TMP_Text _RWR;
    [SerializeField] private TMP_Text _matchStatsText;

    #endregion

    #region UnityMethods
    private void Awake()
    {
        Instance = this;
        OnTeamMatchEvent += onTeamMatchDelegates;
        onTeamMatchDelegates += OnMatchEnded;   
    }
    private void Start()
    {
        if (Instance != this)
            Destroy(Instance);
    }
    #endregion

    #region OnEvents
    /// <summary>
    /// This method calleds by event, when one player die
    /// </summary>
    /// <param name="team"></param>

    public void CheckHandler(out bool exit) 
    {
        _matchStatsText.text = RedKills.ToString() + "/" + BlueKills.ToString();
        if (RedKills == 15)
        {
            Debug.Log("Red team Wictory");
            VictoryTeam = Teams.RED;
            onTeamMatchDelegates?.Invoke();
            Debug.Log("CH");
            exit = true;
        }

        else if (BlueKills == 15)
        {
            Debug.Log("Blue team Wictory");
            VictoryTeam = Teams.BLUE;
            onTeamMatchDelegates?.Invoke();
            Debug.Log("CH");
            exit = true;
        }
        else
            exit = false;
    }
    private void OnMatchEnded() 
    {
        if (VictoryTeam == Teams.RED)
        {
            PanelRed.SetActive(true);
        }
        else 
        {
            PanelBlue.SetActive(true);
        }
    }

    public short ScoreCounter(ushort k, ushort d, bool b) 
    {
        short score;
        if (b)
        {
            score = 10;
            if (d == 0)
                score += (short)(k * 3);
            else
                score += (short)((k * 3) / d);
        }
        else
        {
            score = -10;
            if (k > d)
                score += (short)((k / 2) + d);
            else
                score -= (short)(d - k);
        }

        return score;
    }
    #endregion
}
