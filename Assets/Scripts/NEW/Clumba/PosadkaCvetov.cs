using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PosadkaCvetov : MonoBehaviour
{
    public Transform tochkaStarta;
    public float dlinaLucha = 30f;
    public Material materialLucha;
    public Material materialPrizraka;
    public Color tsvetPopal = Color.green;
    public Color tsvetNePopal = Color.red;

    [Header("Ввод")]
    public InputActionAsset naborDeistvii;
    public string imyaKarty = "XRI Left Interaction";
    public string imyaDeistviya = "PlaceFlower";

<<<<<<< HEAD
    [Header("Радиус поиска уже посаженного цветка (м)")]
    public float radiusSushchestvuyushchego = 0.4f;

    [Header("Двойной клик для выбора (сек)")]
    public float oknoDvoynogoKlika = 0.35f;
    public float mnozhitelPodsvetki = 1.15f;

=======
>>>>>>> 80acacfca905617c4b4f9a11aa0eea92352ec13c
    private LineRenderer luch;
    private Material matLuchKopiya;
    private InputAction deistvie;
    private GameObject prizrak;
    private GameObject prefabPrizrak;
    private KlumbaTile tekushayaKlumba;
    private Vector3 tochkaPopadaniya;
<<<<<<< HEAD
    private bool moznoPosadit = false;

    // Для дабл-клика
    private float vremyaPoslednegoKlika = -10f;
    private GameObject kandidatVybor;
    private GameObject vybrannoe;
=======
>>>>>>> 80acacfca905617c4b4f9a11aa0eea92352ec13c

    void Awake()
    {
        luch = GetComponent<LineRenderer>();
        luch.positionCount = 2;
        luch.useWorldSpace = true;
        luch.startWidth = 0.01f;
        luch.endWidth = 0.01f;
        luch.enabled = false;

        if (materialLucha != null) luch.material = materialLucha;
        else { Shader s = Shader.Find("Universal Render Pipeline/Unlit"); if (s != null) luch.material = new Material(s); }
        matLuchKopiya = luch.material;

        if (naborDeistvii != null)
        {
            var k = naborDeistvii.FindActionMap(imyaKarty);
            if (k != null) deistvie = k.FindAction(imyaDeistviya);
        }
    }

<<<<<<< HEAD
    void OnEnable() { if (deistvie != null) { deistvie.performed += ObrabotatNazhatie; deistvie.Enable(); } }
    void OnDisable() { if (deistvie != null) deistvie.performed -= ObrabotatNazhatie; if (luch != null) luch.enabled = false; SpryatatPrizrak(); }
=======
    void OnEnable() { if (deistvie != null) { deistvie.performed += Posadit; deistvie.Enable(); } }
    void OnDisable() { if (deistvie != null) deistvie.performed -= Posadit; if (luch != null) luch.enabled = false; SpryatatPrizrak(); }
>>>>>>> 80acacfca905617c4b4f9a11aa0eea92352ec13c

    void LateUpdate()
    {
        GameObject prefab = VyborInstrumenta.TekushiyPrefab;

<<<<<<< HEAD
        if (prefab == null) { if (luch.enabled) luch.enabled = false; SpryatatPrizrak(); return; }

        Nasazhdenie nas = prefab.GetComponent<Nasazhdenie>();
        if (nas == null || nas.kategoriya != "Цветы") { if (luch.enabled) luch.enabled = false; SpryatatPrizrak(); return; }
=======
        if (prefab == null)
        {
            if (luch.enabled) luch.enabled = false;
            SpryatatPrizrak();
            return;
        }

        Nasazhdenie nas = prefab.GetComponent<Nasazhdenie>();
        if (nas == null || nas.kategoriya != "Цветы")
        {
            if (luch.enabled) luch.enabled = false;
            SpryatatPrizrak();
            return;
        }

        if (SostoyanieMenyu.AnyMenuOpen)
        {
            if (luch.enabled) luch.enabled = false;
            SpryatatPrizrak();
            return;
        }
>>>>>>> 80acacfca905617c4b4f9a11aa0eea92352ec13c

        if (!luch.enabled) luch.enabled = true;

        Transform start = tochkaStarta != null ? tochkaStarta : transform;
        Vector3 nachalo = start.position, napravlenie = start.forward, konets = nachalo + napravlenie * dlinaLucha;
        bool popal = false;
<<<<<<< HEAD
        moznoPosadit = false;
=======
>>>>>>> 80acacfca905617c4b4f9a11aa0eea92352ec13c

        if (Physics.Raycast(nachalo, napravlenie, out RaycastHit hit, dlinaLucha, ~0, QueryTriggerInteraction.Ignore))
        {
            KlumbaTile klumba = hit.collider.GetComponentInParent<KlumbaTile>();
            if (klumba != null)
            {
<<<<<<< HEAD
                Vector3 localHit = klumba.transform.InverseTransformPoint(hit.point);
                // Учитываем реальную толщину забора (кладка из кубиков)
                float otstup = Mathf.Max(klumba.tolshinaZabora, klumba.razmerKubika * 0.5f) + 0.02f;
=======
                // ПРОВЕРКА: не попали ли мы в заборчик
                Vector3 localHit = klumba.transform.InverseTransformPoint(hit.point);

                // Оставляем отступ от края (толщина забора + 2 см)
                float otstup = klumba.tolshinaZabora + 0.02f;
>>>>>>> 80acacfca905617c4b4f9a11aa0eea92352ec13c
                float maxX = (klumba.shirina / 2f) - otstup;
                float maxZ = (klumba.dlina / 2f) - otstup;

                if (Mathf.Abs(localHit.x) <= maxX && Mathf.Abs(localHit.z) <= maxZ)
                {
<<<<<<< HEAD
=======
                    // Попали внутрь клумбы
>>>>>>> 80acacfca905617c4b4f9a11aa0eea92352ec13c
                    popal = true;
                    tekushayaKlumba = klumba;
                    tochkaPopadaniya = hit.point;
                    konets = hit.point;
<<<<<<< HEAD

                    GameObject uzheEst = NaytiTsvetok(klumba, hit.point, radiusSushchestvuyushchego);
                    moznoPosadit = (uzheEst == null);

                    if (moznoPosadit) PokazatPrizrak(prefab, hit.point);
                    else SpryatatPrizrak();
                }
                else
                {
=======
                    PokazatPrizrak(prefab, hit.point);
                }
                else
                {
                    // Попали в заборчик или слишком близко к краю
>>>>>>> 80acacfca905617c4b4f9a11aa0eea92352ec13c
                    popal = false;
                    SpryatatPrizrak();
                }
            }
            else SpryatatPrizrak();
        }
        else SpryatatPrizrak();

        luch.SetPosition(0, nachalo);
        luch.SetPosition(1, konets);
<<<<<<< HEAD
        UstanovitTsvetLucha((popal && moznoPosadit) ? tsvetPopal : tsvetNePopal);
    }

    // Обработка нажатия: либо сажаем, либо ловим дабл-клик на выбор
    private void ObrabotatNazhatie(InputAction.CallbackContext ctx)
    {
        if (VyborInstrumenta.TekushiyPrefab == null || tekushayaKlumba == null) return;

        GameObject uzheEst = NaytiTsvetok(tekushayaKlumba, tochkaPopadaniya, radiusSushchestvuyushchego);

        // Если попали в уже существующий цветок — логика выбора (дабл-клик)
        if (uzheEst != null)
        {
            float now = Time.time;
            if (kandidatVybor == uzheEst && now - vremyaPoslednegoKlika <= oknoDvoynogoKlika)
            {
                Vybrat(uzheEst);
                vremyaPoslednegoKlika = -10f;
                kandidatVybor = null;
            }
            else
            {
                kandidatVybor = uzheEst;
                vremyaPoslednegoKlika = now;
            }
            return; // На существующий цветок новый не сажаем
        }

        // Если попали на пустое место — сажаем
        if (!moznoPosadit) return;

        GameObject novoe = Instantiate(VyborInstrumenta.TekushiyPrefab, tochkaPopadaniya, Quaternion.Euler(0, Random.Range(0, 360), 0), tekushayaKlumba.kontejnerCvetov);
        novoe.name = VyborInstrumenta.TekushiyPrefab.name;

        Nasazhdenie nas = novoe.GetComponent<Nasazhdenie>();
        if (nas != null) nas.sourcePrefab = VyborInstrumenta.TekushiyPrefab;

        moznoPosadit = false;
    }

    private void Vybrat(GameObject tsvetok)
    {
        SnatPodsvetku();
        vybrannoe = tsvetok;
        if (vybrannoe == null) return;

        vybrannoe.transform.localScale = vybrannoe.transform.localScale * mnozhitelPodsvetki;
        Debug.Log("[Vybor] Выбран цветок: " + vybrannoe.name);
    }

    private void SnatPodsvetku()
    {
        if (vybrannoe == null) return;
        vybrannoe.transform.localScale = vybrannoe.transform.localScale / mnozhitelPodsvetki;
        vybrannoe = null;
    }

    private GameObject NaytiTsvetok(KlumbaTile klumba, Vector3 tochka, float radius)
    {
        if (klumba.kontejnerCvetov == null) return null;
        GameObject blizhayshiy = null;
        float minSqr = radius * radius;
        foreach (Transform t in klumba.kontejnerCvetov)
        {
            if (t == null) continue;
            float d = (t.position - tochka).sqrMagnitude;
            if (d < minSqr) { minSqr = d; blizhayshiy = t.gameObject; }
        }
        return blizhayshiy;
=======
        UstanovitTsvetLucha(popal ? tsvetPopal : tsvetNePopal);
    }

    private void Posadit(InputAction.CallbackContext ctx)
    {
        if (SostoyanieMenyu.AnyMenuOpen) return;
        if (VyborInstrumenta.TekushiyPrefab != null && tekushayaKlumba != null)
        {
            GameObject novoe = Instantiate(VyborInstrumenta.TekushiyPrefab, tochkaPopadaniya, Quaternion.Euler(0, Random.Range(0, 360), 0), tekushayaKlumba.kontejnerCvetov);
            novoe.name = VyborInstrumenta.TekushiyPrefab.name;

            Nasazhdenie nas = novoe.GetComponent<Nasazhdenie>();
            if (nas != null) nas.sourcePrefab = VyborInstrumenta.TekushiyPrefab;
        }
>>>>>>> 80acacfca905617c4b4f9a11aa0eea92352ec13c
    }

    private void UstanovitTsvetLucha(Color c)
    {
        if (matLuchKopiya == null) return;
        if (matLuchKopiya.HasProperty("_BaseColor")) matLuchKopiya.SetColor("_BaseColor", c);
        else if (matLuchKopiya.HasProperty("_Color")) matLuchKopiya.SetColor("_Color", c);
    }

    private void PokazatPrizrak(GameObject prefab, Vector3 pos)
    {
        if (prefabPrizrak != prefab) { UnichtozhitPrizrak(); SozdatPrizrak(prefab); }
        if (prizrak != null) { prizrak.SetActive(true); prizrak.transform.position = pos; }
    }

    private void SpryatatPrizrak() { if (prizrak != null) prizrak.SetActive(false); }

    private void SozdatPrizrak(GameObject prefab)
    {
        prefabPrizrak = prefab;
        prizrak = Instantiate(prefab);
        Collider[] cols = prizrak.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols) c.enabled = false;

        if (materialPrizraka != null)
        {
            Renderer[] rends = prizrak.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
            {
                Material[] mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = materialPrizraka;
                r.sharedMaterials = mats;
            }
        }
    }

    private void UnichtozhitPrizrak() { if (prizrak != null) Destroy(prizrak); prizrak = null; prefabPrizrak = null; }
}