using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hydra_Reminder.Models;
using Hydra_Reminder.Services;

namespace Hydra_Reminder.ViewModels;

/// <summary>
/// 환경설정(MainWindow) 바인딩 ViewModel.
/// Settings 인스턴스를 그대로 노출하여 양방향 바인딩.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly SettingsService _settingsService; // 저장/로드 서비스

    [ObservableProperty]
    private Settings settings; // UI 바인딩 대상 (참조형 - 서비스가 소유)

    public MainViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        settings = settingsService.Current;
    }

    /// <summary>
    /// 저장 커맨드 - 값 클램프 후 저장 + 창 숨김(트레이 상주 UX)
    /// </summary>
    [RelayCommand]
    private void Save()
    {
		Settings.IntervalMinutesWeekday = int.Clamp(Settings.IntervalMinutesWeekday, 10, 120);
		Settings.IntervalMinutesWeekend = int.Clamp(Settings.IntervalMinutesWeekend, 10, 120);
        _settingsService.Save();
        // 저장 후 창 숨기기 (트레이 상주 패턴)
        var app = System.Windows.Application.Current;
        if (app?.MainWindow != null && app.MainWindow.IsVisible)
        {
            app.MainWindow.Hide();
        }
    }
}
