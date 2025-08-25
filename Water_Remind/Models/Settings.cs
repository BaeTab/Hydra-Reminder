using System;
using System.IO;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hydra_Reminder.Models;

/// <summary>
/// 사용자 설정 + 상태(오늘 마신 횟수, 최근 7일 기록 등)를 보관하는 POCO.
/// 간단히 JSON 직렬화/역직렬화 하며 INotifyPropertyChanged 로 UI 반영.
/// </summary>
public class Settings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ====== 알림 간격 ======
    private int _intervalMinutesWeekday = 45; // 평일 기본 간격 (분)
    public int IntervalMinutesWeekday { get => _intervalMinutesWeekday; set { if (_intervalMinutesWeekday != value) { _intervalMinutesWeekday = value; OnPropertyChanged(); } } }

    private int _intervalMinutesWeekend = 60; // 주말 간격 (분)
    public int IntervalMinutesWeekend { get => _intervalMinutesWeekend; set { if (_intervalMinutesWeekend != value){ _intervalMinutesWeekend = value; OnPropertyChanged(); } } }

    // ====== 카운트 관련 ======
    private int _todayDrinkCount = 0; // 오늘 마신 횟수
    public int TodayDrinkCount { get => _todayDrinkCount; set { if (_todayDrinkCount != value) { _todayDrinkCount = value; OnPropertyChanged(); } } }

    private int[] _last7DaysCounts = Array.Empty<int>(); // 직전 7일(오늘 제외) 기록 배열 (오래된→최근)
    public int[] Last7DaysCounts { get => _last7DaysCounts; set { if (_last7DaysCounts != value) { _last7DaysCounts = value; OnPropertyChanged(); } } }

    // ====== 시간대 설정(DND, 점심) ======
    private TimeSpan _doNotDisturbStart = new(22,0,0); // 방해금지 시작
    public TimeSpan DoNotDisturbStart { get => _doNotDisturbStart; set { if (_doNotDisturbStart != value) { _doNotDisturbStart = value; OnPropertyChanged(); } } }

    private TimeSpan _doNotDisturbEnd = new(6,30,0);   // 방해금지 종료
    public TimeSpan DoNotDisturbEnd { get => _doNotDisturbEnd; set { if (_doNotDisturbEnd != value) { _doNotDisturbEnd = value; OnPropertyChanged(); } } }

    private TimeSpan _lunchStart = new(12,0,0); // 점심 시작
    public TimeSpan LunchStart { get => _lunchStart; set { if (_lunchStart != value) { _lunchStart = value; OnPropertyChanged(); } } }

    private TimeSpan _lunchEnd = new(13,0,0);   // 점심 종료
    public TimeSpan LunchEnd { get => _lunchEnd; set { if (_lunchEnd != value) { _lunchEnd = value; OnPropertyChanged(); } } }

    // ====== 기타 UI/동작 ======
    private bool _enableWidget;                 // 위젯 표시 여부
    public bool EnableWidget { get => _enableWidget; set { if (_enableWidget != value) { _enableWidget = value; OnPropertyChanged(); } } }

    private bool _soundEnabled = true;          // 사운드 사용
    public bool SoundEnabled { get => _soundEnabled; set { if (_soundEnabled != value) { _soundEnabled = value; OnPropertyChanged(); } } }

    private bool _startMinimized = true;        // 시작시 최소화
    public bool StartMinimized { get => _startMinimized; set { if (_startMinimized != value) { _startMinimized = value; OnPropertyChanged(); } } }

    private bool _todayDisabled;                // 오늘 하루 비활성화 플래그
    public bool TodayDisabled { get => _todayDisabled; set { if (_todayDisabled != value) { _todayDisabled = value; OnPropertyChanged(); } } }

    // 위젯 좌표 (추후 위젯 기능 확장 대비)
    private int _widgetX = 50;
    public int WidgetX { get => _widgetX; set { if (_widgetX != value) { _widgetX = value; OnPropertyChanged(); } } }

    private int _widgetY = 50;
    public int WidgetY { get => _widgetY; set { if (_widgetY != value) { _widgetY = value; OnPropertyChanged(); } } }

    // 마지막 카운트가 기록된 날짜 (자정 넘어갈 때 롤오버 판단)
    private DateOnly _lastCountDate = DateOnly.FromDateTime(DateTime.Now);
    public DateOnly LastCountDate { get => _lastCountDate; set { if (_lastCountDate != value) { _lastCountDate = value; OnPropertyChanged(); } } }

    // ====== 업무일 선택 플래그 (월~일) ======
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

    // 설정 파일 경로 (Roaming AppData/HydraReminder/settings.json)
    [JsonIgnore]
    public string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HydraReminder", "settings.json");
}
