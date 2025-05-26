using UnityEngine;
using UnityEngine.Events;

public class DayNightCycleManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float realSecondsPerDay = 720f; // 12 minutes per day
    public int totalDays = 7;

    [Header("Sun Rotation")]
    public Light sun;

    [Header("Events (Optional Hooks)")]
    public UnityEvent OnMorning;     // 6am–12pm
    public UnityEvent OnAfternoon;   // 12pm–6pm
    public UnityEvent OnEvening;     // 6pm–12am
    public UnityEvent OnNight;       // 12am–6am
    public UnityEvent OnNewDay;
    public UnityEvent OnEndOfWeek;

    [Range(0f, 24f)] public float startingHour = 6f;
    private float currentTime;

    private int currentDay = 1;
    private int lastPeriodIndex = -1;

    private readonly string[] periodNames = { "Night", "Morning", "Afternoon", "Evening" };
    void Start()
    {
        currentTime = (startingHour / 24f) * realSecondsPerDay;
    }
    void Update()
    {
        if (currentDay > totalDays) return;

        currentTime += Time.deltaTime;

        // Normalized time of day (0 to 1)
        float dayTime = (currentTime % realSecondsPerDay) / realSecondsPerDay;

        // Rotate sun from -90 to 270 degrees over the course of a day
        float sunAngle = dayTime * 360f - 90f;
        if (sun != null)
            sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // Determine current 6-hour period
        float hour = dayTime * 24f;
        int periodIndex = Mathf.FloorToInt(hour / 6f); // 0-3

        if (periodIndex != lastPeriodIndex)
        {
            lastPeriodIndex = periodIndex;
            TriggerTimeZoneEvent(periodIndex);
            Debug.Log($"[Day {currentDay}] Period: {periodNames[periodIndex]}");
        }

        // Check for day rollover
        if (currentTime >= currentDay * realSecondsPerDay)
        {
            currentDay++;
            if (currentDay > totalDays)
            {
                OnEndOfWeek?.Invoke();
                Debug.Log("Game Over — 7 days completed.");
            }
            else
            {
                OnNewDay?.Invoke();
                Debug.Log($"New Day: {currentDay}");
            }
        }
    }

    private void TriggerTimeZoneEvent(int index)
    {
        switch (index)
        {
            case 0: OnNight?.Invoke(); break;
            case 1: OnMorning?.Invoke(); break;
            case 2: OnAfternoon?.Invoke(); break;
            case 3: OnEvening?.Invoke(); break;
        }
    }
    public void SkipTime(float seconds)
    {
        currentTime += seconds;
    }

    // Public accessors
    public int GetCurrentDay() => currentDay;
    public string GetCurrentTimeZone() => periodNames[lastPeriodIndex];
    public float GetCurrentTimeOfDay() => (currentTime % realSecondsPerDay) / realSecondsPerDay;

}
