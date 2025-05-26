using UnityEngine;

[CreateAssetMenu(menuName = "Quest System/Quest")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea] public string description;

    public int requiredDay;
    public string requiredTimeZone;

    public bool autoStart = false;

    [Header("Reputation")]
    public int repRewardOnTime = 10;
    public int repRewardLate = 4;
}