using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Hydra_Reminder.Views;

/// <summary>
/// �˸� �佺Ʈ â. �ڵ� ���� + ������ ���� �ִϸ��̼�.
/// </summary>
public partial class PopupToast : Window
{
    private readonly DispatcherTimer _autoClose = new() { Interval = TimeSpan.FromMinutes(10) }; // 10�� �� �ڵ� ���� (���콺 ���� �� �Ͻ�����)

    public event Action? Completed;     // �Ϸ� Ŭ��
    public event Action? Snoozed;       // ������ Ŭ��
    public event Action? DisabledToday; // ���� �׸� Ŭ��

    public PopupToast()
    {
        InitializeComponent();
        _autoClose.Tick += (_, _) => { _autoClose.Stop(); SafeClose(); };
        Loaded += (_, _) => _autoClose.Start();
        MouseEnter += (_, _) => _autoClose.Stop();
        MouseLeave += (_, _) => _autoClose.Start();
    }

    /// <summary>
    /// ���� ���� ���� �ݱ�
    /// </summary>
    private void SafeClose()
    {
        if (!IsLoaded) return;
        try { Close(); } catch { }
    }

    /// <summary>
    /// �¿� ��鸲(Shake) Ű������ �ִϸ��̼�
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
