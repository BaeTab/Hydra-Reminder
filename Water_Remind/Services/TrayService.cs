using System;
using System.Windows;
using WF = System.Windows.Forms;
using Hydra_Reminder.Views;

namespace Hydra_Reminder.Services;

/// <summary>
/// Ʈ���� ������ �� ���ؽ�Ʈ �޴��� ����. ����� ��ȣ�ۿ�(���� ����, ��� �˸�, ����Ʈ ���� ��)�� ������.
/// </summary>
public class TrayService : IDisposable
{
    private readonly WF.NotifyIcon _icon;          // Ʈ���� ������
    private readonly ScheduleService _scheduleService; // ������ ����
    private readonly ReminderService _reminderService; // �˸� ǥ��
    private readonly SettingsService _settingsService; // ���� ����

    public event Action? ExitRequested;            // ���� �޴� Ŭ�� �� �ܺ� �뺸(App���� Shutdown)

    public TrayService(ScheduleService schedule, ReminderService reminder, SettingsService settings)
    {
        _scheduleService = schedule;
        _reminderService = reminder;
        _settingsService = settings;
        _icon = new WF.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Information,
            Text = "Hydra Reminder",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };
    }

    /// <summary>
    /// ���ؽ�Ʈ �޴� ����
    /// </summary>
    private WF.ContextMenuStrip BuildMenu()
    {
        var menu = new WF.ContextMenuStrip();
        menu.Items.Add("���� �˸�", null, (_, _) => ForceReminder());
        menu.Items.Add("5�� ������", null, (_, _) => _scheduleService.Snooze(TimeSpan.FromMinutes(5)));
        menu.Items.Add("���� �˸� ����", null, (_, _) => { _scheduleService.DisableToday(); });
        menu.Items.Add(new WF.ToolStripSeparator());
        var dnd = new WF.ToolStripMenuItem("���� ���� ���", null, (_, _) => ToggleDnd());
        menu.Items.Add(dnd);
        var interval = new WF.ToolStripMenuItem("���� ����");
        foreach (var m in new[] {15,30,45,60,90})
        {
            var item = new WF.ToolStripMenuItem(m+"��") { Tag = m };
            item.Click += (_, _) => _settingsService.UpdateIntervalWeekday(m);
            interval.DropDownItems.Add(item);
        }
        menu.Items.Add(interval);
        menu.Items.Add(new WF.ToolStripSeparator());
        menu.Items.Add("ȯ�漳��", null, (_, _) => ShowSettingsWindow());
        menu.Items.Add("����Ʈ", null, (_, _) => ShowReportWindow());
        menu.Items.Add("����", null, (_, _) => ExitRequested?.Invoke());
        return menu;
    }

    /// <summary>
    /// ��� �˸� ���� ǥ��
    /// </summary>
    private void ForceReminder()
    {
        _reminderService.ShowReminder(
            onComplete: () => _scheduleService.CompleteDrink(),
            onSnooze: () => _scheduleService.Snooze(TimeSpan.FromMinutes(5)),
            onDisableToday: () => _scheduleService.DisableToday());
    }

    /// <summary>
    /// ���ر��� �ð� ��� (00:00~00:00 = ��Ȱ�� ���·� �ؼ�)
    /// </summary>
    private void ToggleDnd()
    {
        var s = _settingsService.Current;
        if (s.DoNotDisturbStart == TimeSpan.Zero && s.DoNotDisturbEnd == TimeSpan.Zero)
        {
            s.DoNotDisturbStart = new TimeSpan(22,0,0);
            s.DoNotDisturbEnd = new TimeSpan(6,30,0);
        }
        else
        {
            s.DoNotDisturbStart = TimeSpan.Zero;
            s.DoNotDisturbEnd = TimeSpan.Zero;
        }
        _settingsService.Save();
    }

    /// <summary>
    /// ����(����) â ǥ��/Ȱ��ȭ
    /// </summary>
    private void ShowSettingsWindow()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (System.Windows.Application.Current.MainWindow is Window w)
            {
                w.Show();
                w.WindowState = WindowState.Normal;
                w.Activate();
            }
        });
    }

    private ReportWindow? _reportWindow; // ����Ʈ â �̱��� ����
    /// <summary>
    /// ����Ʈ â ǥ��/��Ȱ��ȭ
    /// </summary>
    private void ShowReportWindow()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (_reportWindow == null)
            {
                _reportWindow = new ReportWindow(_settingsService);
                _reportWindow.Closed += (_, _) => _reportWindow = null; // ������ ���� ����
            }
            if (!_reportWindow.IsVisible)
                _reportWindow.Show();
            _reportWindow.Activate();
        });
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
