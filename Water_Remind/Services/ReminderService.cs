using System;
using System.Media;
using System.Windows;
using Hydra_Reminder.Views;

namespace Hydra_Reminder.Services;

/// <summary>
/// 토스트(팝업) 표시 및 사용자 액션(완료/스누즈/오늘끄기) 콜백 연결 담당.
/// 동시에 하나만 표시되도록 이전 인스턴스 정리.
/// </summary>
public class ReminderService
{
    private readonly SettingsService _settingsService; // 사운드 설정 등 접근
    private PopupToast? _current;                      // 현재 표시 중 토스트

    public ReminderService(SettingsService settings)
    {
        _settingsService = settings;
    }

    /// <summary>
    /// 새 토스트 표시. 기존 토스트가 있으면 닫고 교체.
    /// 화면 우하단(작업 영역 기준) 위치.
    /// </summary>
    public void ShowReminder(Action onComplete, Action onSnooze, Action onDisableToday)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (_current != null)
            {
                _current.Close();
                _current = null;
            }
            _current = new PopupToast();
            _current.Completed += () => { onComplete(); CloseCurrent(); };
            _current.Snoozed += () => { onSnooze(); CloseCurrent(); };
            _current.DisabledToday += () => { onDisableToday(); CloseCurrent(); };

            // 위치 계산 (멀티모니터 개선 여지는 있음)
            var wa = SystemParameters.WorkArea;
            _current.Left = wa.Right - _current.Width - 16;
            _current.Top = wa.Bottom - _current.Height - 16;
            _current.Show();
            _current.AnimateAppear();
            if (_settingsService.Current.SoundEnabled)
            {
                SystemSounds.Asterisk.Play(); // 기본 사운드
            }
        });
    }

    /// <summary>
    /// 현재 토스트 닫고 참조 해제
    /// </summary>
    private void CloseCurrent()
    {
        if (_current != null)
        {
            _current.Close();
            _current = null;
        }
    }
}
