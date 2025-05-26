using UnityEngine;

public class ReputationManager : MonoBehaviour
{
    public int currentReputation = 0;

    public void AddReputation(int amount)
    {
        currentReputation += amount;
        Debug.Log($"Reputation changed: {amount} | New total: {currentReputation}");
    }

    public int GetReputation() => currentReputation;
}