using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData 
{
    // Basic info from your UI
    public string characterName;
    public string surname;
    public string sex;
    public string religion;
    public string backgroundTrait;
    public string powerName;
    public string powerDescription;
    
    // Default stats (we'll add these automatically)
    public int age = 16;
    public int health = 100;
    public float money = 100.0f;
    public int strength = 10;
    public int luck = 10;
    public int agility = 10;
    public int speed = 10;
    
    // Constructor (runs when character is created)
    public CharacterData()
    {
        // Set any default values here
    }
}