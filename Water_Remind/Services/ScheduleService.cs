using System;
using System.Windows.Threading;
using Hydra_Reminder.Models;

namespace Hydra_Reminder.Services;

/// <summary>
/// �ֱ��� DispatcherTimer ������� ���� �˸� �ð��� ����ϰ� ReminderDue �̺�Ʈ �߻�.
/// ����, ���ر���, ������, ���� ��Ȱ��ȭ �÷��� ���� ���.
/// </summary>
public class ScheduleService : IDisposable
{
    private readonly SettingsService _settingsService; // ���� ����
    private readonly DispatcherTimer _ticker;          // 1�� Tick Ÿ�̸�
    private DateTime _nextDue;                         // ���� �˸� ���� �ð�
    private DateTime _lastCompute;                     // ������ ���� �ð�

    public event Action<DateTime>? NextDueChanged;      // UI � ���� ���� ���� (���� �̻��)
    public event Action? ReminderDue;                   // �˸� ���� ���� �̺�Ʈ

    public DateTime NextDue => _nextDue;

    // Ȱ�� ��� �⺻ ������ (�����ð� ���� ����)
    private static readonly TimeSpan ActiveWindowStart = new(10,0,0);
    private static readonly TimeSpan ActiveWindowEnd = new(22,0,0);

    public ScheduleService(SettingsService settingsService)
    {
        _settingsService = settingsService;
        _settingsService.SettingsChanged += OnSettingsChanged; // ���� ���� �� ��� ����
        _ticker = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _ticker.Tick += Ticker_Tick;
    }

    private void OnSettingsChanged() => ComputeNextDue(force:true);

    /// <summary>
    /// ������ ���� ���� (��� 1ȸ ��� + Ÿ�̸� ��ŸƮ)
    /// </summary>
    public void Start()
    {
        ComputeNextDue(initial:true);
        _ticker.Start();
    }

