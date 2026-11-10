# Сборка Zelen VR

## 1. Standalone (Windows)

1. **File → Build Settings**.
2. Platform: **Windows, Mac, Linux → Windows**.
3. Target: **Windows**, Architecture: **x86_64**.
4. Scenes In Build: `Assets/Scenes/SampleScene.unity`.
5. **Player Settings**:
   - Company: `DefaultCompany`
   - Product Name: `Praktika_Zelen67`
   - Scripting Backend: `IL2CPP` (для релиза) / `Mono` (для отладки)
6. **Build** → папка `Builds/Windows/`.

Для отладки без шлема: в сцене включён **XR Device Simulator** (XR Interaction Toolkit).

## 2. Android / Meta Quest

1. Установить **Android Build Support** (через Unity Hub → Add Modules).
2. **File → Build Settings → Android → Switch Platform**.
3. **Player Settings → XR Plug-in Management**:
   - Android → включить **OpenXR**.
   - OpenXR → Interaction Profiles: **Oculus Touch Controller Profile**.
   - OpenXR → Render Mode: **Single Pass Instanced**.
4. **Player Settings → Other Settings**:
   - Scripting Backend: `IL2CPP`
   - Target Architectures: `ARM64` (только)
   - Minimum API Level: `Android 10 (API 29)` или выше
   - Graphics APIs: `Vulkan` (если шейдеры URP глючат — `OpenGLES3`)
5. **Build** → `Builds/Android/zelen.apk`.
6. Установка на шлем:

adb install -r Builds/Android/zelen.apk

Или через **SideQuest** (drag-n-drop APK).

## 3. Проверка перед релизом

- [ ] Сцена `SampleScene` в Scenes In Build
- [ ] OpenXR включён, **XR Plug-in Management → Project Validation** — 0 ошибок
- [ ] `Product Name` и `Package Name` заданы
- [ ] IL2CPP + ARM64 для Android
- [ ] Сборка завершилась без ошибок компиляции

## 4. Частые проблемы

| Симптом | Причина | Решение |
|---------|---------|---------|
| «OpenXR not initialized» | Не включён OpenXR в XR Plug-in Management | Включить, перезагрузить Editor |
| Шлем не видит APK | ARM64 не выбран / слишком высокий min API | Player Settings → Android |
| Чёрный экран в шлеме | Single Pass Instanced + шейдер | Переключить на Multi Pass |
| Не работает триггер | Поле `naborDeistvii` пустое на скриптах `Posadka*` | Назначить `InputActionAsset` |