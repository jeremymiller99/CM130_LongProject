using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    
    [Header("Properties")]
    public float weight = 1f;
    
    [Header("Quest Link")]
    public string associatedQuestName;
    
    // Optional VFX/SFX
    public GameObject pickupEffect;
    public AudioClip pickupSound;
}