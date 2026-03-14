# HumanSchedule – Population Simulation Prototype

Eine WPF-Desktop-Anwendung zur Simulation von Tagesplänen einer Bevölkerung. Personen bewegen sich auf einer 2D-Karte zwischen Standorten gemäß ihres konfigurierten Zeitplans.

---

## Systemvoraussetzungen

| Komponente | Anforderung |
|---|---|
| **Betriebssystem** | Windows 10 (64-bit) oder neuer |
| **.NET Runtime** | .NET 8 Desktop Runtime – wird **nicht** benötigt, wenn die App als *self-contained* Einzeldatei veröffentlicht wurde. Andernfalls hier herunterladen: https://dotnet.microsoft.com/download/dotnet/8.0 |

---

## App starten (veröffentlichte Einzeldatei / ClickOnce)

1. **ZIP-Datei entpacken** (falls als ZIP heruntergeladen).
2. Den Ordner öffnen, in dem sich die Datei **`setup.exe`** befindet.
3. Anwendung herunter laden und installieren, indem Sie auf **`setup.exe`** doppelklicken.
4. Ab dann kann die App über das Startmenü oder die Verknüpfung auf dem Desktop gestartet werden.

> **Hinweis:** Beim ersten Start kann Windows SmartScreen eine Warnung anzeigen. Auf **„Weitere Informationen"** und dann **„Trotzdem ausführen"** klicken.

---

## Bedienung

### Welt laden / speichern

Die Anwendung arbeitet mit **JSON-Dateien**, die die gesamte Welt (Standorte + Personen + Zeitpläne) beschreiben.

- **Datei → Open** – eine bestehende `world.json`-Datei laden.
- **Datei → Save** – den aktuellen Zustand als JSON-Datei speichern.
- **Datei → Exit** – Anwendung beenden.

### Simulation steuern

| Button | Funktion |
|---|---|
| **+ Step** | Führt einen einzelnen Simulationsschritt aus (1 Minute Simulationszeit). |
| **▶ Play** | Startet die kontinuierliche Simulation. Der Button wechselt zu **⏸ Pause**. |
| **⏸ Pause** | Hält die laufende Simulation an. |

Die aktuelle Simulationszeit wird oben links in der Toolbar angezeigt.

### Karte

Die Karte (links) zeigt alle Standorte und Personen als Emoji-Icons auf einer 1000 × 1000 Fläche. Personen bewegen sich in Echtzeit zwischen Standorten.

### Personen verwalten (rechtes Panel)

1. **People-Liste** – Alle Personen der Welt. Durch Anklicken wird eine Person ausgewählt.
2. **Selected Person** – Hier können Vorname, Nachname, Icon und Heimatadresse der ausgewählten Person bearbeitet werden.
3. **Schedule** – Der Tagesplan der ausgewählten Person:
   - **+ Add** – Neuen Zeitplan-Eintrag hinzufügen.
   - **- Remove** – Ausgewählten Eintrag löschen.
   - Spalten: **Time** (Uhrzeit), **Location** (Zielort), **Activity** (Tätigkeit), **Travel** (Reisezeit), **Spend** (Aufenthaltszeit), **Total** (Gesamtzeit).
   - Zeilen mit Zeitkonflikten werden **rot** hervorgehoben.

### Standorte verwalten (ganz rechts)

- **Locations-Liste** – Alle Orte der Welt.
- Durch Anklicken eines Ortes können **Name**, **Icon**, **X**- und **Y**-Koordinaten bearbeitet werden.

---

## JSON-Dateiformat (world.json)

Die Welt wird als JSON-Datei mit folgendem Aufbau gespeichert:

```json
{
  "Locations": [
    {
      "Icon": "🏠",
      "Name": "Home",
      "Coord": { "X": 200, "Y": 300 }
    },
    {
      "Icon": "🏢",
      "Name": "Office",
      "Coord": { "X": 700, "Y": 500 }
    }
  ],
  "Citizens": [
    {
      "Icon": "👨",
      "Firstname": "Max",
      "Lastname": "Muster",
      "Home": "Home",
      "Schedule": [
        { "Time": "08:00:00", "Location": "Office", "Activity": "Working" },
        { "Time": "17:00:00", "Location": "Home", "Activity": "Resting" }
      ]
    }
  ]
}
```

---

## Aus dem Quellcode bauen (optional)

Falls die App nicht als fertige EXE vorliegt:

### Voraussetzungen

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Bauen und Starten

```bash
cd HumanSchedule
dotnet build
dotnet run
```

### Als Einzeldatei veröffentlichen

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Die fertige EXE befindet sich dann unter:

```
bin\Release\net8.0-windows\win-x64\publish\HumanSchedule.exe
```

---

## Projektstruktur

```
HumanSchedule/
├── Models/
│   ├── Citizen.cs           # Person mit Icon, Name, Heimat, Schedule
│   ├── Location.cs          # Standort mit Koordinaten
│   ├── ScheduleEvent.cs     # Einzelner Zeitplan-Eintrag
│   ├── WorldData.cs         # JSON-Serialisierungsmodell
│   └── NotifyBase.cs        # INotifyPropertyChanged Basisklasse
├── Services/
│   ├── WorldService.cs      # Laden / Speichern der Welt (JSON)
│   ├── SimulationService.cs # Simulationslogik (Bewegung, Zeitplanung)
│   └── IconService.cs       # Emoji-Icons aus Textdateien laden
├── MainWindow.xaml           # UI-Layout
├── MainWindow.xaml.cs        # UI-Logik
├── citizen-symbols.txt       # Verfügbare Personen-Emojis
├── location-symbols.txt      # Verfügbare Standort-Emojis
└── HumanSchedule.csproj      # Projektdatei (.NET 8, WPF)
```
