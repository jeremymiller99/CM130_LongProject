using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerController player);
    void OnGainFocus();   // Called when this becomes the current interactable
    void OnLoseFocus();   // Called when this stops being the current interactable
    GameObject GetGameObject();
    int GetPriority();    // Higher priority = more important (items might be 1, NPCs might be 2)
} 