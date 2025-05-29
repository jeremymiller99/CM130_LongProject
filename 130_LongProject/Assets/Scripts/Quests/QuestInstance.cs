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
        if (state != QuestState.Inactive) return;

        if (currentDay == data.requiredDay && timeZone == data.requiredTimeZone)
        {
            state = data.autoStart ? QuestState.AvailableActive : QuestState.AvailableInactive;
            Debug.Log($"Quest Available: {data.questName} (State: {state})");
        }
    }

    public void StartQuest(int currentDay, string timeZone)
    {
        if (state != QuestState.AvailableInactive) return;

        wasStartedLate = currentDay > data.requiredDay || timeZone != data.requiredTimeZone;
        state = QuestState.AvailableActive;
        Debug.Log($"Started quest: {data.questName} (Late: {wasStartedLate})");
    }

    public void CompleteQuest(ReputationManager repManager)
    {
        if (state != QuestState.AvailableActive || !taskCompleted || state == QuestState.Completed) 
        {
            Debug.LogWarning($"Cannot complete quest {data.questName}: Invalid state ({state}) or task not completed ({taskCompleted})");
            return;
        }

        state = QuestState.Completed;
        int rep = wasStartedLate ? data.repRewardLate : data.repRewardOnTime;
        repManager.AddReputation(rep);
        Debug.Log($"Completed quest: {data.questName} | Rep: {rep}");
    }

    public bool MarkTaskAsComplete()
    {
        if (state != QuestState.AvailableActive || taskCompleted) return false;

        taskCompleted = true;
        Debug.Log($"Task completed for quest: {data.questName}");
        return true;
    }

    public bool IsAvailable() => state == QuestState.AvailableInactive || state == QuestState.AvailableActive;
    public bool IsActive() => state == QuestState.AvailableActive;
    public bool IsCompleted() => state == QuestState.Completed;
}