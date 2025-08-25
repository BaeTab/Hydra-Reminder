using System.Windows;
using Hydra_Reminder.ViewModels;
using Hydra_Reminder.Services;

namespace Hydra_Reminder.Views;

/// <summary>
/// 리포트 창 (히스토리/통계 표시). X 클릭 시 숨김.
/// </summary>
public partial class ReportWindow : Window
{
    public ReportWindow(SettingsService settings)
    {
        InitializeComponent();
        DataContext = new ReportViewModel(settings);
    }

    // 드래그 이동
    private void DragMove_Window(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
