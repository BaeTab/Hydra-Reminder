using System;
using System.Globalization;
using System.Windows.Data;

namespace Hydra_Reminder.Converters;

/// <summary>
/// 막대 차트 높이 변환기.
/// Count(정수)를 0~30 범위로 가정하고 0~120px 로 스케일.
/// 최소 표시 높이 4px 보장.
/// </summary>
public class BarHeightConverter : IValueConverter
{
    private const double MaxPixel = 120.0;          // 최대 픽셀 높이
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int v)
        {
            double maxDomain = 30.0;                // 기준 상한 (넘으면 클램프)
            double ratio = v / maxDomain;           // 정규화
            ratio = Math.Clamp(ratio, 0.0, 1.0);    // 0~1 범위 제한
            double h = MaxPixel * ratio;            // 스케일 적용
            return h < 4 ? 4 : h;                   // 가독성 위한 최소 높이
        }
        return 0d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
