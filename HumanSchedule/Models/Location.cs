using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanSchedule.Models;

public class Location : NotifyBase
{
  private string _icon = "🏠";
  private string _name = string.Empty;
  private double _x;
  private double _y;

  public string Icon
  {
    get => _icon;
    set
    {
      if (!string.IsNullOrEmpty(value))
        SetField(ref _icon, value);
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

  public static IReadOnlyCollection<string> GetAllowedIcons() => PopSimProto.Services.IconService.LocationIcons;

  public override string ToString() => $"{Icon} {Name}";
}
