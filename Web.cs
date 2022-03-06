using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Photon.Pun;

public class Web : MonoBehaviour
{
    [SerializeField] private DataManager dataManager;
    public TMP_InputField Name;
    public TMP_InputField Password;
    public TMP_InputField Email;
    public TMP_Text errorText;

    private IEnumerator GetUsers()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://localhost/ServerPHPCodeBase/GetUsers.php"))
        {
            yield return www.SendWebRequest();
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                byte[] results = www.downloadHandler.data;
            }
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    public void OnLoginButtonClicked() 
    {
        StartCoroutine(Login(Name.text, Password.text, Email.text));
    }
    public void OnRegisterButtonClicked()
    {
        StartCoroutine(Register(Name.text, Password.text, Email.text));
    }
    public IEnumerator Login(string username, string password, string email)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        form.AddField("email", email);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/ServerPHPCodeBase/LoginUser.php", form)) 
        {
            yield return www.SendWebRequest();
            if (www.isHttpError || www.isNetworkError)
            {
                errorText.text = "Login Failed: " + www.error;
                MenuManager.Instance.OpenMenu("error");
            }
            else 
            {
                Debug.Log(www.downloadHandler.text);
                if (www.downloadHandler.text == "Login Success")
                {
                    dataManager.OnUsernameInputValueChanged();
                    Launcher.Instance.OnLoggedIn();
                }
            }
        }
    }
    public IEnumerator Register(string username, string password, string email)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        form.AddField("email", email);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/ServerPHPCodeBase/RegisterUser.php", form))
        {
            yield return www.SendWebRequest();
            if (www.isHttpError || www.isNetworkError)
            {
                errorText.text = "Register Failed: " + www.error;
                MenuManager.Instance.OpenMenu("error");
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                if (www.downloadHandler.text == "Register Success")
                {
                    dataManager.OnUsernameInputValueChanged();
                    Launcher.Instance.OnLoggedIn();
                }
            }
        }
    }
}
