using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanSchedule.Models;

public class ScheduleEvent : NotifyBase
{
  private TimeSpan _time;
  private Location? _targetLocation;
  private string _activity = string.Empty;
  private TimeSpan _travelTime;
  private TimeSpan _spendTime;
  private TimeSpan _totalTime;
  private bool _hasConflict;

  public TimeSpan Time
  {
    get => _time;
    set => SetField(ref _time, value);
  }

  public Location? TargetLocation
  {
    get => _targetLocation;
    set => SetField(ref _targetLocation, value);
  }

  public string Activity
  {
    get => _activity;
    set => SetField(ref _activity, value);
  }

  // Automatically calculated fields
  public TimeSpan TravelTime
  {
    get => _travelTime;
    set => SetField(ref _travelTime, value);
  }

  public TimeSpan SpendTime
  {
    get => _spendTime;
    set => SetField(ref _spendTime, value);
  }

  public TimeSpan TotalTime
  {
    get => _totalTime;
    set => SetField(ref _totalTime, value);
  }

  public bool HasConflict
  {
    get => _hasConflict;
    set => SetField(ref _hasConflict, value);
  }

  /// <summary>
  /// Formats a TimeSpan as "Xh00" (e.g. 0h30, 8h30, 11h40)
  /// </summary>
  public static string FormatDuration(TimeSpan ts)
  {
    int totalHours = (int)ts.TotalHours;
    int minutes = ts.Minutes;
    return $"{totalHours}h{minutes:D2}";
  }
}
