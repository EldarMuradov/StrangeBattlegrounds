using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;

public class Web : MonoBehaviour
{
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private TMP_Text _loadingText;

    private AsyncOperation _loadingSceneOperation;

    public TMP_InputField Name;
    public TMP_InputField Password;
    public TMP_InputField Email;
    public TMP_Text ErrorText;

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

    public void ExitGame() => Application.Quit();
    
    public void OnLoginButtonClicked() => StartCoroutine(Login(Name.text, Password.text, Email.text));
    
    public void OnRegisterButtonClicked() => StartCoroutine(Register(Name.text, Password.text, Email.text));
    
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
                ErrorText.text = "Login Failed: " + www.error;
                MenuManager.Instance.OpenMenu("error");
            }
            else 
            {
                Debug.Log(www.downloadHandler.text);
                if (www.downloadHandler.text != "Username doesn't exist" && www.downloadHandler.text != "Login failed")
                {
                    PlayerPrefs.SetString("id", www.downloadHandler.text);
                    LoginNameManager.Instance.OnLoggedIn();
                    StartCoroutine(LoadSceneCur(1));
                }
                else
                {
                    MenuManager.Instance.OpenMenu("error");
                    ErrorText.text = "Login Failed: " + www.downloadHandler.text;
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
                ErrorText.text = "Register Failed: " + www.error;
                MenuManager.Instance.OpenMenu("error");
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                if (www.downloadHandler.text == "Register Success")
                {
                    LoginNameManager.Instance.OnLoggedIn();
                    StartCoroutine(LoadSceneCur(1));
                }
                else 
                {
                    MenuManager.Instance.OpenMenu("error");
                    ErrorText.text = "Register Failed: " + www.downloadHandler.text;
                }
            }
        }
    }
    
    public IEnumerator LoadSceneCur(int _sceneID)
    {
        _loadingPanel.SetActive(true);
        yield return new WaitForSeconds(1f);
        _loadingSceneOperation = SceneManager.LoadSceneAsync(_sceneID);
        while (!_loadingSceneOperation.isDone)
        {

            _loadingText.text = "Loading... " + ((int)_loadingSceneOperation.progress * 100).ToString();
            yield return 0;
        }
        _loadingPanel.SetActive(false);
        _loadingText.text = "";
        yield return new WaitForSeconds(1f);
    }
}
