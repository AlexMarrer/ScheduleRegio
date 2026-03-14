using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanSchedule.Models;

public class Citizen : NotifyBase
{
  private static readonly HashSet<string> AllowedIcons = new HashSet<string>{"👦","👧","👨","👩","👴","👵"};
  private string _icon = "👤";
  private string _firstname = string.Empty;
  private string _lastname = string.Empty;
  private Location? _home;
  private string _status = "Idle";
  private double _currentX;
  private double _currentY;
  private bool _isTraveling;

  public string Icon
  {
    get => _icon;
    set
    {
      if (!string.IsNullOrEmpty(value))
        SetField(ref _icon, value);
    }
  }

  public string Firstname
  {
    get => _firstname;
    set { SetField(ref _firstname, value); OnPropertyChanged(nameof(DisplayName)); }
  }

  public string Lastname
  {
    get => _lastname;
    set { SetField(ref _lastname, value); OnPropertyChanged(nameof(DisplayName)); }
  }

  public string DisplayName => $"{Icon} {Firstname} {Lastname}";

  public Location? Home
  {
    get => _home;
    set => SetField(ref _home, value);
  }

  public string Status
  {
    get => _status;
    set => SetField(ref _status, value);
  }

  public double CurrentX
  {
    get => _currentX;
    set => SetField(ref _currentX, value);
  }

  public double CurrentY
  {
    get => _currentY;
    set => SetField(ref _currentY, value);
  }

  public bool IsTraveling
  {
    get => _isTraveling;
    set => SetField(ref _isTraveling, value);
  }

  public ObservableCollection<ScheduleEvent> Schedule { get; set; } = new();

  public static IReadOnlyCollection<string> GetAllowedIcons() => PopSimProto.Services.IconService.CitizenIcons;
}
