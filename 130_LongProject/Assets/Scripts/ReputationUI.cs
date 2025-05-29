using TMPro;
using UnityEngine;

public class ReputationUI : MonoBehaviour
{
    public TMP_Text reputationText;
    
    private ReputationManager reputationManager;

    void Start()
    {
        reputationManager = FindObjectOfType<ReputationManager>();
    }

    void Update()
    {
        if (reputationManager == null || reputationText == null) return;
        
        int currentRep = reputationManager.GetReputation();
        reputationText.text = "Reputation: " + currentRep.ToString();
    }
} 