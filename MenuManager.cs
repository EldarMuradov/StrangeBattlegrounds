using Photon.Pun;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField] private Menu[] menus;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenMenu(string menuName) 
    {
        for (int i = 0; i < menus.Length; i++) 
        {
            if (menus[i].MenuName == menuName)
            {
                menus[i].Open();
            }
            else if(menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }

    public void OpenMenuAndExitRoom(Menu menu) 
    {
        PhotonNetwork.LeaveRoom();
        OpenMenu(menu);

    }
    public void OpenMenu(Menu menu) 
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
        menu.Open();
    }

    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }
}
