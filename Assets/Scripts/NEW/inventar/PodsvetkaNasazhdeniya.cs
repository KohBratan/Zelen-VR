using UnityEngine;
using System.Collections.Generic;

// Подсветка насаждения через подмену материалов.
public class PodsvetkaNasazhdeniya : MonoBehaviour
{
    private Material materialPodsvetki;
    private bool podsvecheno = false;
    private Dictionary<Renderer, Material[]> ishodnye = new Dictionary<Renderer, Material[]>();

    public void Vkluchit(Material mat)
    {
        if (mat == null) return;

        materialPodsvetki = mat;
        podsvecheno = true;

        Renderer[] all = GetComponentsInChildren<Renderer>();
        foreach (var r in all)
        {
            if (r == null) continue;

            if (!ishodnye.ContainsKey(r))
            {
                ishodnye[r] = r.sharedMaterials;
            }

            Material[] cur = new Material[r.sharedMaterials.Length];
            for (int j = 0; j < cur.Length; j++) cur[j] = materialPodsvetki;
            r.sharedMaterials = cur;
        }
    }

    public void Vyklyuchit()
    {
        if (!podsvecheno) return;
        podsvecheno = false;

        foreach (var kvp in ishodnye)
        {
            if (kvp.Key != null)
            {
                kvp.Key.sharedMaterials = kvp.Value;
            }
        }
        ishodnye.Clear();
    }

    public void ObnovitPodsvetku()
    {
        if (podsvecheno && materialPodsvetki != null)
        {
            Vkluchit(materialPodsvetki);
        }
    }

    // ИСПРАВЛЕНИЕ: Теперь принимает массив рендереров, чтобы не трогать подложку
    public void ObnovitKesh(Renderer[] celRenderery, Material noviyMat)
    {
        if (!podsvecheno || noviyMat == null || celRenderery == null) return;

        foreach (var r in celRenderery)
        {
            if (r != null && ishodnye.ContainsKey(r))
            {
                // Запоминаем новый материал как оригинальный
                Material[] mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = noviyMat;
                ishodnye[r] = mats;

                // И возвращаем желтую подсветку
                Material[] yellow = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < yellow.Length; i++) yellow[i] = materialPodsvetki;
                r.sharedMaterials = yellow;
            }
        }
    }
}