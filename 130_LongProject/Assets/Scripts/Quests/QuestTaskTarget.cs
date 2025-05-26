using UnityEngine;

public class QuestTaskTarget : MonoBehaviour
{
    public string questName; // The quest this object completes
    private bool taskCompleted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !taskCompleted)
        {
            QuestManager qm = FindObjectOfType<QuestManager>();
            if (qm != null)
            {
                bool success = qm.MarkTaskAsComplete(questName);
                if (success)
                {
                    taskCompleted = true;
                    Debug.Log($"[QuestTaskTarget] Task for '{questName}' marked as complete.");
                    // Optional: play animation, effects, hide horse, etc.
                }
            }
        }
    }
}
