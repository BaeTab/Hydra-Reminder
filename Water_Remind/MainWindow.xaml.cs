using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hydra_Reminder.Services;
using Hydra_Reminder.ViewModels;

namespace Hydra_Reminder
{
    /// <summary>
    /// 환경설정(메인) 창 - 커스텀 타이틀바 + 숨김 패턴.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SettingsService _settingsService; // 설정 주입

        public MainWindow(SettingsService settingsService)
        {
            _settingsService = settingsService;
            InitializeComponent();
            DataContext = new MainViewModel(settingsService);
        }

        // 타이틀바 드래그 이동 (더블클릭 최대/복원)
        private void DragMove_Window(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
                return;
            }
            DragMove();
        }

        // 닫기(X) → 실제 종료 아닌 숨김 처리 (트레이 상주)
        private void Close_Click(object sender, RoutedEventArgs e) => Hide();

        private void ToggleMaximize()
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }
    }
}