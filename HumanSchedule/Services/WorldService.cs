using System.IO;
using System.Text.Json;
using HumanSchedule.Models;

namespace HumanSchedule.Services;

public class WorldService
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    WriteIndented = true,
    PropertyNameCaseInsensitive = true
  };

  /// <summary>
  /// Loads a world JSON file and converts to in-memory models.
  /// </summary>
  public (List<Location> Locations, List<Citizen> Citizens) LoadWorld(string filePath)
  {
    string json = File.ReadAllText(filePath);
    var data = JsonSerializer.Deserialize<WorldData>(json, JsonOptions)
        ?? throw new InvalidOperationException("Failed to parse world JSON.");

    // 1) Convert Locations
    var locations = data.Locations.Select(ld => new Location
    {
      Icon = ld.Icon,
      Name = ld.Name,
      X = ld.Coord.X,
      Y = ld.Coord.Y
    }).ToList();

    var locationLookup = locations.ToDictionary(l => l.Name);

    // 2) Convert Citizens
    var citizens = data.Citizens.Select(cd =>
    {
      var citizen = new Citizen
      {
        Icon = cd.Icon,
        Firstname = cd.Firstname,
        Lastname = cd.Lastname,
        Home = locationLookup.GetValueOrDefault(cd.Home)
      };

      // Start at home
      if (citizen.Home != null)
      {
        citizen.CurrentX = citizen.Home.X;
        citizen.CurrentY = citizen.Home.Y;
      }

      // Convert schedule
      foreach (var sed in cd.Schedule.OrderBy(s => TimeSpan.Parse(s.Time)))
      {
        citizen.Schedule.Add(new ScheduleEvent
        {
          Time = TimeSpan.Parse(sed.Time),
          TargetLocation = locationLookup.GetValueOrDefault(sed.Location),
          Activity = sed.Activity
        });
      }

      return citizen;
    }).ToList();

    return (locations, citizens);
  }

  /// <summary>
  /// Converts in-memory models back to JSON and saves to file.
  /// </summary>
  public void SaveWorld(string filePath, List<Location> locations, List<Citizen> citizens)
  {
    var data = new WorldData
    {
      Locations = locations.Select(l => new LocationData
      {
        Icon = l.Icon,
        Name = l.Name,
        Coord = new CoordData { X = l.X, Y = l.Y }
      }).ToList(),

      Citizens = citizens.Select(c => new CitizenData
      {
        Icon = c.Icon,
        Firstname = c.Firstname,
        Lastname = c.Lastname,
        Home = c.Home?.Name ?? string.Empty,
        Schedule = c.Schedule.Select(se => new ScheduleEventData
        {
          Time = se.Time.ToString(@"hh\:mm\:ss"),
          Location = se.TargetLocation?.Name ?? string.Empty,
          Activity = se.Activity
        }).ToList()
      }).ToList()
    };

    string json = JsonSerializer.Serialize(data, JsonOptions);
    File.WriteAllText(filePath, json);
  }
}