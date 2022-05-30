using Photon.Pun;
using TMPro;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField emailInput;

    private void Start()
    {
        var data = Data.LoadProfile();
        usernameInput.text = data.username;
        passwordInput.text = data.password;
        emailInput.text = data.email;
        PhotonNetwork.NickName = PlayerPrefs.GetString("username");
    }
   
    public void OnUsernameInputValueChanged() 
    {
        PhotonNetwork.NickName = usernameInput.text;
        PlayerPrefs.SetString("username", usernameInput.text);
        PlayerPrefs.SetString("password", passwordInput.text);
        Data.SaveProfile(new ProfileData(usernameInput.text, passwordInput.text, emailInput.text));
    }
}