    /// <summary>
    /// ���� ȣ��Ǿ� �˸� ����/�ֱ��� ���� �Ǵ�
    /// </summary>
    private void Ticker_Tick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        // �˸� �ð� ���� + Ȱ�� ���� ���� ���� + ���� ��Ȱ��ȭ �ƴ�
        if (now >= _nextDue && IsWithinActiveWindow(now) && !_settingsService.Current.TodayDisabled)
        {
            ReminderDue?.Invoke();
            ComputeNextDue();
        }
        // 1�� �̻� �����ٸ� ȯ�� ��ȭ ���� �� ����
        if ((now - _lastCompute).TotalSeconds > 60)
            ComputeNextDue();
    }

    /// <summary>
    /// ������: ���� �ð� �ڷ� ���� �˸� �̵�
    /// </summary>
    public void Snooze(TimeSpan span)
    {
        _nextDue = DateTime.Now + span;
        NextDueChanged?.Invoke(_nextDue);
    }

    /// <summary>
    /// �� ���� �Ϸ� ó�� �� ī��Ʈ ���� �� ���� ������ ����
    /// </summary>
    public void CompleteDrink()
    {
        _settingsService.IncrementDrink();
        ComputeNextDue();
    }

    public void DisableToday() => _settingsService.Current.TodayDisabled = true; // ���� ��Ȱ��ȭ
    public void ResetTodayDisable() => _settingsService.Current.TodayDisabled = false; // ��Ȱ��ȭ ����

    /// <summary>
    /// ���� �ð��� Ȱ�� ��� ������ + DND/Lunch ���� ������ ��� �����ϴ���
    /// </summary>
    private bool IsWithinActiveWindow(DateTime dt)
    {
        var s = _settingsService.Current;
        if (!IsConfiguredWorkday(dt.DayOfWeek, s)) return false;
        var t = dt.TimeOfDay;
        if (t < ActiveWindowStart || t >= ActiveWindowEnd) return false;
        if (IsQuietHours(dt)) return false;
        if (IsLunch(dt)) return false;
        return true;
    }

    /// <summary>
    /// ���ر���(DND) �ð����� �Ǵ� (���� ��ħ ����)
    /// </summary>
    public bool IsQuietHours(DateTime dt)
    {
        var s = _settingsService.Current.DoNotDisturbStart;
        var e = _settingsService.Current.DoNotDisturbEnd;
        var nowT = dt.TimeOfDay;
        if (s <= e) return nowT >= s && nowT < e; // �ܼ� ����
        return nowT >= s || nowT < e;             // ���� ��ħ
    }

    /// <summary>
    /// ���� �ð� ���� ���� (���� >= ����� ��Ȱ������ ����)
    /// </summary>
    private bool IsLunch(DateTime dt)
    {
        var s = _settingsService.Current.LunchStart;
        var e = _settingsService.Current.LunchEnd;
        if (s >= e) return false;
        var t = dt.TimeOfDay;
        return t >= s && t < e;
    }

    /// <summary>
    /// ���� �˸� �ð� ��� (���� ���� ���� �� �ܼ� ����+����, �ƴϸ� ���� ���� �ð� Ž��)
    /// </summary>
    private void ComputeNextDue(bool initial=false, bool force=false)
    {
        var now = DateTime.Now;
        if (!force && !initial && _nextDue != default && _nextDue > now && IsWithinActiveWindow(now))
            return; // ���� ������ ����

        _lastCompute = now;

        if (!IsWithinActiveWindow(now))
        {
            _nextDue = FindNextActiveStart(now);
            NextDueChanged?.Invoke(_nextDue);
            return;
        }

        _nextDue = now + GetInterval();
        if (!IsWithinActiveWindow(_nextDue))
            _nextDue = FindNextActiveStart(_nextDue);
        NextDueChanged?.Invoke(_nextDue);
    }

    /// <summary>
    /// ������ �����ϴ� ���� ������ ���� �ð� Ž�� (�ִ� 30�� Ž��)
    /// </summary>
    private DateTime FindNextActiveStart(DateTime from)
    {
        var baseDate = from.Date;
        for (int dayOffset = 0; dayOffset <= 30; dayOffset++)
        {
            var d = baseDate.AddDays(dayOffset);
            if (!IsConfiguredWorkday(d.DayOfWeek, _settingsService.Current)) continue;
            var candidate = d + ActiveWindowStart;
            int safetyHours = 0;
            while ((IsQuietHours(candidate) || IsLunch(candidate)) && safetyHours < 12)
            {
                candidate = candidate.AddHours(1);
                safetyHours++;
                if (candidate.TimeOfDay >= ActiveWindowEnd)
                    break; // �׳� �Ұ�
            }
            if (candidate.Date == d && candidate.TimeOfDay < ActiveWindowEnd && candidate > from)
                return candidate;
        }
        return from + TimeSpan.FromHours(1); // fallback (�̷л� ���� ���� X)
    }

    /// <summary>
    /// ���� ����(������/�ָ�) ���� ���� ���� ����(TimeSpan)
    /// </summary>
    private TimeSpan GetInterval()
    {
        var s = _settingsService.Current;
        bool isWorkday = IsConfiguredWorkday(DateTime.Now.DayOfWeek, s);
        int minutes = isWorkday ? s.IntervalMinutesWeekday : s.IntervalMinutesWeekend;
        minutes = Math.Clamp(minutes, 5, 180); // ���� Ŭ����
        return TimeSpan.FromMinutes(minutes);
    }

    /// <summary>
    /// �ش� ������ ����ڰ� üũ�� ���������� �Ǻ�
    /// </summary>
    private static bool IsConfiguredWorkday(DayOfWeek day, Settings s) => day switch
    {
        DayOfWeek.Monday => s.WorkdayMonday,
        DayOfWeek.Tuesday => s.WorkdayTuesday,
        DayOfWeek.Wednesday => s.WorkdayWednesday,
        DayOfWeek.Thursday => s.WorkdayThursday,
        DayOfWeek.Friday => s.WorkdayFriday,
        DayOfWeek.Saturday => s.WorkdaySaturday,
        DayOfWeek.Sunday => s.WorkdaySunday,
        _ => true
    };

    public void Dispose()
    {
        _ticker.Stop();
        _ticker.Tick -= Ticker_Tick;
        _settingsService.SettingsChanged -= OnSettingsChanged;
    }
}
