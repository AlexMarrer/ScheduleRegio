using System.Text.Json.Serialization;

namespace HumanSchedule.Models;

public class WorldData
{
  [JsonPropertyName("Locations")]
  public List<LocationData> Locations { get; set; } = new();

  [JsonPropertyName("Citizens")]
  public List<CitizenData> Citizens { get; set; } = new();
}

public class LocationData
{
  [JsonPropertyName("Icon")]
  public string Icon { get; set; } = "🏠";

  [JsonPropertyName("Name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("Coord")]
  public CoordData Coord { get; set; } = new();
}

public class CoordData
{
  [JsonPropertyName("X")]
  public double X { get; set; }

  [JsonPropertyName("Y")]
  public double Y { get; set; }
}

public class CitizenData
{
  [JsonPropertyName("Icon")]
  public string Icon { get; set; } = "👤";

  [JsonPropertyName("Firstname")]
  public string Firstname { get; set; } = string.Empty;

  [JsonPropertyName("Lastname")]
  public string Lastname { get; set; } = string.Empty;

  [JsonPropertyName("Home")]
  public string Home { get; set; } = string.Empty;

  [JsonPropertyName("Schedule")]
  public List<ScheduleEventData> Schedule { get; set; } = new();
}

public class ScheduleEventData
{
  [JsonPropertyName("Time")]
  public string Time { get; set; } = "00:00:00";

  [JsonPropertyName("Location")]
  public string Location { get; set; } = string.Empty;

  [JsonPropertyName("Activity")]
  public string Activity { get; set; } = string.Empty;
}