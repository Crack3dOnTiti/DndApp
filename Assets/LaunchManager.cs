using UnityEngine;
using UnityEngine.UI;

public class LaunchManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject launchPanel;
    public GameObject hostPanel;
    public GameObject playerQueryPanel;
    public GameObject playerCreatePanel;
    public GameObject playerConnectionPanel;

    [Header("Buttons")]
    public Button hostButton;
    public Button playerButton;
    public Button createPlayerButton;
    public Button createPlayerCreationFinishedButton;
    public Button createPlayerLoadedButton;
    public Button returnFromHostButton;

    void Start()
    {
        HideAllPanels(true);

        hostButton.onClick.AddListener(StartAsHost);
        playerButton.onClick.AddListener(StartPlayerQuery);
        createPlayerButton.onClick.AddListener(StartPlayerCreator);
        createPlayerCreationFinishedButton.onClick.AddListener(StartPlayerConnection);
        createPlayerLoadedButton.onClick.AddListener(StartPlayerConnection);
        returnFromHostButton.onClick.AddListener(StartMenu);
    }

    void HideAllPanels(bool start)
    {
        if (start)
        {
            launchPanel.SetActive(true);
            hostPanel.SetActive(false);
            playerQueryPanel.SetActive(false);
            playerCreatePanel.SetActive(false);
            playerConnectionPanel.SetActive(false);
        }
        else
        {
            launchPanel.SetActive(false);
            hostPanel.SetActive(false);
            playerQueryPanel.SetActive(false);
            playerCreatePanel.SetActive(false);
            playerConnectionPanel.SetActive(false);

        }
    }
    void StartAsHost()
    {
        HideAllPanels(false);
        hostPanel.SetActive(true);
    }
    void StartPlayerQuery()
    {
        HideAllPanels(false);
        playerQueryPanel.SetActive(true);
    }

    void StartPlayerCreator()
    {
        HideAllPanels(false);
        playerCreatePanel.SetActive(true);
    }

    void StartPlayerConnection()
    {
        HideAllPanels(false);
        playerConnectionPanel.SetActive(true);
    }

    void StartMenu()
    {
        HideAllPanels(false);
        launchPanel.SetActive(true);
    }
}