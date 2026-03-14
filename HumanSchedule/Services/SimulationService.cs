using HumanSchedule.Models;

namespace HumanSchedule.Services;

public class SimulationService
{
  private const double MapSizeMeters = 10000.0;
  private const double SpeedKmH = 5.0;
  private const double SpeedMetersPerMinute = SpeedKmH * 1000.0 / 60.0;

  public void Step(DateTime currentTime, List<Citizen> citizens)
  {
    var timeOfDay = currentTime.TimeOfDay;
    foreach (var citizen in citizens)
    {
      UpdateCitizen(citizen, timeOfDay);
    }
  }

  private void UpdateCitizen(Citizen citizen, TimeSpan currentTime)
  {
    if (citizen.Schedule.Count == 0)
    {
      citizen.Status = "Idle (no schedule)";
      return;
    }

    var sortedEvents = citizen.Schedule.OrderBy(e => e.Time).ToList();
    ScheduleEvent? activeEvent = null;
    ScheduleEvent? nextEvent = null;

    for (int i = sortedEvents.Count - 1; i >= 0; i--)
    {
      if (sortedEvents[i].Time <= currentTime)
      {
        activeEvent = sortedEvents[i];
        nextEvent = (i + 1 < sortedEvents.Count) ? sortedEvents[i + 1] : sortedEvents[0];
        break;
      }
    }

    if (activeEvent == null)
    {
      activeEvent = sortedEvents[^1];
      nextEvent = sortedEvents[0];
    }

    if (activeEvent.TargetLocation == null) return;

    var departureX = GetDepartureX(citizen, activeEvent, sortedEvents);
    var departureY = GetDepartureY(citizen, activeEvent, sortedEvents);

    var destX = activeEvent.TargetLocation.X;
    var destY = activeEvent.TargetLocation.Y;

    double distance = EuclideanDistance(departureX, departureY, destX, destY);
    double travelTimeMinutes = distance / SpeedMetersPerMinute;

    double minutesSinceEventStart = (currentTime - activeEvent.Time).TotalMinutes;
    if (minutesSinceEventStart < 0) minutesSinceEventStart += 24 * 60;

    double distanceTraveled = minutesSinceEventStart * SpeedMetersPerMinute;

    if (distanceTraveled < distance)
    {
      citizen.IsTraveling = true;
      var (ix, iy) = Interpolate(departureX, departureY, destX, destY, distanceTraveled);
      citizen.CurrentX = ix;
      citizen.CurrentY = iy;
      citizen.Status = $"Traveling to {activeEvent.TargetLocation.Icon} {activeEvent.TargetLocation.Name}";
    }
    else
    {
      citizen.IsTraveling = false;
      citizen.CurrentX = destX;
      citizen.CurrentY = destY;
      citizen.Status = $"{activeEvent.Activity} at {activeEvent.TargetLocation.Icon} {activeEvent.TargetLocation.Name}";
    }
  }

  private double GetDepartureX(Citizen citizen, ScheduleEvent activeEvent, List<ScheduleEvent> sorted)
  {
    int idx = sorted.IndexOf(activeEvent);
    if (idx == 0)
    {
      var prev = sorted[^1];
      return prev.TargetLocation?.X ?? citizen.Home?.X ?? 0;
    }
    return sorted[idx - 1].TargetLocation?.X ?? citizen.Home?.X ?? 0;
  }

  private double GetDepartureY(Citizen citizen, ScheduleEvent activeEvent, List<ScheduleEvent> sorted)
  {
    int idx = sorted.IndexOf(activeEvent);
    if (idx == 0)
    {
      var prev = sorted[^1];
      return prev.TargetLocation?.Y ?? citizen.Home?.Y ?? 0;
    }
    return sorted[idx - 1].TargetLocation?.Y ?? citizen.Home?.Y ?? 0;
  }

  public static double EuclideanDistance(double x1, double y1, double x2, double y2)
  {
    return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
  }

  public static (double X, double Y) Interpolate(double depX, double depY, double desX, double desY, double d)
  {
    double distance = EuclideanDistance(depX, depY, desX, desY);
    if (distance == 0) return (depX, depY);

    double ratio = d / distance;
    return (
        depX + (desX - depX) * ratio,
        depY + (desY - depY) * ratio
    );
  }

  public static void RecalculateScheduleTimes(Citizen citizen)
  {
    var sorted = citizen.Schedule.OrderBy(e => e.Time).ToList();
    if (sorted.Count == 0) return;

    for (int i = 0; i < sorted.Count; i++)
    {
      var current = sorted[i];
      var next = sorted[(i + 1) % sorted.Count];

      double depX, depY;
      if (i == 0)
      {
        var prev = sorted[^1];
        depX = prev.TargetLocation?.X ?? citizen.Home?.X ?? 0;
        depY = prev.TargetLocation?.Y ?? citizen.Home?.Y ?? 0;
      }
      else
      {
        depX = sorted[i - 1].TargetLocation?.X ?? citizen.Home?.X ?? 0;
        depY = sorted[i - 1].TargetLocation?.Y ?? citizen.Home?.Y ?? 0;
      }

      double destX = current.TargetLocation?.X ?? 0;
      double destY = current.TargetLocation?.Y ?? 0;

      double distance = EuclideanDistance(depX, depY, destX, destY);
      double travelMinutes = distance / SpeedMetersPerMinute;
      current.TravelTime = TimeSpan.FromMinutes(Math.Ceiling(travelMinutes));

      TimeSpan totalTime;
      if (i + 1 < sorted.Count)
      {
        totalTime = next.Time - current.Time;
      }
      else
      {
        totalTime = (TimeSpan.FromHours(24) - current.Time) + sorted[0].Time;
      }
      current.TotalTime = totalTime;
      current.SpendTime = totalTime - current.TravelTime;

      current.HasConflict = current.SpendTime < TimeSpan.Zero;
    }
  }
}