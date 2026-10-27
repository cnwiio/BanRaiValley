using System;
public enum Season
{
    Spring = 0,
    Summer = 1,
    Fall   = 2,
    Winter = 3
}

public enum DayOfWeek
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}

[Serializable]
public struct GameDateTime
{
    // ----------------------------------------------------------------
    // Fields
    // ----------------------------------------------------------------

    public int         Year;
    public Season      Season;
    public int         DayOfSeason;   // 1 - 30
    public DayOfWeek   DayOfWeek;
    public int         Hour;          // 0 - 23
    public int         Minute;        // 0 - 59

    // ----------------------------------------------------------------
    // Computed Properties
    // ----------------------------------------------------------------

    /// <summary>
    /// Total days elapsed since the very first day of Year 1, Spring.
    /// (Year 1, Spring, Day 1 = 0)
    /// </summary>
    public int TotalDaysPassed =>
        (Year - 1) * 120 + ((int)Season * 30) + (DayOfSeason - 1);

    /// <summary>
    /// Fraction of the current day that has elapsed (0.0 = midnight, 1.0 = next midnight).
    /// </summary>
    public float NormalizedDayTime => (Hour * 60f + Minute) / 1440f;

    /// <summary>Returns true when the current hour falls within 06:00 - 17:59.</summary>
    public bool IsDayTime => Hour >= 6 && Hour < 18;

    /// <summary>Returns true when the current hour is outside the daylight window.</summary>
    public bool IsNightTime => !IsDayTime;

    // ----------------------------------------------------------------
    // Formatting Methods
    // ----------------------------------------------------------------

    /// <summary>
    /// Returns the time as a human-readable string.
    /// </summary>
    /// <param name="use24HourFormat">
    ///   When true, returns 24-hour format (e.g. "18:30").
    ///   When false (default), returns 12-hour AM/PM format (e.g. "06:00 AM").
    /// </param>
    public string ToTimeString(bool use24HourFormat = false)
    {
        if (use24HourFormat)
            return string.Format("{0:D2}:{1:D2}", Hour, Minute);

        string period      = Hour < 12 ? "AM" : "PM";
        int    displayHour = Hour % 12;
        if (displayHour == 0) displayHour = 12;

        return string.Format("{0:D2}:{1:D2} {2}", displayHour, Minute, period);
    }

    /// <summary>
    /// Returns the date as a human-readable string, e.g. "Mon, Spring 1, Year 1".
    /// </summary>
    public string ToDateString()
    {
        string dayAbbreviation = DayOfWeek.ToString().Substring(0, 3);
        return string.Format("{0}, {1} {2}, Year {3}", dayAbbreviation, Season, DayOfSeason, Year);
    }

    // ----------------------------------------------------------------
    // Static Factory
    // ----------------------------------------------------------------

    /// <summary>
    /// The canonical starting date: Year 1, Spring, Day 1, Monday, 06:00.
    /// </summary>
    public static GameDateTime InitialDate => new GameDateTime
    {
        Year        = 1,
        Season      = Season.Spring,
        DayOfSeason = 1,
        DayOfWeek   = DayOfWeek.Monday,
        Hour        = 6,
        Minute      = 0
    };
}

