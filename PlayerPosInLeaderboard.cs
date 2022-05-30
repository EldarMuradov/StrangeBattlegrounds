using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerPosInLeaderboard : MonoBehaviour
{
    public TMP_Text PosText;

    private void Start()
    {
        StartCoroutine(GetUserInfo());
    }
    public IEnumerator GetUserInfo()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", Launcher.Instance.Name);
        using (UnityWebRequest www = UnityWebRequest.Post($"http://localhost/ServerPHPCodeBase/GetPosInLeaderboard.php", form))
        {
            yield return www.SendWebRequest();
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                PosText.text = www.downloadHandler.text;
                Debug.Log(www.downloadHandler.text);
                byte[] results = www.downloadHandler.data;
            }
        }
    }
}
