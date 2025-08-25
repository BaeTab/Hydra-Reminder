using System;
using System.IO;
using System.Text.Json;
using Hydra_Reminder.Models;

namespace Hydra_Reminder.Services;

/// <summary>
/// Settings 객체 로드/저장 + 카운트 롤오버 + 최근 로그 제공 등 상태 관리 담당.
/// </summary>
public class SettingsService
{
    private Settings _settings = new(); // 내부 보관 인스턴스
    public Settings Current => _settings; // 외부 노출 (바인딩 대상)

    public event Action? SettingsChanged; // 저장 후 스케줄 재계산 등 통지

    /// <summary>
    /// 설정 파일 로드. 손상/실패 시 기본값 생성.
    /// 날짜 변경 감지 시 RollDay 처리.
    /// </summary>
    public void Load()
    {
        try
        {
            var path = _settings.SettingsPath;
            var dir = Path.GetDirectoryName(path)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var loaded = JsonSerializer.Deserialize<Settings>(json);
                if (loaded != null)
                    _settings = loaded;
            }
            if (_settings.LastCountDate != DateOnly.FromDateTime(DateTime.Now))
            {
                RollDay(); // 자정 넘어감 -> 이전 날짜 기록 push
            }
        }
        catch
        {
            _settings = new Settings(); // 손상 시 새로
        }
    }

    /// <summary>
    /// 설정 저장 (옵션으로 SettingsChanged 이벤트 발생)
    /// </summary>
    public void Save(bool raise = true)
    {
        try
        {
            var path = _settings.SettingsPath;
            var dir = Path.GetDirectoryName(path)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch { /* 로그 생략 */ }
        if (raise) SettingsChanged?.Invoke();
    }

    /// <summary>
    /// 한 번 물을 마셨을 때 호출. 날짜 바뀌었으면 선 롤오버.
    /// </summary>
    public void IncrementDrink()
    {
        if (_settings.LastCountDate != DateOnly.FromDateTime(DateTime.Now))
            RollDay();
        _settings.TodayDrinkCount += 1;
        Save(raise:false); // 스케줄 재계산 불필요하여 이벤트 생략
    }

    /// <summary>
    /// 평일 간격 트레이 메뉴로 변경 시 호출.
    /// </summary>
    public void UpdateIntervalWeekday(int minutes)
    {
        _settings.IntervalMinutesWeekday = minutes;
        Save();
    }

    /// <summary>
    /// 기록 초기화 (최근 7일 + 오늘 카운트 리셋)
    /// </summary>
    public void ResetHistory()
    {
        _settings.Last7DaysCounts = Array.Empty<int>();
        _settings.TodayDrinkCount = 0;
        _settings.LastCountDate = DateOnly.FromDateTime(DateTime.Now);
        Save(raise:false);
    }

    /// <summary>
    /// 자정(날짜 변경) 시 호출되어 오늘 카운트를 최근 배열 뒤에 추가 후 today 리셋.
    /// </summary>
    private void RollDay()
    {
        var list = _settings.Last7DaysCounts.ToList();
        if (list.Count >= 7) list.RemoveAt(0); // 최대 7개 유지
        list.Add(_settings.TodayDrinkCount);
        _settings.Last7DaysCounts = list.ToArray();
        _settings.TodayDrinkCount = 0;
        _settings.LastCountDate = DateOnly.FromDateTime(DateTime.Now);
        Save(raise:false);
    }

    /// <summary>
    /// 최근 7일(최대) + 오늘(optional) 로그 반환 (내림차순 Date)
    /// </summary>
    public IEnumerable<DayLog> GetRecentLogs(bool includeToday)
    {
        var baseDate = DateOnly.FromDateTime(DateTime.Now);
        var logs = new List<DayLog>();
        int n = _settings.Last7DaysCounts.Length;
        for (int i = 0; i < n; i++)
        {
            int daysAgo = n - i; // index 0 = n일 전
            logs.Add(new DayLog { Date = baseDate.AddDays(-daysAgo), Count = _settings.Last7DaysCounts[i] });
        }
        if (includeToday)
        {
            logs.Add(new DayLog { Date = baseDate, Count = _settings.TodayDrinkCount });
        }
        return logs.OrderByDescending(l => l.Date);
    }
}
