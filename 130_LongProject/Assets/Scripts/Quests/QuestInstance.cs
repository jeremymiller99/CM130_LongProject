using UnityEngine;

[System.Serializable]
public class QuestInstance
{
    public QuestData data;
    public QuestState state = QuestState.Inactive;
    private bool wasStartedLate = false;
    public bool taskCompleted = false;

    public void TryActivate(int currentDay, string timeZone)
    {
        if (state == QuestState.Inactive &&
            currentDay == data.requiredDay &&
            timeZone == data.requiredTimeZone)
        {
            state = data.autoStart ? QuestState.Active : QuestState.Available;
            Debug.Log($"Quest Available: {data.questName}");
        }
    }

    public void StartQuest(int currentDay, string timeZone)
    {
        if (state == QuestState.Available)
        {
            wasStartedLate = currentDay > data.requiredDay || timeZone != data.requiredTimeZone;
            state = QuestState.Active;
            Debug.Log($"Started quest: {data.questName} (Late: {wasStartedLate})");
        }
    }

    public void CompleteQuest(ReputationManager repManager)
    {
        if (state == QuestState.Active && taskCompleted)
        {
            state = QuestState.Completed;
            int rep = wasStartedLate ? data.repRewardLate : data.repRewardOnTime;
            repManager.AddReputation(rep);
            Debug.Log($"Completed quest: {data.questName} | Rep: {rep}");
        }
        else if (state == QuestState.Active && !taskCompleted)
        {
            Debug.Log($"Quest '{data.questName}' is active, but task not completed.");
        }
    }

    public bool MarkTaskAsComplete()
    {
        if (state == QuestState.Active && !taskCompleted)
        {
            taskCompleted = true;
            return true;
        }
        return false;
    }
}