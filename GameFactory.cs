using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameFactory : GameManager
{
    #region Fields
    public static GameFactory Instance;
    public TMP_Text text;
    public GameObject panel;
    public int LevelID;
    private void Awake() => Instance = this;
    public Level level;
    public List<Enemy> Enemies = new List<Enemy>(1001);
    public GameObject[] ElseLevels;
    public GameObject Canvas;
    public GameObject UpdateManager;
    #endregion



    #region LoadBase
    public void OnGameStarted(int i)
    {
        foreach (GameObject obj in ElseLevels)
            obj.SetActive(false);
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(Canvas);
        StartCoroutine(LoadSceneCur(i, text, panel, Enemies, level));
    }
    public void OnEnteredMultiplayerMode()
    {
        foreach (GameObject obj in ElseLevels)
            obj.SetActive(false);
        MenuManager.Instance.OpenMenu("singleplayermap");
    }
    public void OnExitMultiplayerMode()
    {
        foreach (GameObject obj in ElseLevels)
            obj.SetActive(true);
        MenuManager.Instance.OpenMenu("title");
    }
    public void GameInLabLoad() => StartCoroutine(LoadSceneCur(2, text, panel, Enemies, level));
    public void FirstLevelLoad() => StartCoroutine(LoadSceneCur(3, text, panel, Enemies, level));
    public void SecondLevelLoad() => StartCoroutine(LoadSceneCur(4, text, panel, Enemies, level));
    public void ThirdLevelLoad() => StartCoroutine(LoadSceneCur(5, text, panel, Enemies, level));
    public void ShelterLoadIn() => StartCoroutine(LoadSceneCur(6, text, panel, Enemies, level));
    public void ShelterToCityLoadOut() => StartCoroutine(LoadSceneCur(7, text, panel, Enemies, level));
    public void SecondLabLoad() => StartCoroutine(LoadSceneCur(8, text, panel, Enemies, level));
    public void LastRoomLoad() => StartCoroutine(LoadSceneCur(9, text, panel, Enemies, level));
    public void LastLabLoad() => StartCoroutine(LoadSceneCur(10, text, panel, Enemies, level));
    #endregion
}
