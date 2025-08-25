using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hydra_Reminder.Models;
using Hydra_Reminder.Services;

namespace Hydra_Reminder.ViewModels;

/// <summary>
/// ȯ�漳��(MainWindow) ���ε� ViewModel.
/// Settings �ν��Ͻ��� �״�� �����Ͽ� ����� ���ε�.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly SettingsService _settingsService; // ����/�ε� ����

    [ObservableProperty]
    private Settings settings; // UI ���ε� ��� (������ - ���񽺰� ����)

    public MainViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        settings = settingsService.Current;
    }

    /// <summary>
    /// ���� Ŀ�ǵ� - �� Ŭ���� �� ���� + â ����(Ʈ���� ���� UX)
    /// </summary>
    [RelayCommand]
    private void Save()
    {
		Settings.IntervalMinutesWeekday = int.Clamp(Settings.IntervalMinutesWeekday, 10, 120);
		Settings.IntervalMinutesWeekend = int.Clamp(Settings.IntervalMinutesWeekend, 10, 120);
        _settingsService.Save();
        // ���� �� â ����� (Ʈ���� ���� ����)
        var app = System.Windows.Application.Current;
        if (app?.MainWindow != null && app.MainWindow.IsVisible)
        {
            app.MainWindow.Hide();
        }
    }
}
