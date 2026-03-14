using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanSchedule.Models;

public class Location : NotifyBase
{
  private static readonly HashSet<string> AllowedIcons = new HashSet<string>{"🏥","🏫","🏋️","🎬","🛒","☕","📚","🚓","🚒","⛪","🏨"};
  private string _icon = "🏠";
  private string _name = string.Empty;
  private double _x;
  private double _y;

  public string Icon
  {
    get => _icon;
    set
    {
      if (AllowedIcons.Contains(value))
        SetField(ref _icon, value);
      // else ignore or optionally throw exception
    }
  }

  public string Name
  {
    get => _name;
    set => SetField(ref _name, value);
  }

  public double X
  {
    get => _x;
    set => SetField(ref _x, value);
  }

  public double Y
  {
    get => _y;
    set => SetField(ref _y, value);
  }

  public static IReadOnlyCollection<string> GetAllowedIcons() => AllowedIcons;

  public override string ToString() => $"{Icon} {Name}";
}
