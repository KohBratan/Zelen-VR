using UnityEngine;

// ƒобавл€етс€ на посаженное насаждение автоматически (из VyborNasazhdeniya).
// «апоминает исходные материалы и по команде подмен€ет их на материал подсветки.
public class PodsvetkaNasazhdeniya : MonoBehaviour
{
    private Renderer[] otrisovshiki;
    private Material[][] ishodnyeMaterialy;
    private bool podsvecheno = false;

    void Awake()
    {
        otrisovshiki = GetComponentsInChildren<Renderer>(true);
        ishodnyeMaterialy = new Material[otrisovshiki.Length][];

        for (int i = 0; i < otrisovshiki.Length; i++)
        {
            ishodnyeMaterialy[i] = otrisovshiki[i].sharedMaterials;
        }
    }

    public void Vkluchit(Material materialPodsvetki)
    {
        if (podsvecheno || materialPodsvetki == null)
        {
            return;
        }

        for (int i = 0; i < otrisovshiki.Length; i++)
        {
            Material[] novye = new Material[ishodnyeMaterialy[i].Length];
            for (int j = 0; j < novye.Length; j++)
            {
                novye[j] = materialPodsvetki;
            }

            otrisovshiki[i].sharedMaterials = novye;
        }

        podsvecheno = true;
    }

    public void Vyklyuchit()
    {
        if (!podsvecheno)
        {
            return;
        }

        for (int i = 0; i < otrisovshiki.Length; i++)
        {
            if (otrisovshiki[i] != null)
            {
                otrisovshiki[i].sharedMaterials = ishodnyeMaterialy[i];
            }
        }

        podsvecheno = false;
    }
}