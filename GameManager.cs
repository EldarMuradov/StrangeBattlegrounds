using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoCache
{
    protected AsyncOperation loadingSceneOperation;
    public IEnumerator LoadSceneCur(int _sceneID, TMP_Text text, GameObject panel, IList Enemies, Level level)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(1f);
        loadingSceneOperation = SceneManager.LoadSceneAsync(_sceneID);
        while (!loadingSceneOperation.isDone)
        {
            text.text = "Loading... " + loadingSceneOperation.progress.ToString();
            yield return 0;
        }
        panel.SetActive(false);
        text.text = "";
        yield return new WaitForSeconds(1f);
    }
}
