using System;
using System.Globalization;
using System.Windows.Data;

namespace Hydra_Reminder.Converters;

/// <summary>
/// ���� ��Ʈ ���� ��ȯ��.
/// Count(����)�� 0~30 ������ �����ϰ� 0~120px �� ������.
/// �ּ� ǥ�� ���� 4px ����.
/// </summary>
public class BarHeightConverter : IValueConverter
{
    private const double MaxPixel = 120.0;          // �ִ� �ȼ� ����
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int v)
        {
            double maxDomain = 30.0;                // ���� ���� (������ Ŭ����)
            double ratio = v / maxDomain;           // ����ȭ
            ratio = Math.Clamp(ratio, 0.0, 1.0);    // 0~1 ���� ����
            double h = MaxPixel * ratio;            // ������ ����
            return h < 4 ? 4 : h;                   // ������ ���� �ּ� ����
        }
        return 0d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
