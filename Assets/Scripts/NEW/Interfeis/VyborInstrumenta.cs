using UnityEngine;

// Общее место, где хранится, какое насаждение сейчас выбрано в меню.
// Позже скрипт расстановки будет брать префаб отсюда.
public static class VyborInstrumenta
{
    public static GameObject TekushiyPrefab;

    // Сбрасываем выбор при старте игры (на случай отключённого Domain Reload)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Sbrosit()
    {
        TekushiyPrefab = null;
    }
}