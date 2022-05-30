using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfo : MonoBehaviour
{
    public string UserId;
    public string _userName;
    public string _userLevel;
    public string _userSBCs;
    public string _userScore;
    public static UserInfo Instance;
    private void Awake()
    {
        if (Instance)
            Destroy(gameObject);
        Instance = this;
        _userName = PlayerPrefs.GetString("username");
        UserId = PlayerPrefs.GetString("id");
    }
}
