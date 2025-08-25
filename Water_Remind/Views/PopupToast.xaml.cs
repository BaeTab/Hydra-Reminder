using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Hydra_Reminder.Views;

/// <summary>
/// 알림 토스트 창. 자동 닫힘 + 간단한 등장 애니메이션.
/// </summary>
public partial class PopupToast : Window
{
    private readonly DispatcherTimer _autoClose = new() { Interval = TimeSpan.FromMinutes(10) }; // 10분 후 자동 닫힘 (마우스 진입 시 일시정지)

    public event Action? Completed;     // 완료 클릭
    public event Action? Snoozed;       // 스누즈 클릭
    public event Action? DisabledToday; // 오늘 그만 클릭

    public PopupToast()
    {
        InitializeComponent();
        _autoClose.Tick += (_, _) => { _autoClose.Stop(); SafeClose(); };
        Loaded += (_, _) => _autoClose.Start();
        MouseEnter += (_, _) => _autoClose.Stop();
        MouseLeave += (_, _) => _autoClose.Start();
    }

    /// <summary>
    /// 예외 방지 안전 닫기
    /// </summary>
    private void SafeClose()
    {
        if (!IsLoaded) return;
        try { Close(); } catch { }
    }

    /// <summary>
    /// 좌우 흔들림(Shake) 키프레임 애니메이션
    /// </summary>
    public void AnimateAppear()
    {
        var sb = new Storyboard();
        var anim = new DoubleAnimationUsingKeyFrames();
        var baseLeft = Left;
        void KF(double ms, double delta) => anim.KeyFrames.Add(new DiscreteDoubleKeyFrame(baseLeft + delta, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(ms))));
        KF(0,0); KF(60,-6); KF(120,6); KF(180,-4); KF(240,4); KF(300,0);
        Storyboard.SetTarget(anim, this);
        Storyboard.SetTargetProperty(anim, new PropertyPath("Left"));
        sb.Children.Add(anim);
        sb.Begin();
    }

    private void Complete_Click(object sender, RoutedEventArgs e){ Completed?.Invoke(); SafeClose(); }
    private void Snooze_Click(object sender, RoutedEventArgs e){ Snoozed?.Invoke(); SafeClose(); }
    private void DisableToday_Click(object sender, RoutedEventArgs e){ DisabledToday?.Invoke(); SafeClose(); }
}
