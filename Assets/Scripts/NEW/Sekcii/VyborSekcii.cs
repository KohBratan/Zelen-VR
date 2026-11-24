using System.Collections.Generic;
using UnityEngine;

public static class VyborSekcii
{
    public static List<GameObject> Derevya = new List<GameObject>();
    public static List<GameObject> Kusty = new List<GameObject>();

    // Плотность (0 - пусто, 0.5 - стандарт, 1 - густо)
    public static float Plotnost = 0.5f;

    public static bool EstVybor()
    {
        return Derevya.Count > 0 || Kusty.Count > 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Sbrosit()
    {
        Derevya.Clear();
        Kusty.Clear();
        Plotnost = 0.5f;
    }
}