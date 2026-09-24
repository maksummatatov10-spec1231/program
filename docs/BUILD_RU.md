# Сборка PhysSim Studio (RU)

## 1. Что нужно

- **Unity Hub**: https://unity.com/download
- **Unity 2022.3 LTS** (любой патч, например 2022.3.62f1) — модуль **Windows Build Support (IL2CPP не обязателен)**.

## 2. Открыть проект

1. Unity Hub → **Add → Select from disk** → выбрать папку проекта (где лежит `Assets`).
2. Открыть. Первый импорт займёт 1–3 минуты.
3. Откройте сцену `Assets/_Project/Scenes/Main.unity`, нажмите **Play** — программа запустится.

> Если Unity спросит про версию — установите предложенную 2022.3.x через Hub.

## 3. Собрать EXE (одна кнопка)

Меню **PhysSim → Собрать EXE (Windows x64)**.
Результат: `Build/Windows/PhysSimStudio.exe` (+ папка `PhysSimStudio_Data`). Это готовая переносимая программа —
её можно сразу архивировать и запускать на любом Windows x64.

## 4. Установщик .exe (опционально)

Готовый скрипт Inno Setup лежит в `installer/PhysSimStudio.iss`.

1. Скачайте **Inno Setup 6**: https://jrsoftware.org/isdl.php
2. Соберите EXE (шаг 3).
3. Откройте `installer/PhysSimStudio.iss` в Inno Setup Compiler → **Build → Compile**.
4. Готовый `PhysSimStudio-Setup.exe` появится в `installer/Output/`.

## 5. FBX-импорт (опционально)

FBX — закрытый формат, в рантайме он читается только через нативный плагин. Вариант:

1. Скачайте **AssimpNet** (пакет `AssimpNet` для .NET / Unity-порт).
2. Положите managed-сборку в `Assets/Plugins/AssimpNet/`, нативные библиотеки — в `Assets/Plugins/x86_64/`.
3. Реализуйте тело метода `FbxImporter.Import` по плану из комментария в файле
   `Assets/_Project/Scripts/ImportExport/FbxImporter.cs` (обход нод Assimp → меши → фабрика).

До этого импортёр честно сообщит в статус-баре, что FBX требует плагина. **OBJ работает полностью** (свой парсер).

## 6. Тесты

`Window → General → Test Runner → EditMode → Run All` — проверка расчёта объёма меша,
санити базы материалов (121 позиция, уникальные id) и поискового фильтра.

## 7. Частые вопросы

- **Название в заголовке окна** — `Edit → Project Settings → Player → Product Name` (по умолчанию берётся из имени папки).
- **Куда сохраняются сцены** — куда укажете (по умолчанию предлагается `папка игры/Saves`); файл `.pscene` (JSON) + папка `meshes/`.
- **Проект не открывается «в безопасном режиме»** — убедитесь, что скачали патч 2022.3 (не 2023/6000): файл `ProjectSettings/ProjectVersion.txt` фиксирует версию.
