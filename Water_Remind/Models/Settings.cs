using System;
using System.IO;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hydra_Reminder.Models;

/// <summary>
/// ����� ���� + ����(���� ���� Ƚ��, �ֱ� 7�� ��� ��)�� �����ϴ� POCO.
/// ������ JSON ����ȭ/������ȭ �ϸ� INotifyPropertyChanged �� UI �ݿ�.
/// </summary>
public class Settings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ====== �˸� ���� ======
    private int _intervalMinutesWeekday = 45; // ���� �⺻ ���� (��)
    public int IntervalMinutesWeekday { get => _intervalMinutesWeekday; set { if (_intervalMinutesWeekday != value) { _intervalMinutesWeekday = value; OnPropertyChanged(); } } }

    private int _intervalMinutesWeekend = 60; // �ָ� ���� (��)
    public int IntervalMinutesWeekend { get => _intervalMinutesWeekend; set { if (_intervalMinutesWeekend != value){ _intervalMinutesWeekend = value; OnPropertyChanged(); } } }

    // ====== ī��Ʈ ���� ======
    private int _todayDrinkCount = 0; // ���� ���� Ƚ��
    public int TodayDrinkCount { get => _todayDrinkCount; set { if (_todayDrinkCount != value) { _todayDrinkCount = value; OnPropertyChanged(); } } }

    private int[] _last7DaysCounts = Array.Empty<int>(); // ���� 7��(���� ����) ��� �迭 (�����ȡ��ֱ�)
    public int[] Last7DaysCounts { get => _last7DaysCounts; set { if (_last7DaysCounts != value) { _last7DaysCounts = value; OnPropertyChanged(); } } }

    // ====== �ð��� ����(DND, ����) ======
    private TimeSpan _doNotDisturbStart = new(22,0,0); // ���ر��� ����
    public TimeSpan DoNotDisturbStart { get => _doNotDisturbStart; set { if (_doNotDisturbStart != value) { _doNotDisturbStart = value; OnPropertyChanged(); } } }

    private TimeSpan _doNotDisturbEnd = new(6,30,0);   // ���ر��� ����
    public TimeSpan DoNotDisturbEnd { get => _doNotDisturbEnd; set { if (_doNotDisturbEnd != value) { _doNotDisturbEnd = value; OnPropertyChanged(); } } }

    private TimeSpan _lunchStart = new(12,0,0); // ���� ����
    public TimeSpan LunchStart { get => _lunchStart; set { if (_lunchStart != value) { _lunchStart = value; OnPropertyChanged(); } } }

    private TimeSpan _lunchEnd = new(13,0,0);   // ���� ����
    public TimeSpan LunchEnd { get => _lunchEnd; set { if (_lunchEnd != value) { _lunchEnd = value; OnPropertyChanged(); } } }

    // ====== ��Ÿ UI/���� ======
    private bool _enableWidget;                 // ���� ǥ�� ����
    public bool EnableWidget { get => _enableWidget; set { if (_enableWidget != value) { _enableWidget = value; OnPropertyChanged(); } } }

    private bool _soundEnabled = true;          // ���� ���
    public bool SoundEnabled { get => _soundEnabled; set { if (_soundEnabled != value) { _soundEnabled = value; OnPropertyChanged(); } } }

    private bool _startMinimized = true;        // ���۽� �ּ�ȭ
    public bool StartMinimized { get => _startMinimized; set { if (_startMinimized != value) { _startMinimized = value; OnPropertyChanged(); } } }

    private bool _todayDisabled;                // ���� �Ϸ� ��Ȱ��ȭ �÷���
    public bool TodayDisabled { get => _todayDisabled; set { if (_todayDisabled != value) { _todayDisabled = value; OnPropertyChanged(); } } }

    // ���� ��ǥ (���� ���� ��� Ȯ�� ���)
    private int _widgetX = 50;
    public int WidgetX { get => _widgetX; set { if (_widgetX != value) { _widgetX = value; OnPropertyChanged(); } } }

    private int _widgetY = 50;
    public int WidgetY { get => _widgetY; set { if (_widgetY != value) { _widgetY = value; OnPropertyChanged(); } } }

    // ������ ī��Ʈ�� ��ϵ� ��¥ (���� �Ѿ �� �ѿ��� �Ǵ�)
    private DateOnly _lastCountDate = DateOnly.FromDateTime(DateTime.Now);
    public DateOnly LastCountDate { get => _lastCountDate; set { if (_lastCountDate != value) { _lastCountDate = value; OnPropertyChanged(); } } }

    // ====== ������ ���� �÷��� (��~��) ======
    private bool _workdayMonday = true;
    public bool WorkdayMonday { get => _workdayMonday; set { if (_workdayMonday != value) { _workdayMonday = value; OnPropertyChanged(); } } }
    private bool _workdayTuesday = true;
    public bool WorkdayTuesday { get => _workdayTuesday; set { if (_workdayTuesday != value) { _workdayTuesday = value; OnPropertyChanged(); } } }
    private bool _workdayWednesday = true;
    public bool WorkdayWednesday { get => _workdayWednesday; set { if (_workdayWednesday != value) { _workdayWednesday = value; OnPropertyChanged(); } } }
    private bool _workdayThursday = true;
    public bool WorkdayThursday { get => _workdayThursday; set { if (_workdayThursday != value) { _workdayThursday = value; OnPropertyChanged(); } } }
    private bool _workdayFriday = true;
    public bool WorkdayFriday { get => _workdayFriday; set { if (_workdayFriday != value) { _workdayFriday = value; OnPropertyChanged(); } } }
    private bool _workdaySaturday = false;
    public bool WorkdaySaturday { get => _workdaySaturday; set { if (_workdaySaturday != value) { _workdaySaturday = value; OnPropertyChanged(); } } }
    private bool _workdaySunday = false;
    public bool WorkdaySunday { get => _workdaySunday; set { if (_workdaySunday != value) { _workdaySunday = value; OnPropertyChanged(); } } }

    // ���� ���� ��� (Roaming AppData/HydraReminder/settings.json)
    [JsonIgnore]
    public string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HydraReminder", "settings.json");
}
