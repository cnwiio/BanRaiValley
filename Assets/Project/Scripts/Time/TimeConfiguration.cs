using UnityEngine;

[CreateAssetMenu(fileName = "TimeConfiguration", menuName = "Scriptable Objects/TimeConfiguration")]
public class TimeConfiguration : ScriptableObject
{
    [Header("Simulation Timing")]
    [Tooltip("How many real-world seconds pass per in-game minute.")]
    public float realSecondsPerGameMinute = 0.75f;

    [Tooltip("How many in-game minutes advance per timer tick.")]
    public int minuteTickInterval = 1;
    
    [Header("Day Boundaries")]
    [Tooltip("The hour (0-23) at which a new day begins after sleep.")]
    public int startHour = 6;

    [Tooltip("The hour (0-23) at which the player is forcibly passed out.")]
    public int passoutHour = 2;

    [Tooltip("Number of in-game days per season.")]
    public int daysPerSeason = 30;
}
