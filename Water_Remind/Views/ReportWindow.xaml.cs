using System.Windows;
using Hydra_Reminder.ViewModels;
using Hydra_Reminder.Services;

namespace Hydra_Reminder.Views;

/// <summary>
/// ����Ʈ â (�����丮/��� ǥ��). X Ŭ�� �� ����.
/// </summary>
public partial class ReportWindow : Window
{
    public ReportWindow(SettingsService settings)
    {
        InitializeComponent();
        DataContext = new ReportViewModel(settings);
    }

    // �巡�� �̵�
    private void DragMove_Window(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
