using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCreatorManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField nameInput;
    public TMP_InputField surnameInput;
    public TMP_Dropdown sexDropdown;
    public TMP_Dropdown religionDropdown;
    public TMP_InputField powerNameInput;
    public TMP_InputField powerDescriptionInput;
    
    [Header("Background Class Buttons")]
    public Button luckyBastardButton;
    public Button problemChildButton;
    public Button mediatorButton;
    public Button easyTriggerButton;
    
    [Header("Control Buttons")]
    public Button createCharacterButton;
    public Button backButton;  // NEW
    
    [Header("Panels")]
    public GameObject characterCreatorPanel;
    public GameObject previousPanel; // Where to go when Back is pressed
    
    // Track selected background
    private string selectedBackground = "";
    
    void Start()
    {
        // Background button listeners
        luckyBastardButton.onClick.AddListener(() => SelectBackground("Lucky Bastard"));
        problemChildButton.onClick.AddListener(() => SelectBackground("Problem Child"));
        mediatorButton.onClick.AddListener(() => SelectBackground("Mediator"));
        easyTriggerButton.onClick.AddListener(() => SelectBackground("Easy Trigger"));
        
        // Control button listeners
        createCharacterButton.onClick.AddListener(CreateAndSaveCharacter);
        backButton.onClick.AddListener(GoBack);
    }
    
    void SelectBackground(string backgroundName)
    {
        selectedBackground = backgroundName;
        Debug.Log($"Selected background: {backgroundName}");
        
        // Optional: Visual feedback - change button colors
        ResetButtonColors();
        HighlightSelectedButton(backgroundName);
    }
    
    void ResetButtonColors()
    {
        // Reset all buttons to normal color
        ColorBlock colors = luckyBastardButton.colors;
        colors.normalColor = Color.white;
        
        luckyBastardButton.colors = colors;
        problemChildButton.colors = colors;
        mediatorButton.colors = colors;
        easyTriggerButton.colors = colors;
    }
    
    void HighlightSelectedButton(string backgroundName)
    {
        Button selectedButton = null;
        
        switch (backgroundName)
        {
            case "Lucky Bastard": selectedButton = luckyBastardButton; break;
            case "Problem Child": selectedButton = problemChildButton; break;
            case "Mediator": selectedButton = mediatorButton; break;
            case "Easy Trigger": selectedButton = easyTriggerButton; break;
        }
        
        if (selectedButton != null)
        {
            ColorBlock colors = selectedButton.colors;
            colors.normalColor = Color.green; // Highlight selected
            selectedButton.colors = colors;
        }
    }
    
    void CreateAndSaveCharacter()
    {
        // Validate inputs
        if (string.IsNullOrEmpty(nameInput.text))
        {
            Debug.LogError("Name is required!");
            return;
        }
        
        if (string.IsNullOrEmpty(selectedBackground))
        {
            Debug.LogError("Please select a background class!");
            return;
        }
        
        // Create character
        CharacterData newCharacter = new CharacterData();
        
        // Fill basic info
        newCharacter.characterName = nameInput.text;
        newCharacter.surname = surnameInput.text;
        newCharacter.sex = sexDropdown.options[sexDropdown.value].text;
        newCharacter.religion = religionDropdown.options[religionDropdown.value].text;
        newCharacter.backgroundTrait = selectedBackground; // Use button selection
        newCharacter.powerName = powerNameInput.text;
        newCharacter.powerDescription = powerDescriptionInput.text;
        
        // Apply background bonuses
        ApplyBackgroundBonuses(newCharacter);
        
        // Save to JSON
        SaveCharacterToJSON(newCharacter);
        
        Debug.Log($"Character {newCharacter.characterName} created!");
    }
    
    void ApplyBackgroundBonuses(CharacterData character)
    {
        switch (character.backgroundTrait)
        {
            case "Lucky Bastard":
                character.luck += 2;
                character.strength -= 1;
                character.money *= 1.5f;
                break;
                
            case "Problem Child":
                character.strength += 2;
                character.agility -= 1;
                character.money *= 0.8f;
                break;
                
            case "Mediator":
                character.money *= 1.2f;
                break;
                
            case "Easy Trigger":
                character.speed += 2;
                character.strength -= 1;
                character.money *= 0.9f;
                break;
        }
    }
    
    void SaveCharacterToJSON(CharacterData character)
    {
        string jsonString = JsonUtility.ToJson(character, true);
        string fileName = character.characterName + "_" + character.surname + ".json";
        string savePath = Application.persistentDataPath + "/" + fileName;
        
        System.IO.File.WriteAllText(savePath, jsonString);
        
        Debug.Log($"Character saved to: {savePath}");
        Debug.Log($"JSON: {jsonString}");
    }
    
    void GoBack()
    {
        characterCreatorPanel.SetActive(false);
        previousPanel.SetActive(true);
        Debug.Log("Returning to previous panel");
    }
}