using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using HumanSchedule.Models;
using HumanSchedule.Services;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PopSimProto;

public partial class MainWindow : Window
{
  private readonly WorldService _worldService = new();
  private readonly SimulationService _simService = new();

  private ObservableCollection<Location> _locations = new();
  private ObservableCollection<Citizen> _citizens = new();
  private DateTime _simDateTime;
  private bool _isRunning;
  private DispatcherTimer? _simTimer;

  private Citizen? _selectedCitizen;
  private Location? _selectedLocation;
  private bool _suppressEvents;

  private List<string> _locationIcons;
  private List<string> _citizenIcons;

  public MainWindow()
  {
    InitializeComponent();
    _locationIcons = File.ReadAllText("location-symbols.txt").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
    _citizenIcons = File.ReadAllText("citizen-symbols.txt").Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
    CmbLocationIcon.ItemsSource = _locationIcons;
    CmbCitizenIcon.ItemsSource = _citizenIcons;
    SetupSimTimer();
    UpdateUI();
  }

  private void SetupSimTimer()
  {
    _simTimer = new DispatcherTimer
    {
      Interval = TimeSpan.FromMilliseconds(16)
    };
    _simTimer.Tick += (s, e) =>
    {
      SimStep();
    };
  }

  private void BtnOpen_Click(object sender, RoutedEventArgs e)
  {
    var dlg = new OpenFileDialog
    {
      Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
      Title = "Load World State"
    };

    if (dlg.ShowDialog() == true)
    {
      try
      {
        LoadWorld(dlg.FileName);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error loading world:\n{ex.Message}", "Load Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
  }

  private void BtnSave_Click(object sender, RoutedEventArgs e)
  {
    var dlg = new SaveFileDialog
    {
      Filter = "JSON files (*.json)|*.json",
      Title = "Save World State",
      FileName = "world.json"
    };

    if (dlg.ShowDialog() == true)
    {
      try
      {
        _worldService.SaveWorld(dlg.FileName, _locations.ToList(), _citizens.ToList());
        MessageBox.Show("World saved successfully.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error saving world:\n{ex.Message}", "Save Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
  }

  private void BtnExit_Click(object sender, RoutedEventArgs e)
  {
    Close();
  }

  private void LoadWorld(string filePath)
  {
    StopSimulation();

    _locations.Clear();
    _citizens.Clear();
    _selectedCitizen = null;
    _selectedLocation = null;

    var (locations, citizens) = _worldService.LoadWorld(filePath);

    _locations = new ObservableCollection<Location>(locations);
    _citizens = new ObservableCollection<Citizen>(citizens);

    foreach (var c in _citizens)
    {
      SimulationService.RecalculateScheduleTimes(c);
    }

    _simDateTime = DateTime.Today;

    PeopleList.ItemsSource = _citizens;
    LocationList.ItemsSource = _locations;
    CmbHome.ItemsSource = _locations;
    ColScheduleLocation.ItemsSource = _locations;

    RenderMap();
    UpdateUI();
  }

  private void BtnStep_Click(object sender, RoutedEventArgs e)
  {
    SimStep();
  }

  private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
  {
    if (_isRunning)
      StopSimulation();
    else
      StartSimulation();
  }

  private void StartSimulation()
  {
    _isRunning = true;
    _simTimer?.Start();
    BtnPlayPause.Content = "⏸ Pause";
    BtnStep.IsEnabled = false;
    SetEditControlsEnabled(false);
  }

  private void StopSimulation()
  {
    _isRunning = false;
    _simTimer?.Stop();
    BtnPlayPause.Content = "▶ Play";
    BtnStep.IsEnabled = true;
    SetEditControlsEnabled(true);
  }

  private void SimStep()
  {
    _simDateTime = _simDateTime.AddMinutes(1);
    _simService.Step(_simDateTime, _citizens.ToList());
    RenderMap();
    UpdateUI();
  }

  private void SetEditControlsEnabled(bool enabled)
  {
    CitizenEditorPanel.IsEnabled = enabled && _selectedCitizen != null;
    LocationEditorPanel.IsEnabled = enabled && _selectedLocation != null;
    BtnAddSchedule.IsEnabled = enabled;
    BtnRemoveSchedule.IsEnabled = enabled;
  }

  private void RenderMap()
  {
    MapCanvas.Children.Clear();

    const double scale = 1000.0 / 10000.0;

    foreach (var loc in _locations)
    {
      double cx = loc.X * scale;
      double cy = loc.Y * scale;

      bool isSelected = loc == _selectedLocation;

      var tb = new TextBlock
      {
        Text = loc.Icon,
        FontSize = isSelected ? 22 : 18,
        ToolTip = loc.Name,
        Tag = loc
      };

      if (isSelected)
      {
        var ring = new Ellipse
        {
          Width = 30,
          Height = 30,
          Stroke = Brushes.Blue,
          StrokeThickness = 2,
          Fill = new SolidColorBrush(Color.FromArgb(40, 0, 100, 255))
        };
        Canvas.SetLeft(ring, cx - 15);
        Canvas.SetTop(ring, cy - 15);
        MapCanvas.Children.Add(ring);
      }

      Canvas.SetLeft(tb, cx - 9);
      Canvas.SetTop(tb, cy - 9);
      MapCanvas.Children.Add(tb);
    }

    foreach (var cit in _citizens)
    {
      double cx = cit.CurrentX * scale;
      double cy = cit.CurrentY * scale;

      bool isSelected = cit == _selectedCitizen;

      if (isSelected)
      {
        var ring = new Ellipse
        {
          Width = 26,
          Height = 26,
          Stroke = Brushes.OrangeRed,
          StrokeThickness = 2,
          Fill = new SolidColorBrush(Color.FromArgb(40, 255, 100, 0))
        };
        Canvas.SetLeft(ring, cx - 13);
        Canvas.SetTop(ring, cy - 13);
        MapCanvas.Children.Add(ring);
      }

      var tb = new TextBlock
      {
        Text = cit.Icon,
        FontSize = isSelected ? 18 : 14,
        ToolTip = $"{cit.Firstname} {cit.Lastname}\n{cit.Status}",
        Tag = cit
      };

      Canvas.SetLeft(tb, cx - 7);
      Canvas.SetTop(tb, cy - 7);
      MapCanvas.Children.Add(tb);
    }
  }

  private void PeopleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    _selectedCitizen = PeopleList.SelectedItem as Citizen;
    LoadCitizenEditor();
    RenderMap();
  }

  private void LoadCitizenEditor()
  {
    _suppressEvents = true;

    if (_selectedCitizen == null)
    {
      CitizenEditorPanel.IsEnabled = false;
      CmbCitizenIcon.SelectedItem = "👤";
      TxtFirstname.Text = "";
      TxtLastname.Text = "";
      CmbHome.SelectedItem = null;
      LblStatus.Text = "Status: --";
      ScheduleGrid.ItemsSource = null;
    }
    else
    {
      CitizenEditorPanel.IsEnabled = !_isRunning;
      CmbCitizenIcon.SelectedItem = _selectedCitizen.Icon;
      TxtFirstname.Text = _selectedCitizen.Firstname;
      TxtLastname.Text = _selectedCitizen.Lastname;
      CmbHome.SelectedItem = _selectedCitizen.Home;
      LblStatus.Text = $"Status: {_selectedCitizen.Status}";
      ScheduleGrid.ItemsSource = _selectedCitizen.Schedule;
    }

    _suppressEvents = false;
  }

  private void CitizenField_Changed(object sender, TextChangedEventArgs e)
  {
    if (_suppressEvents || _selectedCitizen == null) return;

    _selectedCitizen.Firstname = TxtFirstname.Text;
    _selectedCitizen.Lastname = TxtLastname.Text;

    PeopleList.Items.Refresh();
    RenderMap();
  }

  private void CmbHome_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (_suppressEvents || _selectedCitizen == null) return;

    _selectedCitizen.Home = CmbHome.SelectedItem as Location;
    SimulationService.RecalculateScheduleTimes(_selectedCitizen);
    ScheduleGrid.Items.Refresh();
  }

  private void CmbCitizenIcon_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (_suppressEvents || _selectedCitizen == null) return;
    _selectedCitizen.Icon = CmbCitizenIcon.SelectedItem as string;
    RenderMap();
  }

  private void BtnAddSchedule_Click(object sender, RoutedEventArgs e)
  {
    if (_selectedCitizen == null) return;

    var newEvent = new ScheduleEvent
    {
      Time = TimeSpan.FromHours(12),
      TargetLocation = _locations.FirstOrDefault(),
      Activity = "Idle"
    };

    _selectedCitizen.Schedule.Add(newEvent);
    SimulationService.RecalculateScheduleTimes(_selectedCitizen);
    ScheduleGrid.Items.Refresh();
    RenderMap();
  }

  private void BtnRemoveSchedule_Click(object sender, RoutedEventArgs e)
  {
    if (_selectedCitizen == null || ScheduleGrid.SelectedItem is not ScheduleEvent selected) return;

    var result = MessageBox.Show("Are you sure you want to remove this schedule event?",
        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

    if (result == MessageBoxResult.Yes)
    {
      _selectedCitizen.Schedule.Remove(selected);
      SimulationService.RecalculateScheduleTimes(_selectedCitizen);
      ScheduleGrid.Items.Refresh();
      RenderMap();
    }
  }

  private void LocationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    _selectedLocation = LocationList.SelectedItem as Location;
    LoadLocationEditor();
    RenderMap();
  }

  private void LoadLocationEditor()
  {
    _suppressEvents = true;

    if (_selectedLocation == null)
    {
      LocationEditorPanel.IsEnabled = false;
      CmbLocationIcon.SelectedItem = "🏠";
      TxtLocationName.Text = "";
      TxtLocationX.Text = "";
      TxtLocationY.Text = "";
    }
    else
    {
      LocationEditorPanel.IsEnabled = !_isRunning;
      CmbLocationIcon.SelectedItem = _selectedLocation.Icon;
      TxtLocationName.Text = _selectedLocation.Name;
      TxtLocationX.Text = _selectedLocation.X.ToString();
      TxtLocationY.Text = _selectedLocation.Y.ToString();
    }

    _suppressEvents = false;
  }

  private void LocationField_Changed(object sender, TextChangedEventArgs e)
  {
    if (_suppressEvents || _selectedLocation == null) return;

    string newName = TxtLocationName.Text.Trim();
    if (!string.IsNullOrEmpty(newName) &&
        !_locations.Any(l => l != _selectedLocation && l.Name == newName))
    {
      _selectedLocation.Name = newName;
    }

    if (double.TryParse(TxtLocationX.Text, out double x) && x >= 0 && x <= 10000)
    { 
      _selectedLocation.X = x;
    }

    if (double.TryParse(TxtLocationY.Text, out double y) && y >= 0 && y <= 10000)
    {
      _selectedLocation.Y = y;
    }

    foreach (var c in _citizens)
    {
      SimulationService.RecalculateScheduleTimes(c);
    }

    LocationList.Items.Refresh();
    RenderMap();
  }

  private void CmbLocationIcon_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (_suppressEvents || _selectedLocation == null) return;
    _selectedLocation.Icon = CmbLocationIcon.SelectedItem as string;
    RenderMap();
  }

  private void UpdateUI()
  {
    SimDateTimeLabel.Text = _citizens.Count > 0
        ? _simDateTime.ToString("dd.MM.yyyy HH:mm")
        : "-- no world loaded --";

    if (_selectedCitizen != null)
    {
      LblStatus.Text = $"Status: {_selectedCitizen.Status}";
    }
  }
}