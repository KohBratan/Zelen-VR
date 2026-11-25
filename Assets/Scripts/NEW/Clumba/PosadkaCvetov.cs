using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class PosadkaCvetov : MonoBehaviour
{
    [Header("Параметры луча")]
    public Transform tochkaStarta;
    public float dlinaLucha = 30f;
    public Material materialLucha;
    public Material materialPrizraka;
    public Color tsvetPopal = Color.green;
    public Color tsvetNePopal = Color.red;

    [Header("Параметры кисти (рисование точками)")]
    [Range(0.1f, 3f)]
    public float diametrKisti = 0.5f; // диаметр = ширина / 2
    public float minimalnoeRasstoyanie = 0.15f; // между точками при рисовании
    public float skorostPosadki = 0.05f; // скорость «рисования» кистью

    [Header("Индикатор кисти")]
    public GameObject prefabIndikatoraKisti; // Cylinder / Quad с прозрачным материалом
    private GameObject indikatorKisti;

    [Header("Ввод")]
    public InputActionAsset naborDeistvii;
    public string imyaKarty = "XRI Left Interaction";
    public string imyaDeistviya = "PlaceFlower";

    private LineRenderer luch;
    private Material matLuchKopiya;
    private InputAction deistvie;
    private GameObject prizrak;
    private GameObject prefabPrizrak;
    private KlumbaTile tekushayaKlumba;
    private Vector3 tochkaPopadaniya;

    private bool risuem = false;
    private float vremyaSleduyuscheyPosadki = 0f;

    void Awake()
    {
        luch = GetComponent<LineRenderer>();
        luch.positionCount = 2; luch.useWorldSpace = true; luch.startWidth = 0.01f; luch.endWidth = 0.01f;
        luch.enabled = false;

        if (materialLucha != null) luch.material = materialLucha;
        else { Shader s = Shader.Find("Universal Render Pipeline/Unlit"); if (s != null) luch.material = new Material(s); }
        matLuchKopiya = luch.material;

        if (naborDeistvii != null)
        {
            var k = naborDeistvii.FindActionMap(imyaKarty);
            if (k != null)
            {
                deistvie = k.FindAction(imyaDeistviya);
            }
        }

        // Создаём индикатор кисти, если он задан
        if (prefabIndikatoraKisti != null)
        {
            indikatorKisti = Instantiate(prefabIndikatoraKisti);
            indikatorKisti.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (deistvie != null)
        {
            deistvie.performed += NachatPosadku;
            deistvie.canceled += ZakonchitPosadku;
            deistvie.Enable();
        }
    }

    void OnDisable()
    {
        if (deistvie != null)
        {
            deistvie.performed -= NachatPosadku;
            deistvie.canceled -= ZakonchitPosadku;
        }
        if (luch != null) luch.enabled = false;
        SpryatatPrizrak();
        if (indikatorKisti != null) indikatorKisti.SetActive(false);
    }

    void LateUpdate()
    {
        GameObject prefab = VyborInstrumenta.TekushiyPrefab;

        if (prefab == null)
        {
            if (luch.enabled) luch.enabled = false;
            SpryatatPrizrak();
            if (indikatorKisti != null) indikatorKisti.SetActive(false);
            tekushayaKlumba = null;
            return;
        }

        Nasazhdenie nas = prefab.GetComponent<Nasazhdenie>();

        // Разрешаем посадку любых насаждений (деревья, кусты, газоны, цветы),
        // запрещаем только секции.
        if (nas == null || nas.kategoriya == "Секции")
        {
            if (luch.enabled) luch.enabled = false;
            SpryatatPrizrak();
            if (indikatorKisti != null) indikatorKisti.SetActive(false);
            tekushayaKlumba = null;
            return;
        }

        if (!luch.enabled) luch.enabled = true;

        Transform start = tochkaStarta != null ? tochkaStarta : transform;
        Vector3 nachalo = start.position, napravlenie = start.forward, konets = nachalo + napravlenie * dlinaLucha;
        bool popal = false;

        if (Physics.Raycast(nachalo, napravlenie, out RaycastHit hit, dlinaLucha, ~0, QueryTriggerInteraction.Ignore))
        {
            KlumbaTile klumba = hit.collider.GetComponentInParent<KlumbaTile>();
            if (klumba != null)
            {
                Vector3 localHit = klumba.transform.InverseTransformPoint(hit.point);

                float otstup = klumba.tolshinaZabora + 0.02f;
                float maxX = (klumba.shirina / 2f) - otstup;
                float maxZ = (klumba.dlina / 2f) - otstup;

                if (Mathf.Abs(localHit.x) <= maxX && Mathf.Abs(localHit.z) <= maxZ)
                {
                    popal = true;
                    tekushayaKlumba = klumba;

                    if (Physics.Raycast(hit.point + Vector3.up * 2f, Vector3.down, out RaycastHit groundHit, 5f, klumba.maskaTerreina))
                    {
                        tochkaPopadaniya = groundHit.point;
                        konets = groundHit.point;
                    }
                    else
                    {
                        tochkaPopadaniya = hit.point;
                        konets = hit.point;
                    }

                    PokazatPrizrak(prefab, tochkaPopadaniya);

                    // Обновляем индикатор кисти на земле
                    if (indikatorKisti != null)
                    {
                        indikatorKisti.SetActive(true);
                        indikatorKisti.transform.position = tochkaPopadaniya + Vector3.up * 0.02f;
                        // Масштабируем префаб-цилиндр под диаметр кисти (предполагается, что базовый диаметр = 1)
                        indikatorKisti.transform.localScale = new Vector3(diametrKisti, 0.01f, diametrKisti);
                    }

                    // Если зажата кнопка и прошёл интервал - рисуем точку
                    if (risuem && Time.time >= vremyaSleduyuscheyPosadki)
                    {
                        PosaditKistyu(prefab, tekushayaKlumba);
                        vremyaSleduyuscheyPosadki = Time.time + skorostPosadki;
                    }
                }
                else
                {
                    SpryatatPrizrak();
                    if (indikatorKisti != null) indikatorKisti.SetActive(false);
                }
            }
            else
            {
                SpryatatPrizrak();
                if (indikatorKisti != null) indikatorKisti.SetActive(false);
            }
        }
        else
        {
            SpryatatPrizrak();
            if (indikatorKisti != null) indikatorKisti.SetActive(false);
        }

        luch.SetPosition(0, nachalo);
        luch.SetPosition(1, konets);
        UstanovitTsvetLucha(popal ? tsvetPopal : tsvetNePopal);
    }

    private void NachatPosadku(InputAction.CallbackContext ctx)
    {
        risuem = true;
        // Первая точка ставится сразу при нажатии
        if (VyborInstrumenta.TekushiyPrefab != null && tekushayaKlumba != null)
        {
            Nasazhdenie nas = VyborInstrumenta.TekushiyPrefab.GetComponent<Nasazhdenie>();
            // Секции не сажаем, всё остальное (деревья/кусты/газоны/цветы) - можно.
            if (nas != null && nas.kategoriya != "Секции")
            {
                PosaditOdinochno(VyborInstrumenta.TekushiyPrefab, tekushayaKlumba, tochkaPopadaniya);
            }
        }
    }

    private void ZakonchitPosadku(InputAction.CallbackContext ctx)
    {
        risuem = false;
    }

    // Одиночная посадка (для одиночного клика)
    private void PosaditOdinochno(GameObject prefab, KlumbaTile klumba, Vector3 pos)
    {
        GameObject novoe = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0, 360), 0), klumba.kontejnerCvetov);
        Nasazhdenie nas = novoe.GetComponent<Nasazhdenie>();
        if (nas != null) nas.sourcePrefab = prefab;
    }

    // Кистевая посадка (несколько точек в радиусе с проверкой, чтобы не ставить друг на друга)
    private void PosaditKistyu(GameObject prefab, KlumbaTile klumba)
    {
        int popytki = 0;
        float radius = diametrKisti / 2f;

        while (popytki < 10) // несколько попыток найти свободную точку в пятне кисти
        {
            popytki++;

            // Берём случайную точку внутри круга кисти
            Vector2 sluchaynayaTochka = Random.insideUnitCircle * radius;
            Vector3 mirovayaTochka = tochkaPopadaniya + new Vector3(sluchaynayaTochka.x, 0, sluchaynayaTochka.y);

            // Проверяем, не вышли ли мы за стенки клумбы
            Vector3 localPos = klumba.transform.InverseTransformPoint(mirovayaTochka);
            float otstup = klumba.tolshinaZabora + 0.02f;
            if (Mathf.Abs(localPos.x) > (klumba.shirina / 2f) - otstup || Mathf.Abs(localPos.z) > (klumba.dlina / 2f) - otstup)
                continue; // точка за бортом — пропускаем

            // Ставим точку на землю
            if (Physics.Raycast(mirovayaTochka + Vector3.up * 2f, Vector3.down, out RaycastHit groundHit, 5f, klumba.maskaTerreina))
            {
                mirovayaTochka = groundHit.point;
            }

            // Проверяем, что рядом нет других цветов
            bool mestoSvobodno = true;
            foreach (Transform child in klumba.kontejnerCvetov)
            {
                if (Vector3.Distance(child.position, mirovayaTochka) < minimalnoeRasstoyanie)
                {
                    mestoSvobodno = false;
                    break;
                }
            }

            if (mestoSvobodno)
            {
                PosaditOdinochno(prefab, klumba, mirovayaTochka);
                break; // точку поставили — выходим
            }
        }
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
