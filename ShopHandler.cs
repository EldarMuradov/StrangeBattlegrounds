using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ShopHandler : MonoBehaviour
{
    public GameObject List;
    public static List<GameObject> Items = new List<GameObject>();
    private void OnEnable()
    {
        _createItemsCallback = (jsonArray) => 
        {
            StartCoroutine(CreateItemsRoutine(jsonArray));    
        };
        CreateItems();
    }
    private Action<string> _createItemsCallback;
    private IEnumerator CreateItemsRoutine(string jsonArrayString) 
    {
        JSONArray jsonArray = JSON.Parse(jsonArrayString) as JSONArray;
        for (int i = 0; i < jsonArray.Count; i++)
        {
            bool isDone = false;
            string itemId = jsonArray[i].AsObject["id"];
            JSONClass itemInfoJson = new JSONClass();
            Action<string> getItemInfoCallback = (itemInfo) =>
            {
                isDone = true;
                JSONArray tempArray = JSON.Parse(itemInfo) as JSONArray;
                itemInfoJson = tempArray[0].AsObject;
            };
            StartCoroutine(GetItem(itemId, getItemInfoCallback));
            yield return new WaitUntil(() => isDone == true);
            GameObject item = Instantiate(Resources.Load("Prefabs/Menu/ShopItem") as GameObject, List.transform.position, List.transform.rotation);
            item.transform.parent = List.transform;
            item.transform.localScale = Vector3.one;
            Items.Add(item);
            item.transform.Find("Name").GetComponent<TMP_Text>().text = itemInfoJson["name"];
            item.transform.Find("Price").GetComponent<TMP_Text>().text = itemInfoJson["price"];
            item.transform.Find("Description").GetComponent<TMP_Text>().text = itemInfoJson["description"];
        }
    }
    private void CreateItems() 
    {
        StartCoroutine(GetItemsIds(_createItemsCallback));
    }
    private IEnumerator GetItemsIds( Action<string> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://localhost/ServerPHPCodeBase/GetItemsIds.php"))
        {
            yield return www.SendWebRequest();
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                string jsonArray = www.downloadHandler.text;

                callback(jsonArray);
            }
        }
    }
    private IEnumerator GetItem(string itemId, Action<string> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", itemId);
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/ServerPHPCodeBase/GetItem.php", form))
        {
            yield return www.SendWebRequest();
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                string jsonArray = www.downloadHandler.text;

                callback(jsonArray);
            }
        }
    }
}
