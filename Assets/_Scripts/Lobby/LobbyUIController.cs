using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUIController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject TitlePanel;

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        TitlePanel.SetActive(false);
    }

    public void ShowTitle()
    {
        mainMenuPanel.SetActive(false);
        TitlePanel.SetActive(true);
    }

    void Start()
    {
      ShowTitle();  
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
