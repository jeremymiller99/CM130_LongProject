using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public ReputationManager repManager;
    private DayNightCycleManager timeManager;
    private List<QuestInstance> allQuests = new List<QuestInstance>();

    void Start()
    {
        timeManager = FindObjectOfType<DayNightCycleManager>();
        repManager = FindObjectOfType<ReputationManager>();

        timeManager.OnMorning.AddListener(CheckQuests);
        timeManager.OnAfternoon.AddListener(CheckQuests);
        timeManager.OnEvening.AddListener(CheckQuests);
        timeManager.OnNight.AddListener(CheckQuests);
    }

    public void RegisterQuest(QuestInstance quest)
    {
        allQuests.Add(quest);
    }

    public void CheckQuests()
    {
        int day = GetCurrentDay();
        string zone = GetCurrentTimeZone();

        foreach (var quest in allQuests)
            quest.TryActivate(day, zone);
    }

    public int GetCurrentDay() => timeManager.GetCurrentDay();
    public string GetCurrentTimeZone() => timeManager.GetCurrentTimeZone();

    public bool MarkTaskAsComplete(string questName)
    {
        foreach (var quest in allQuests)
        {
            if (quest.data.questName == questName && quest.state == QuestState.Active)
            {
                return quest.MarkTaskAsComplete();
            }
        }
        return false;
    }
    public List<QuestInstance> GetAllQuests()
    {
        return allQuests;
    }
}