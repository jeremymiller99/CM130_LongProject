using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPCBehavior : MonoBehaviour
{
    public NPCProfile profile;
    public ReputationManager repManager;
    public QuestTrigger questTrigger;

    private void Start()
    {
        if (repManager == null)
            repManager = FindObjectOfType<ReputationManager>();

        if (profile.assignedQuest != null && questTrigger != null)
            questTrigger.questData = profile.assignedQuest;
    }

    public void Interact()
    {
        int rep = repManager.GetReputation();
        string dialogue = profile.defaultDialogue;

        if (rep < profile.reputationThreshold)
            dialogue = profile.lowRepDialogue;
        else if (rep >= profile.reputationThreshold)
            dialogue = profile.highRepDialogue;

        DialogueManager.Show($"<b>{profile.npcName}:</b> {dialogue}");

        if (questTrigger != null)
            questTrigger.ManualInteract();
    }

}