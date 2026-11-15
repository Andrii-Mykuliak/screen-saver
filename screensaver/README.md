# Зимове вікно як скрінсейвер (Windows)

Збірка: `dotnet publish -c Release -o out`, далі `out\WinterWindow.exe` перейменувати в `WinterWindow.scr`.
Потрібні .NET 8 Desktop Runtime і WebView2 Runtime (є у Windows 11).

Встановлення: правою кнопкою на `.scr`, пункт Install. Перегляд: пункт Test.
Закривається від руху миші, клавіші чи кліку. Журнал: `%LOCALAPPDATA%\AsciiWinterWindow\log.txt`.
