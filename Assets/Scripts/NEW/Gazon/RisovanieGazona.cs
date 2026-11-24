using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class RisovanieGazona : MonoBehaviour
{
    [Header("Откуда идёт луч (если пусто - сам объект)")]
    public Transform tochkaStarta;

    [Header("Слой террейна")]
    public LayerMask maskaTerreina;

    [Header("Длина луча")]
    public float dlinaLucha = 30f;

    [Header("Размер ячейки сетки (метры)")]
    public float razmerYacheyki = 1f;

    [Header("Контейнер для газонов (необязательно)")]
    public Transform kontejnerGazonov;

    [Header("Материал луча и материал призрака")]
    public Material materialLucha;
    public Material materialPrizraka;

    [Header("Отдельный префаб-призрак (если пусто - создаётся Quad 1x1)")]
    public GameObject prefabPrizraka;

    [Header("Цвет луча: попал на свободную ячейку / не попал или ячейка занята")]
    public Color tsvetPopal = Color.green;
    public Color tsvetNePopal = Color.red;

    [Header("Весь asset с действиями")]
    public InputActionAsset naborDeistvii;

    [Header("Карта и имя действия триггера (Button)")]
    public string imyaKarty = "XRI Left Interaction";
    public string imyaDeistviya = "PaintGazon";

    private InputAction deistvieRisovaniya;
    private bool risuem = false;
    private LineRenderer luch;
    private Material materialLuchaKopiya;
    private Dictionary<Vector2Int, GameObject> yacheyki = new Dictionary<Vector2Int, GameObject>();

    private GameObject prizrak;

    void Awake()
    {
        luch = GetComponent<LineRenderer>();
        luch.positionCount = 2;
        luch.useWorldSpace = true;
        luch.startWidth = 0.01f;
        luch.endWidth = 0.01f;
        luch.startColor = Color.white;
        luch.endColor = Color.white;
        luch.enabled = false;

        if (materialLucha != null) luch.material = materialLucha;
        else { Shader shader = Shader.Find("Universal Render Pipeline/Unlit"); if (shader != null) luch.material = new Material(shader); }
        materialLuchaKopiya = luch.material;

        if (naborDeistvii == null) return;
        var karta = naborDeistvii.FindActionMap(imyaKarty);
        if (karta != null) deistvieRisovaniya = karta.FindAction(imyaDeistviya);
    }

    void OnEnable()
    {
        if (deistvieRisovaniya != null)
        {
            deistvieRisovaniya.performed += NachatRisovanie;
            deistvieRisovaniya.canceled += ZakonchitRisovanie;
            deistvieRisovaniya.Enable();
        }
    }

    void OnDisable()
    {
        if (deistvieRisovaniya != null)
        {
            deistvieRisovaniya.performed -= NachatRisovanie;
            deistvieRisovaniya.canceled -= ZakonchitRisovanie;
        }
        if (luch != null) luch.enabled = false;
        SpryatatPrizrak();
    }

    void OnDestroy() { UnichtozhitPrizrak(); }

    void LateUpdate()
    {
        GameObject prefab = VyborInstrumenta.TekushiyPrefab;
        bool etoGazon = prefab != null && prefab.GetComponent<GazonTile>() != null;

        if (!etoGazon)
        {
            luch.enabled = false;
            SpryatatPrizrak();
            return;
        }

        Transform start = tochkaStarta != null ? tochkaStarta : transform;
        Vector3 nachalo = start.position;
        Vector3 napravlenie = start.forward;
        Vector3 konets = nachalo + napravlenie * dlinaLucha;

        RaycastHit hit;
        if (Physics.Raycast(nachalo, napravlenie, out hit, dlinaLucha, ~0, QueryTriggerInteraction.Ignore))
        {
            konets = hit.point;

            int x = Mathf.FloorToInt(hit.point.x / razmerYacheyki);
            int z = Mathf.FloorToInt(hit.point.z / razmerYacheyki);
            Vector2Int klyuch = new Vector2Int(x, z);

            if (yacheyki.ContainsKey(klyuch) && yacheyki[klyuch] == null) yacheyki.Remove(klyuch);
            bool yacheykaSvobodna = !yacheyki.ContainsKey(klyuch);

            Vector3 tsentr = new Vector3((x + 0.5f) * razmerYacheyki, hit.point.y, (z + 0.5f) * razmerYacheyki);

            bool mozhnoStavit = yacheykaSvobodna;

            // ПРОВЕРКА: Если навелись на клумбу, проверяем, влезает ли квадрат газона целиком
            KlumbaTile klumba = hit.collider.GetComponentInParent<KlumbaTile>();
            if (klumba != null)
            {
                float half = razmerYacheyki / 2f;
                Vector3[] corners = {
                    tsentr + new Vector3(half, 0, half), tsentr + new Vector3(-half, 0, half),
                    tsentr + new Vector3(half, 0, -half), tsentr + new Vector3(-half, 0, -half)
                };

                // ИСПРАВЛЕНИЕ: Увеличили отступ до 20 см (0.2f), чтобы газон не залезал на забор
                float otstup = klumba.tolshinaZabora + 0.2f;
                float maxX = (klumba.shirina / 2f) - otstup;
                float maxZ = (klumba.dlina / 2f) - otstup;

                foreach (var c in corners)
                {
                    Vector3 localPos = klumba.transform.InverseTransformPoint(c);
                    if (Mathf.Abs(localPos.x) > maxX || Mathf.Abs(localPos.z) > maxZ)
                    {
                        mozhnoStavit = false; // Газон вылезает за забор - запрещаем!
                        break;
                    }
                }
            }

            if (mozhnoStavit)
            {
                PokazatPrizrak(tsentr);
                UstanovitTsvetLucha(tsvetPopal);
            }
            else
            {
                SpryatatPrizrak();
                UstanovitTsvetLucha(tsvetNePopal);
            }

            if (risuem && mozhnoStavit)
            {
                SozdatYacheyku(x, z, hit.point);
            }
        }
        else
        {
            SpryatatPrizrak();
            UstanovitTsvetLucha(tsvetNePopal);
        }

        luch.enabled = true;
        luch.SetPosition(0, nachalo);
        luch.SetPosition(1, konets);
    }

    private void NachatRisovanie(InputAction.CallbackContext context) { risuem = true; }
    private void ZakonchitRisovanie(InputAction.CallbackContext context) { risuem = false; }

    private void SozdatYacheyku(int x, int z, Vector3 tochka)
    {
        Vector2Int klyuch = new Vector2Int(x, z);

        if (yacheyki.ContainsKey(klyuch))
        {
            if (yacheyki[klyuch] != null) return;
            yacheyki.Remove(klyuch);
        }

        Vector3 tsentr = new Vector3((x + 0.5f) * razmerYacheyki, tochka.y, (z + 0.5f) * razmerYacheyki);

        GameObject gazon = Instantiate(VyborInstrumenta.TekushiyPrefab, tsentr, Quaternion.identity, kontejnerGazonov);
        gazon.name = "Gazon_" + x + "_" + z;

        GazonTile tile = gazon.GetComponent<GazonTile>();
        if (tile != null)
        {
            tile.dlina = razmerYacheyki;
            tile.shirina = razmerYacheyki;
            tile.maskaTerreina = maskaTerreina;
        }

        yacheyki.Add(klyuch, gazon);
    }

    private void PokazatPrizrak(Vector3 pozitsiya)
    {
        if (prizrak == null) SozdatPrizrak();
        if (prizrak != null) { prizrak.SetActive(true); prizrak.transform.position = pozitsiya + Vector3.up * 0.01f; }
    }

    private void SpryatatPrizrak() { if (prizrak != null) prizrak.SetActive(false); }

    private void SozdatPrizrak()
    {
        if (prefabPrizraka != null)
        {
            prizrak = Instantiate(prefabPrizraka);
            prizrak.name = prefabPrizraka.name + " (prizrak)";
            prizrak.transform.localScale = new Vector3(razmerYacheyki, 1f, razmerYacheyki);

            Collider[] kollajdery = prizrak.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < kollajdery.Length; i++) kollajdery[i].enabled = false;

            if (materialPrizraka != null)
            {
                Renderer[] otrisovshiki = prizrak.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < otrisovshiki.Length; i++)
                {
                    Material[] novyeMaterialy = new Material[otrisovshiki[i].sharedMaterials.Length];
                    for (int j = 0; j < novyeMaterialy.Length; j++) novyeMaterialy[j] = materialPrizraka;
                    otrisovshiki[i].sharedMaterials = novyeMaterialy;
                }
            }
        }
        else
        {
            prizrak = GameObject.CreatePrimitive(PrimitiveType.Quad);
            prizrak.name = "GazonPrizrak (Quad)";

            Collider col = prizrak.GetComponent<Collider>();
            if (col != null) Destroy(col);

            prizrak.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            prizrak.transform.localScale = new Vector3(razmerYacheyki, razmerYacheyki, 1f);

            if (materialPrizraka != null)
            {
                Renderer r = prizrak.GetComponent<Renderer>();
                if (r != null) r.material = materialPrizraka;
            }
        }
    }

    private void UnichtozhitPrizrak() { if (prizrak != null) Destroy(prizrak); prizrak = null; }

    private void UstanovitTsvetLucha(Color tsvet)
    {
        if (materialLuchaKopiya == null) return;
        if (materialLuchaKopiya.HasProperty("_BaseColor")) materialLuchaKopiya.SetColor("_BaseColor", tsvet);
        else if (materialLuchaKopiya.HasProperty("_Color")) materialLuchaKopiya.SetColor("_Color", tsvet);
    }
}