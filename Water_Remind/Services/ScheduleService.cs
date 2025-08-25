using System;
using System.Windows.Threading;
using Hydra_Reminder.Models;

namespace Hydra_Reminder.Services;

/// <summary>
/// 주기적 DispatcherTimer 기반으로 다음 알림 시간을 계산하고 ReminderDue 이벤트 발생.
/// 점심, 방해금지, 업무일, 오늘 비활성화 플래그 등을 고려.
/// </summary>
public class ScheduleService : IDisposable
{
    private readonly SettingsService _settingsService; // 설정 접근
    private readonly DispatcherTimer _ticker;          // 1초 Tick 타이머
    private DateTime _nextDue;                         // 다음 알림 예정 시각
    private DateTime _lastCompute;                     // 마지막 재계산 시각

    public event Action<DateTime>? NextDueChanged;      // UI 등에 다음 예정 통지 (현재 미사용)
    public event Action? ReminderDue;                   // 알림 시점 도달 이벤트

    public DateTime NextDue => _nextDue;

    // 활동 허용 기본 프레임 (업무시간 유사 개념)
    private static readonly TimeSpan ActiveWindowStart = new(10,0,0);
    private static readonly TimeSpan ActiveWindowEnd = new(22,0,0);

    public ScheduleService(SettingsService settingsService)
    {
        _settingsService = settingsService;
        _settingsService.SettingsChanged += OnSettingsChanged; // 설정 변경 → 즉시 재계산
        _ticker = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _ticker.Tick += Ticker_Tick;
    }

    private void OnSettingsChanged() => ComputeNextDue(force:true);

    /// <summary>
    /// 스케줄 서비스 시작 (즉시 1회 계산 + 타이머 스타트)
    /// </summary>
    public void Start()
    {
        ComputeNextDue(initial:true);
        _ticker.Start();
    }

    /// <summary>
    /// 매초 호출되어 알림 도래/주기적 재계산 판단
    /// </summary>
    private void Ticker_Tick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        // 알림 시각 도달 + 활동 가능 조건 충족 + 오늘 비활성화 아님
        if (now >= _nextDue && IsWithinActiveWindow(now) && !_settingsService.Current.TodayDisabled)
        {
            ReminderDue?.Invoke();
            ComputeNextDue();
        }
        // 1분 이상 지났다면 환경 변화 가능 → 재계산
        if ((now - _lastCompute).TotalSeconds > 60)
            ComputeNextDue();
    }

    /// <summary>
    /// 스누즈: 지정 시간 뒤로 다음 알림 이동
    /// </summary>
    public void Snooze(TimeSpan span)
    {
        _nextDue = DateTime.Now + span;
        NextDueChanged?.Invoke(_nextDue);
    }

    /// <summary>
    /// 물 마신 완료 처리 → 카운트 증가 후 다음 스케줄 재계산
    /// </summary>
    public void CompleteDrink()
    {
        _settingsService.IncrementDrink();
        ComputeNextDue();
    }

    public void DisableToday() => _settingsService.Current.TodayDisabled = true; // 오늘 비활성화
    public void ResetTodayDisable() => _settingsService.Current.TodayDisabled = false; // 비활성화 해제

    /// <summary>
    /// 현재 시각이 활동 허용 윈도우 + DND/Lunch 제외 조건을 모두 만족하는지
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
    /// 방해금지(DND) 시간인지 판단 (자정 걸침 대응)
    /// </summary>
    public bool IsQuietHours(DateTime dt)
    {
        var s = _settingsService.Current.DoNotDisturbStart;
        var e = _settingsService.Current.DoNotDisturbEnd;
        var nowT = dt.TimeOfDay;
        if (s <= e) return nowT >= s && nowT < e; // 단순 구간
        return nowT >= s || nowT < e;             // 자정 걸침
    }

    /// <summary>
    /// 점심 시간 범위 여부 (시작 >= 종료면 비활성으로 간주)
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
    /// 다음 알림 시각 계산 (현재 조건 만족 시 단순 현재+간격, 아니면 다음 시작 시각 탐색)
    /// </summary>
    private void ComputeNextDue(bool initial=false, bool force=false)
    {
        var now = DateTime.Now;
        if (!force && !initial && _nextDue != default && _nextDue > now && IsWithinActiveWindow(now))
            return; // 기존 스케줄 유지

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
    /// 조건을 만족하는 다음 가능한 시작 시각 탐색 (최대 30일 탐색)
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
                    break; // 그날 불가
            }
            if (candidate.Date == d && candidate.TimeOfDay < ActiveWindowEnd && candidate > from)
                return candidate;
        }
        return from + TimeSpan.FromHours(1); // fallback (이론상 거의 도달 X)
    }

    /// <summary>
    /// 현재 요일(업무일/주말) 기준 실제 적용 간격(TimeSpan)
    /// </summary>
    private TimeSpan GetInterval()
    {
        var s = _settingsService.Current;
        bool isWorkday = IsConfiguredWorkday(DateTime.Now.DayOfWeek, s);
        int minutes = isWorkday ? s.IntervalMinutesWeekday : s.IntervalMinutesWeekend;
        minutes = Math.Clamp(minutes, 5, 180); // 안전 클램프
        return TimeSpan.FromMinutes(minutes);
    }

    /// <summary>
    /// 해당 요일이 사용자가 체크한 업무일인지 판별
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
