using System;
using Hydra_Reminder.Services;

namespace Hydra_Reminder
{
    /// <summary>
    /// 애플리케이션 진입 / 전역 서비스 초기화
    /// </summary>
    public partial class App : System.Windows.Application
    {
        // 전역 서비스 인스턴스 (DI 단순화된 형태)
        private SettingsService _settingsService = null!;   // 설정 로드/저장
        private ScheduleService _scheduleService = null!;   // 알림 스케줄 계산
        private ReminderService _reminderService = null!;   // 토스트(팝업) 표시
        private TrayService _trayService = null!;           // 트레이 아이콘 및 컨텍스트 메뉴

        public static bool IsExiting { get; private set; } = false; // 명시적 종료 여부 플래그

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            // 메인 윈도우 닫혀도 프로세스 유지 (트레이 상주형)
            ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

            // 1. 설정 로드
            _settingsService = new SettingsService();
            _settingsService.Load();

            // 2. 서비스 구성
            _reminderService = new ReminderService(_settingsService);
            _scheduleService = new ScheduleService(_settingsService);
            _trayService = new TrayService(_scheduleService, _reminderService, _settingsService);
            _trayService.ExitRequested += OnExitRequested; // 트레이 메뉴 "종료" 클릭 시 처리

            // 3. 메인 창 (환경설정) 표시/숨김
            var main = new MainWindow(_settingsService);
            MainWindow = main;
            if (_settingsService.Current.StartMinimized)
                main.Hide(); // 시작 시 최소화(숨김)
            else
                main.Show();

            // 4. 스케줄 시작 (Tick + 최초 계산)
            _scheduleService.ReminderDue += OnReminderDue; // 알림 도래 이벤트 연결
            _scheduleService.Start();
        }

        // 알림 시점 도달 → 토스트 표시
        private void OnReminderDue()
        {
            _reminderService.ShowReminder(
                onComplete: () => _scheduleService.CompleteDrink(),
                onSnooze: () => _scheduleService.Snooze(TimeSpan.FromMinutes(5)),
                onDisableToday: () => _scheduleService.DisableToday());
        }

        // 트레이 종료 요청 → 명시적 종료
        private void OnExitRequested()
        {
            IsExiting = true;
            Shutdown();
        }

        protected override void OnExit(System.Windows.ExitEventArgs e)
        {
            // 자원 정리 및 최종 저장
            _scheduleService.Dispose();
            _trayService.Dispose();
            _settingsService.Save();
            base.OnExit(e);
        }
    }
}
