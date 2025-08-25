using System;
using System.Windows;
using WF = System.Windows.Forms;
using Hydra_Reminder.Views;

namespace Hydra_Reminder.Services;

/// <summary>
/// 트레이 아이콘 및 컨텍스트 메뉴를 관리. 사용자 상호작용(간격 변경, 즉시 알림, 리포트 열기 등)의 진입점.
/// </summary>
public class TrayService : IDisposable
{
    private readonly WF.NotifyIcon _icon;          // 트레이 아이콘
    private readonly ScheduleService _scheduleService; // 스케줄 제어
    private readonly ReminderService _reminderService; // 알림 표시
    private readonly SettingsService _settingsService; // 설정 접근

    public event Action? ExitRequested;            // 종료 메뉴 클릭 시 외부 통보(App에서 Shutdown)

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
    /// 컨텍스트 메뉴 구성
    /// </summary>
    private WF.ContextMenuStrip BuildMenu()
    {
        var menu = new WF.ContextMenuStrip();
        menu.Items.Add("지금 알림", null, (_, _) => ForceReminder());
        menu.Items.Add("5분 스누즈", null, (_, _) => _scheduleService.Snooze(TimeSpan.FromMinutes(5)));
        menu.Items.Add("오늘 알림 끄기", null, (_, _) => { _scheduleService.DisableToday(); });
        menu.Items.Add(new WF.ToolStripSeparator());
        var dnd = new WF.ToolStripMenuItem("방해 금지 토글", null, (_, _) => ToggleDnd());
        menu.Items.Add(dnd);
        var interval = new WF.ToolStripMenuItem("간격 설정");
        foreach (var m in new[] {15,30,45,60,90})
        {
            var item = new WF.ToolStripMenuItem(m+"분") { Tag = m };
            item.Click += (_, _) => _settingsService.UpdateIntervalWeekday(m);
            interval.DropDownItems.Add(item);
        }
        menu.Items.Add(interval);
        menu.Items.Add(new WF.ToolStripSeparator());
        menu.Items.Add("환경설정", null, (_, _) => ShowSettingsWindow());
        menu.Items.Add("리포트", null, (_, _) => ShowReportWindow());
        menu.Items.Add("종료", null, (_, _) => ExitRequested?.Invoke());
        return menu;
    }

    /// <summary>
    /// 즉시 알림 강제 표시
    /// </summary>
    private void ForceReminder()
    {
        _reminderService.ShowReminder(
            onComplete: () => _scheduleService.CompleteDrink(),
            onSnooze: () => _scheduleService.Snooze(TimeSpan.FromMinutes(5)),
            onDisableToday: () => _scheduleService.DisableToday());
    }

    /// <summary>
    /// 방해금지 시간 토글 (00:00~00:00 = 비활성 상태로 해석)
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
    /// 설정(메인) 창 표시/활성화
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

    private ReportWindow? _reportWindow; // 리포트 창 싱글톤 재사용
    /// <summary>
    /// 리포트 창 표시/재활성화
    /// </summary>
    private void ShowReportWindow()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (_reportWindow == null)
            {
                _reportWindow = new ReportWindow(_settingsService);
                _reportWindow.Closed += (_, _) => _reportWindow = null; // 닫히면 참조 해제
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
