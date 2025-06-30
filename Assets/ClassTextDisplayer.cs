using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassTextDisplayer : MonoBehaviour
{
    [Header("Buttons")]
    public Button Lucky;
    public Button Problem;
    public Button Mediator;
    public Button Easy;

    // [Header("Texts")]
    // public TextMeshProUGUI LuckyText;
    // public TextMeshProUGUI ProblemText;
    // public TextMeshProUGUI MediatorText;
    // public TextMeshProUGUI EasyText;
    [Header("Texts")]
    public GameObject LuckyText;
    public GameObject ProblemText;
    public GameObject MediatorText;
    public GameObject EasyText;

    void Start()
    {
        HideAllText();
        Lucky.onClick.AddListener(ShowLuckyText);
        Problem.onClick.AddListener(ShowProblemText);
        Mediator.onClick.AddListener(ShowMediatorText);
        Easy.onClick.AddListener(ShowEasyText);
    }
    
    void ShowLuckyText()
    {
        HideAllText();
        LuckyText.SetActive(true);
    }
    
    void ShowProblemText()
    {
        HideAllText();
        ProblemText.SetActive(true);

    }
    
    void ShowMediatorText()
    {
        HideAllText();
        MediatorText.SetActive(true);
    }
    
    void ShowEasyText()
    {
        HideAllText();
        EasyText.SetActive(true);
    }
    
    void HideAllText()
    {
        LuckyText.SetActive(false);
        ProblemText.SetActive(false);
        MediatorText.SetActive(false);
        EasyText.SetActive(false);
    }
}
