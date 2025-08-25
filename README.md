# Hydra Reminder (Hydra_Reminder)

������ ����ũ�� �� ���ñ� �����δ� WPF ���ø����̼��Դϴ�. (.NET 8, WPF)

## �ֿ� ���
- Ʈ���� ������ ���� / â �ݾƵ� ��׶��� ����
- ����� ���� �˸� ���� (���� / �ָ� �и�)
- ���� ���� �ð�(DND) �� ���� �ð� ����
- ���� �˸� ��Ȱ��ȭ, 5�� ������
- �ֱ� 7�� + ���� ��� ���� �� ����Ʈ(�׷���, ���)
- ��� �ʱ�ȭ / CSV ��������
- Ŀ���� Ÿ��Ʋ�� (���� �ڳ� + ���� â) �� ��ũ ��Ÿ�� UI
- ���� ��/���� ����

## ���� ����
```
Hydra_Reminder (Assembly)
 ���� Models        : Settings, DayLog �� ������ ����
 ���� Services      : ScheduleService, ReminderService, SettingsService, TrayService �� �ٽ� ����
 ���� ViewModels    : MainViewModel (ȯ�漳��), ReportViewModel (����Ʈ)
 ���� Views         : MainWindow, ReportWindow, PopupToast �� WPF UI
 ���� Converters    : BarHeightConverter (���� ��Ʈ ���� ��ȯ)
```

## ���� ���� ���
1. App ���� �� SettingsService �� ���� JSON �ε�
2. ScheduleService �� 1�� �ֱ� Tick ���� ���� �˸� �ð��� ����
3. �˸� ���� ���� �� ReminderService �� PopupToast ǥ��
4. �Ϸ�/������/���ò��� �׼��� ScheduleService/SettingsService �� �ݿ�
5. ����� ���� �Ѿ �� �ڵ� �ѿ��� (���� ī��Ʈ �� �ֱ� 7�� �迭�� �̵�)
6. ReportWindow ���� �ֱ� �α�/���/��Ʈ Ȯ�� �� CSV ���� ����

## ���� & ����
- .NET 8 SDK �ʿ�
- �ַ�� ���丮����:
```
dotnet build
bin/Debug/net8.0-windows/Hydra_Reminder.exe ����
```

## ���� ���� ��ġ
- %APPDATA%/HydraReminder/settings.json

## CSV ��� ��ġ
- settings.json �� ���� ������ drink_report.csv ����

## ���� ���� ���̵��
- ���� ����� ȯ�濡�� �佺Ʈ ��ġ ����ȭ
- Bar ��Ʈ �ڵ� ������ (�ִ밪 ���� Ȯ��)
- ���� ��� ���� (���� �÷��׸� ����)
- �ٱ���(�ѱ���/����) ���ҽ� �и�
- �˸� ���� ����� ����

## ������ �Ŀ�
�佺��ũ 1001-2269-0600

�Ŀ��� �����̸�, ���� ����/���� ����Ʈ�� ū ������ �˴ϴ� ??

## ���̼��� (������ ��� �㰡)
- ������(Non-Commercial) �뵵���� �����Ӱ� ��� / ���� / ���� ����
- ����� �̿�(��/���� ���� â��: �Ǹ�, ���� ���� ����, ����/���� ���� ��) �Ұ�
- ����� �� ������Ʈ �� �� ������ ��ó ǥ�� ����
- ����� ����� ���� ��� ���� ���� �ٶ��ϴ�
