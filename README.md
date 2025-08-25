# Hydra Reminder (Hydra_Reminder)

간단한 데스크톱 물 마시기 리마인더 WPF 애플리케이션입니다. (.NET 8, WPF)

## 주요 기능
- 트레이 아이콘 상주 / 창 닫아도 백그라운드 동작
- 사용자 지정 알림 간격 (평일 / 주말 분리)
- 방해 금지 시간(DND) 및 점심 시간 제외
- 오늘 알림 비활성화, 5분 스누즈
- 최근 7일 + 오늘 기록 저장 및 리포트(그래프, 통계)
- 기록 초기화 / CSV 내보내기
- 커스텀 타이틀바 (라운드 코너 + 투명 창) 및 다크 스타일 UI
- 사운드 온/오프 설정

## 구조 개요
```
Hydra_Reminder (Assembly)
 ├─ Models        : Settings, DayLog 등 데이터 구조
 ├─ Services      : ScheduleService, ReminderService, SettingsService, TrayService 등 핵심 로직
 ├─ ViewModels    : MainViewModel (환경설정), ReportViewModel (리포트)
 ├─ Views         : MainWindow, ReportWindow, PopupToast 등 WPF UI
 ├─ Converters    : BarHeightConverter (막대 차트 높이 변환)
```

## 동작 원리 요약
1. App 시작 시 SettingsService 로 설정 JSON 로드
2. ScheduleService 가 1초 주기 Tick 으로 다음 알림 시간을 관리
3. 알림 시점 도달 → ReminderService 가 PopupToast 표시
4. 완료/스누즈/오늘끄기 액션은 ScheduleService/SettingsService 로 반영
5. 기록은 자정 넘어갈 때 자동 롤오버 (오늘 카운트 → 최근 7일 배열로 이동)
6. ReportWindow 에서 최근 로그/통계/차트 확인 및 CSV 추출 가능

## 빌드 & 실행
- .NET 8 SDK 필요
- 솔루션 디렉토리에서:
```
dotnet build
bin/Debug/net8.0-windows/Hydra_Reminder.exe 실행
```

## 설정 저장 위치
- %APPDATA%/HydraReminder/settings.json

## CSV 출력 위치
- settings.json 과 동일 폴더에 drink_report.csv 생성

## 향후 개선 아이디어
- 다중 모니터 환경에서 토스트 위치 정교화
- Bar 차트 자동 스케일 (최대값 동적 확장)
- 위젯 기능 구현 (현재 플래그만 존재)
- 다국어(한국어/영어) 리소스 분리
- 알림 사운드 사용자 지정

## 개발자 후원
토스뱅크 1001-2269-0600

후원은 선택이며, 개선 제안/버그 리포트도 큰 도움이 됩니다 ??

## 라이선스 (비상업적 사용 허가)
- 비상업적(Non-Commercial) 용도에서 자유롭게 사용 / 수정 / 배포 가능
- 상업적 이용(직/간접 수익 창출: 판매, 유료 서비스 번들, 광고/구독 수익 등) 불가
- 재배포 시 프로젝트 명 및 원저자 출처 표기 권장
- 상업적 사용을 원할 경우 별도 협의 바랍니다
