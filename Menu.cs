using UnityEngine;

public class Menu : MonoBehaviour
{
    public string MenuName;
    public bool open;
    public void Open() 
    {
        open = true;
        gameObject.SetActive(true);
    }

    public void Close() 
    {
        open = false;
        gameObject.SetActive(false);
    }
}
