using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugTimeUI : MonoBehaviour
{
    public TMP_Text timeText;
    public TMP_Text questText;
    public TMP_Text questStatusText;
    public Button addHourButton;

    private DayNightCycleManager timeManager;
    private QuestManager questManager;

    void Start()
    {
        timeManager = FindObjectOfType<DayNightCycleManager>();
        questManager = FindObjectOfType<QuestManager>();

        addHourButton.onClick.AddListener(() => AddHours(1));
    }

    void Update()
    {
        if (timeManager == null || questManager == null) return;

        // Display current time and period
        string zone = timeManager.GetCurrentTimeZone();
        int day = timeManager.GetCurrentDay();

        float realSecondsPerDay = timeManager.realSecondsPerDay;
        float normalizedTime = timeManager.GetCurrentTimeOfDay();
        float hour24 = normalizedTime * 24f;
        int hour = Mathf.FloorToInt(hour24);
        int minute = Mathf.FloorToInt((hour24 - hour) * 60);
        string ampm = hour >= 12 ? "PM" : "AM";
        int hour12 = hour % 12;
        if (hour12 == 0) hour12 = 12;

        timeText.text = $"<b>Day:</b> {day}\n<b>Time:</b> {hour12:00}:{minute:00} {ampm}\n<b>Zone:</b> {zone}";

        // Show all active quests
        string quests = "";
        foreach (var quest in questManager.GetAllQuests())
        {
            if (quest.state == QuestState.AvailableActive)
                quests += $"• {quest.data.questName} (active)\n";
            else if (quest.state == QuestState.AvailableInactive)
                quests += $"• {quest.data.questName} (available)\n";
        }

        questText.text = $"<b>Quests:</b>\n{quests}";
        
        // Update quest status text
        UpdateQuestStatus();
    }

    void AddHours(float hours)
    {
        float secondsToAdd = (hours / 24f) * timeManager.realSecondsPerDay;
        Time.timeScale = 100f; // Optional: increase time scale for speed-up effect
        timeManager.SkipTime(secondsToAdd);
    }

    private void UpdateQuestStatus()
    {
        if (questStatusText == null) return;
        
        questStatusText.text = "Quest Status:\n";
        foreach (QuestInstance quest in questManager.GetAllQuests())
        {
            string state = quest.state switch
            {
                QuestState.Inactive => "Inactive",
                QuestState.AvailableInactive => "Available",
                QuestState.AvailableActive => "Active",
                QuestState.Completed => "Completed",
                QuestState.Failed => "Failed",
                _ => "Unknown"
            };
            questStatusText.text += $"{quest.data.questName}: {state}\n";
        }
    }
}
