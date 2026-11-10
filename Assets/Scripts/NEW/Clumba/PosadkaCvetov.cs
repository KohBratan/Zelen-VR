// ---------------------------------------------------------------------------
// Zelen-VR
// File: PosadkaCvetov.cs
// Posadka tsvetov na klumbe.
// ---------------------------------------------------------------------------
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

    private LineRenderer luch;
    private Material matLuchKopiya;
    private InputAction deistvie;
    private GameObject prizrak;
    private GameObject prefabPrizrak;
    private KlumbaTile tekushayaKlumba;
    private Vector3 tochkaPopadaniya;

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

    void OnEnable() { if (deistvie != null) { deistvie.performed += Posadit; deistvie.Enable(); } }
    void OnDisable() { if (deistvie != null) deistvie.performed -= Posadit; if (luch != null) luch.enabled = false; SpryatatPrizrak(); }

    void LateUpdate()
    {
        GameObject prefab = VyborInstrumenta.TekushiyPrefab;

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

        if (!luch.enabled) luch.enabled = true;

        Transform start = tochkaStarta != null ? tochkaStarta : transform;
        Vector3 nachalo = start.position, napravlenie = start.forward, konets = nachalo + napravlenie * dlinaLucha;
        bool popal = false;

        if (Physics.Raycast(nachalo, napravlenie, out RaycastHit hit, dlinaLucha, ~0, QueryTriggerInteraction.Ignore))
        {
            KlumbaTile klumba = hit.collider.GetComponentInParent<KlumbaTile>();
            if (klumba != null)
            {
                // ПРОВЕРКА: не попали ли мы в заборчик
                Vector3 localHit = klumba.transform.InverseTransformPoint(hit.point);

                // Оставляем отступ от края (толщина забора + 2 см)
                float otstup = klumba.tolshinaZabora + 0.02f;
                float maxX = (klumba.shirina / 2f) - otstup;
                float maxZ = (klumba.dlina / 2f) - otstup;

                if (Mathf.Abs(localHit.x) <= maxX && Mathf.Abs(localHit.z) <= maxZ)
                {
                    // Попали внутрь клумбы
                    popal = true;
                    tekushayaKlumba = klumba;
                    tochkaPopadaniya = hit.point;
                    konets = hit.point;
                    PokazatPrizrak(prefab, hit.point);
                }
                else
                {
                    // Попали в заборчик или слишком близко к краю
                    popal = false;
                    SpryatatPrizrak();
                }
            }
            else SpryatatPrizrak();
        }
        else SpryatatPrizrak();

        luch.SetPosition(0, nachalo);
        luch.SetPosition(1, konets);
        UstanovitTsvetLucha(popal ? tsvetPopal : tsvetNePopal);
    }

    private void Posadit(InputAction.CallbackContext ctx)
    {
        if (VyborInstrumenta.TekushiyPrefab != null && tekushayaKlumba != null)
        {
            GameObject novoe = Instantiate(VyborInstrumenta.TekushiyPrefab, tochkaPopadaniya, Quaternion.Euler(0, Random.Range(0, 360), 0), tekushayaKlumba.kontejnerCvetov);
            novoe.name = VyborInstrumenta.TekushiyPrefab.name;

            Nasazhdenie nas = novoe.GetComponent<Nasazhdenie>();
            if (nas != null) nas.sourcePrefab = VyborInstrumenta.TekushiyPrefab;
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