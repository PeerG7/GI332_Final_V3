using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Character System/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite characterIcon; // รูปที่จะโชว์ในช่อง Pic
    public GameObject playerPrefab; // Prefab ตัวละครที่จะเกิดในเกม
}