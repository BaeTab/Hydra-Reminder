using System;
using System.IO;
using System.Text.Json;
using Hydra_Reminder.Models;

namespace Hydra_Reminder.Services;

/// <summary>
/// Settings ��ü �ε�/���� + ī��Ʈ �ѿ��� + �ֱ� �α� ���� �� ���� ���� ���.
/// </summary>
public class SettingsService
{
    private Settings _settings = new(); // ���� ���� �ν��Ͻ�
    public Settings Current => _settings; // �ܺ� ���� (���ε� ���)

    public event Action? SettingsChanged; // ���� �� ������ ���� �� ����

    /// <summary>
    /// ���� ���� �ε�. �ջ�/���� �� �⺻�� ����.
    /// ��¥ ���� ���� �� RollDay ó��.
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
                RollDay(); // ���� �Ѿ -> ���� ��¥ ��� push
            }
        }
        catch
        {
            _settings = new Settings(); // �ջ� �� ����
        }
    }

    /// <summary>
    /// ���� ���� (�ɼ����� SettingsChanged �̺�Ʈ �߻�)
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
        catch { /* �α� ���� */ }
        if (raise) SettingsChanged?.Invoke();
    }

    /// <summary>
    /// �� �� ���� ������ �� ȣ��. ��¥ �ٲ������ �� �ѿ���.
    /// </summary>
    public void IncrementDrink()
    {
        if (_settings.LastCountDate != DateOnly.FromDateTime(DateTime.Now))
            RollDay();
        _settings.TodayDrinkCount += 1;
        Save(raise:false); // ������ ���� ���ʿ��Ͽ� �̺�Ʈ ����
    }

    /// <summary>
    /// ���� ���� Ʈ���� �޴��� ���� �� ȣ��.
    /// </summary>
    public void UpdateIntervalWeekday(int minutes)
    {
        _settings.IntervalMinutesWeekday = minutes;
        Save();
    }

    /// <summary>
    /// ��� �ʱ�ȭ (�ֱ� 7�� + ���� ī��Ʈ ����)
    /// </summary>
    public void ResetHistory()
    {
        _settings.Last7DaysCounts = Array.Empty<int>();
        _settings.TodayDrinkCount = 0;
        _settings.LastCountDate = DateOnly.FromDateTime(DateTime.Now);
        Save(raise:false);
    }

    /// <summary>
    /// ����(��¥ ����) �� ȣ��Ǿ� ���� ī��Ʈ�� �ֱ� �迭 �ڿ� �߰� �� today ����.
    /// </summary>
    private void RollDay()
    {
        var list = _settings.Last7DaysCounts.ToList();
        if (list.Count >= 7) list.RemoveAt(0); // �ִ� 7�� ����
        list.Add(_settings.TodayDrinkCount);
        _settings.Last7DaysCounts = list.ToArray();
        _settings.TodayDrinkCount = 0;
        _settings.LastCountDate = DateOnly.FromDateTime(DateTime.Now);
        Save(raise:false);
    }

    /// <summary>
    /// �ֱ� 7��(�ִ�) + ����(optional) �α� ��ȯ (�������� Date)
    /// </summary>
    public IEnumerable<DayLog> GetRecentLogs(bool includeToday)
    {
        var baseDate = DateOnly.FromDateTime(DateTime.Now);
        var logs = new List<DayLog>();
        int n = _settings.Last7DaysCounts.Length;
        for (int i = 0; i < n; i++)
        {
            int daysAgo = n - i; // index 0 = n�� ��
            logs.Add(new DayLog { Date = baseDate.AddDays(-daysAgo), Count = _settings.Last7DaysCounts[i] });
        }
        if (includeToday)
        {
            logs.Add(new DayLog { Date = baseDate, Count = _settings.TodayDrinkCount });
        }
        return logs.OrderByDescending(l => l.Date);
    }
}
