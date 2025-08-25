using System;

namespace Hydra_Reminder.Models;

/// <summary>
/// 일자별 마신 횟수 단순 DTO (Report / 차트 바인딩 용도)
/// </summary>
public class DayLog
{
    public DateOnly Date { get; set; }      // 해당 일자
    public int Count { get; set; }          // 마신 횟수
}
