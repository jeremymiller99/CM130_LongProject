public enum QuestState
{
    Inactive,           // Quest is not yet available (wrong time/day)
    AvailableInactive,  // Quest is available but not yet accepted
    AvailableActive,    // Quest is available and has been accepted
    Completed,         // Quest has been completed
    Failed            // Quest has failed (optional state)
}
