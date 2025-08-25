using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Linq;
using Hydra_Reminder.Models;
using Hydra_Reminder.Services;

namespace Hydra_Reminder.ViewModels;

/// <summary>
/// ����Ʈ â ������/��� ��� ViewModel.
/// �ֱ� 7�� + ���� ����� ������� �հ�/���/�ִ�/�ּ�/�߼� ���.
/// </summary>
public class ReportViewModel : INotifyPropertyChanged
{
    private readonly SettingsService _settingsService;
    public ObservableCollection<DayLog> Logs { get; } = new();       // �׸��� ǥ�ÿ� (��������)
    public ObservableCollection<DayLog> ChartItems { get; } = new(); // ��Ʈ ǥ�ÿ� (��������)

    // ===== ǥ�� ������Ƽ�� =====
    private int _totalCount; public int TotalCount { get => _totalCount; set { if (_totalCount != value) { _totalCount = value; OnPropertyChanged(); } } }
    private double _averageCount; public double AverageCount { get => _averageCount; set { if (_averageCount != value) { _averageCount = value; OnPropertyChanged(); } } }
    private int _maxCount; public int MaxCount { get => _maxCount; set { if (_maxCount != value) { _maxCount = value; OnPropertyChanged(); } } }
    private int _minCount; public int MinCount { get => _minCount; set { if (_minCount != value) { _minCount = value; OnPropertyChanged(); } } }
    private int _todayCount; public int TodayCount { get => _todayCount; set { if (_todayCount != value) { _todayCount = value; OnPropertyChanged(); } } }
    private double _todayVsAveragePercent; public double TodayVsAveragePercent { get => _todayVsAveragePercent; set { if (_todayVsAveragePercent != value) { _todayVsAveragePercent = value; OnPropertyChanged(); } } }
    private string? _trendText; public string? TrendText { get => _trendText; set { if (_trendText != value) { _trendText = value; OnPropertyChanged(); } } }
    private string? _exportPath; public string? ExportPath { get => _exportPath; set { if (_exportPath != value) { _exportPath = value; OnPropertyChanged(); } } }

    public ReportViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        Refresh();
    }

    /// <summary>
    /// �α� �籸�� + ��� ����
    /// </summary>
    public void Refresh()
    {
        Logs.Clear();
        ChartItems.Clear();
        foreach (var l in _settingsService.GetRecentLogs(includeToday:true))
            Logs.Add(l);
        // ��Ʈ�� ������ -> �ֽ� ����
        foreach (var l in Logs.OrderBy(l => l.Date))
            ChartItems.Add(l);
        TotalCount = Logs.Sum(l => l.Count);
        AverageCount = Logs.Count > 0 ? Logs.Average(l => l.Count) : 0;
        MaxCount = Logs.Count > 0 ? Logs.Max(l => l.Count) : 0;
        MinCount = Logs.Count > 0 ? Logs.Min(l => l.Count) : 0;
        TodayCount = Logs.FirstOrDefault(l => l.Date == DateOnly.FromDateTime(DateTime.Now))?.Count ?? 0;
        TodayVsAveragePercent = (AverageCount > 0) ? (TodayCount / AverageCount * 100.0) : 0.0;
        if (Logs.Count >= 2)
        {
            var ordered = Logs.OrderByDescending(l => l.Date).Take(2).ToList();
            int diff = ordered[0].Count - ordered[1].Count;
            TrendText = diff switch
            {
                > 0 => $"�� +{diff}",
                < 0 => $"�� {diff}",
                _ => "-"
            };
        }
        else TrendText = "-";
    }

    /// <summary>
    /// CSV �������� (AppData ���� ������ drink_report.csv)
    /// </summary>
    public void Export()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Date,Count");
        foreach (var l in Logs.OrderBy(l => l.Date))
            sb.AppendLine($"{l.Date},{l.Count}");
        var path = Path.Combine(Path.GetDirectoryName(_settingsService.Current.SettingsPath)!, "drink_report.csv");
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        ExportPath = path;
    }

    /// <summary>
    /// ��� �ʱ�ȭ �� ��� �� ������ �ݿ�
    /// </summary>
    public void ResetHistory()
    {
        _settingsService.ResetHistory();
        Refresh();
    }

    // ===== Commands =====
    private RelayCommand? _refreshCommand; public RelayCommand RefreshCommand => _refreshCommand ??= new RelayCommand(() => Refresh());
    private RelayCommand? _exportCommand;  public RelayCommand ExportCommand  => _exportCommand  ??= new RelayCommand(() => Export());
    private RelayCommand? _resetCommand;   public RelayCommand ResetCommand   => _resetCommand   ??= new RelayCommand(() => ResetHistory());

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// �ܼ� RelayCommand ���� (CanExecute ����)
/// </summary>
public class RelayCommand : System.Windows.Input.ICommand
{
    private readonly Action _exec;
    private readonly Func<bool>? _can;
    public RelayCommand(Action exec, Func<bool>? can = null) { _exec = exec; _can = can; }
    public bool CanExecute(object? parameter) => _can?.Invoke() ?? true;
    public void Execute(object? parameter) => _exec();
    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
