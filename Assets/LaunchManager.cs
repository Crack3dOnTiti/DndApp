using UnityEngine;
using UnityEngine.UI;

public class LaunchManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject launchPanel;
    public GameObject hostPanel;
    public GameObject playerQueryPanel;
    public GameObject playerCreatePanel;

    [Header("Buttons")]
    public Button hostButton;
    public Button playerButton;
    public Button createPlayerButton;

    void Start()
    {
        launchPanel.SetActive(true);
        hostPanel.SetActive(false);
        playerQueryPanel.SetActive(false);
        playerCreatePanel.SetActive(false);

        hostButton.onClick.AddListener(StartAsHost);
        playerButton.onClick.AddListener(StartPlayerQuery);
        createPlayerButton.onClick.AddListener(StartPlayerCreator);
    }

    void StartAsHost()
    {
        launchPanel.SetActive(false);
        hostPanel.SetActive(true);
    }
    void StartPlayerQuery()
    {
        launchPanel.SetActive(false);
        playerQueryPanel.SetActive(true);
    }

    void StartPlayerCreator()
    {
        playerQueryPanel.SetActive(false);
        playerCreatePanel.SetActive(true);
    }
}