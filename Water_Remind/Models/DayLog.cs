using System;

namespace Hydra_Reminder.Models;

/// <summary>
/// ���ں� ���� Ƚ�� �ܼ� DTO (Report / ��Ʈ ���ε� �뵵)
/// </summary>
public class DayLog
{
    public DateOnly Date { get; set; }      // �ش� ����
    public int Count { get; set; }          // ���� Ƚ��
}
