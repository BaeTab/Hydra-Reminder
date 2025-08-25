using System;
using System.Media;
using System.Windows;
using Hydra_Reminder.Views;

namespace Hydra_Reminder.Services;

/// <summary>
/// �佺Ʈ(�˾�) ǥ�� �� ����� �׼�(�Ϸ�/������/���ò���) �ݹ� ���� ���.
/// ���ÿ� �ϳ��� ǥ�õǵ��� ���� �ν��Ͻ� ����.
/// </summary>
public class ReminderService
{
    private readonly SettingsService _settingsService; // ���� ���� �� ����
    private PopupToast? _current;                      // ���� ǥ�� �� �佺Ʈ

    public ReminderService(SettingsService settings)
    {
        _settingsService = settings;
    }

    /// <summary>
    /// �� �佺Ʈ ǥ��. ���� �佺Ʈ�� ������ �ݰ� ��ü.
    /// ȭ�� ���ϴ�(�۾� ���� ����) ��ġ.
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

            // ��ġ ��� (��Ƽ����� ���� ������ ����)
            var wa = SystemParameters.WorkArea;
            _current.Left = wa.Right - _current.Width - 16;
            _current.Top = wa.Bottom - _current.Height - 16;
            _current.Show();
            _current.AnimateAppear();
            if (_settingsService.Current.SoundEnabled)
            {
                SystemSounds.Asterisk.Play(); // �⺻ ����
            }
        });
    }

    /// <summary>
    /// ���� �佺Ʈ �ݰ� ���� ����
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
