using UnityEngine;

[CreateAssetMenu(menuName = "NPC/NPC Profile")]
public class NPCProfile : ScriptableObject
{
    public string npcName;
    [TextArea] public string defaultDialogue;

    [Header("Reputation Reactions")]
    public string lowRepDialogue;
    public string highRepDialogue;
    public int reputationThreshold = 25;

    [Header("Quest Link")]
    public QuestData assignedQuest;
}