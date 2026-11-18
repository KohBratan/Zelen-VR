using UnityEngine;

// Общая проверка запрета посадки в существующие клумбы.
// Используется для деревьев, кустов и газонов.
public static class ZapretPosadkiVKlumbu
{
    private const float VysotaProverki = 0.25f;
    private const float RadiusProverki = 0.35f;
    private static readonly Collider[] rezultaty = new Collider[32];

    public static bool TochkaVKlumbe(Vector3 tochka)
    {
        int kolichestvo = Physics.OverlapSphereNonAlloc(
            tochka + Vector3.up * VysotaProverki,
            RadiusProverki,
            rezultaty,
            ~0,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < kolichestvo; i++)
        {
            Collider kollajder = rezultaty[i];
            if (kollajder != null && kollajder.GetComponentInParent<KlumbaTile>() != null)
            {
                return true;
            }
        }

        return false;
    }

    public static bool PryamougolnikVKlumbe(Vector3 tsentr, float shirina, float dlina)
    {
        float polovinaShiriny = shirina * 0.5f;
        float polovinaDliny = dlina * 0.5f;

        return TochkaVKlumbe(tsentr) ||
               TochkaVKlumbe(tsentr + new Vector3(-polovinaShiriny, 0f, -polovinaDliny)) ||
               TochkaVKlumbe(tsentr + new Vector3(-polovinaShiriny, 0f, polovinaDliny)) ||
               TochkaVKlumbe(tsentr + new Vector3(polovinaShiriny, 0f, -polovinaDliny)) ||
               TochkaVKlumbe(tsentr + new Vector3(polovinaShiriny, 0f, polovinaDliny));
    }
}
