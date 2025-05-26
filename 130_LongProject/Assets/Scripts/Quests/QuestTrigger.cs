using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public QuestData questData;
    private QuestInstance runtimeQuest;
    private QuestManager manager;

    private void Start()
    {
        manager = FindObjectOfType<QuestManager>();
        runtimeQuest = new QuestInstance { data = questData };
        manager.RegisterQuest(runtimeQuest);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && runtimeQuest.state == QuestState.Available)
            runtimeQuest.StartQuest(manager.GetCurrentDay(), manager.GetCurrentTimeZone());
    }

    public void ManualInteract()
    {
        if (runtimeQuest.state == QuestState.Available)
            runtimeQuest.StartQuest(manager.GetCurrentDay(), manager.GetCurrentTimeZone());
        else if (runtimeQuest.state == QuestState.Active)
            CompleteQuest();
        else if (runtimeQuest.state == QuestState.Completed)
            Debug.Log("Quest already completed.");
    }

    public void CompleteQuest() => runtimeQuest.CompleteQuest(manager.repManager);
}