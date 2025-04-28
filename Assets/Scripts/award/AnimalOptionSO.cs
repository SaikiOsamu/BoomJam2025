using UnityEngine;

[CreateAssetMenu(fileName = "AnimalOptionSO", menuName = "ScriptableObject/AnimalOptionSO")]
public class AnimalOptionSO : ScriptableObject
{
    public string id;
    public string animalName;
    public string description;
    public Sprite icon;
}
