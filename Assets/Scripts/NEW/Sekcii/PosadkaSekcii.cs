using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(LineRenderer))]
public class PosadkaSekcii : MonoBehaviour
{
    public Transform tochkaStarta;
    public LayerMask maskaTerreina;
    public float dlinaLucha = 50f;
    
    public Material materialLucha;
    public Material materialPrizraka;
    
    public XRInteractorLineVisual[] standartnyeLuchi; 

    public InputActionAsset naborDeistvii;
    public string imyaKarty = "XRI Left Interaction";
    public string imyaDeistviya = "PlaceTool";

    private LineRenderer luch;
    private InputAction deistvie;
    private GameObject prizrak;
    private Vector3 tochkaPopadaniya;
    private GameObject prefabSekcii;
    private bool mozhnPostavit = false;

    void Awake()
    {
        luch = GetComponent<LineRenderer>();
        luch.positionCount = 2; luch.useWorldSpace = true; luch.startWidth = 0.05f; luch.endWidth = 0.05f;
        luch.enabled = false;

        if (materialLucha != null) luch.material = materialLucha;
        else { Shader s = Shader.Find("Universal Render Pipeline/Unlit"); if (s != null) luch.material = new Material(s); }

        if (naborDeistvii != null)
        {
            var k = naborDeistvii.FindActionMap(imyaKarty);
            if (k != null) deistvie = k.FindAction(imyaDeistviya);
        }
    }

    void OnEnable() { if (deistvie != null) { deistvie.performed += Postavit; deistvie.Enable(); } }
    void OnDisable() { if (deistvie != null) deistvie.performed -= Postavit; if (luch != null) luch.enabled = false; UnichtozhitPrizrak(); }

    void LateUpdate()
    {
        prefabSekcii = VyborInstrumenta.TekushiyPrefab;
        bool etoSekciya = prefabSekcii != null && prefabSekcii.GetComponent<UniversalSekciya>() != null;
        
        if (!etoSekciya || SostoyanieMenyu.AnyMenuOpen)
        {
            luch.enabled = false;
            UnichtozhitPrizrak();
            VklyuchitStandartnyeLuchi(true);
            return;
        }

        VklyuchitStandartnyeLuchi(false);

        Transform start = tochkaStarta != null ? tochkaStarta : transform;
        Vector3 nachalo = start.position, napravlenie = start.forward;
        
        if (maskaTerreina == 0) { luch.enabled = true; luch.SetPosition(0, nachalo); luch.SetPosition(1, nachalo + napravlenie * dlinaLucha); UstanovitTsvet(luch.material, Color.red); return; }

        if (Physics.Raycast(nachalo, napravlenie, out RaycastHit hit, dlinaLucha, maskaTerreina, QueryTriggerInteraction.Ignore))
        {
            tochkaPopadaniya = hit.point;
            mozhnPostavit = ProveritMesto(tochkaPopadaniya, prefabSekcii.GetComponent<UniversalSekciya>());

            luch.enabled = true;
            luch.SetPosition(0, nachalo);
            luch.SetPosition(1, hit.point);
            
            if (mozhnPostavit)
            {
                UstanovitTsvet(luch.material, Color.green);
                PokazatPrizrak(prefabSekcii, tochkaPopadaniya);
            }
            else
            {
                UstanovitTsvet(luch.material, Color.red);
                PokazatPrizrak(prefabSekcii, tochkaPopadaniya); // Показываем призрак красным, чтобы видел, куда не влезает
            }
        }
        else
        {
            mozhnPostavit = false;
            luch.enabled = true; luch.SetPosition(0, nachalo); luch.SetPosition(1, nachalo + napravlenie * dlinaLucha); UstanovitTsvet(luch.material, Color.red);
            UnichtozhitPrizrak();
        }
    }

    private bool ProveritMesto(Vector3 centr, UniversalSekciya sek)
    {
        // 1. Проверка на пустоту (свисание с края) - 4 луча по углам
        Vector3 halfSize = new Vector3(sek.shirina / 2f - 0.5f, 1f, sek.dlina / 2f - 0.5f);
        Vector3[] corners = {
            centr + new Vector3(halfSize.x, 0, halfSize.z), centr + new Vector3(-halfSize.x, 0, halfSize.z),
            centr + new Vector3(halfSize.x, 0, -halfSize.z), centr + new Vector3(-halfSize.x, 0, -halfSize.z)
        };
        foreach (var corner in corners)
        {
            if (!Physics.Raycast(corner + Vector3.up * 10f, Vector3.down, out _, 20f, maskaTerreina)) return false; // Угол висит в воздухе
        }

        // 2. Проверка на пересечение (OverlapBox)
        Collider[] hits = Physics.OverlapBox(centr + Vector3.up * 1f, new Vector3(sek.shirina / 2f - 0.5f, 1f, sek.dlina / 2f - 0.5f), Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (h.GetComponentInParent<UniversalSekciya>() != null || h.GetComponentInParent<KlumbaTile>() != null) return false;
        }

        return true;
    }

    private void Postavit(InputAction.CallbackContext ctx)
    {
        if (prefabSekcii != null && prefabSekcii.GetComponent<UniversalSekciya>() != null)
        {
            if (luch.enabled && prizrak != null && mozhnPostavit) 
            {
                GameObject novaya = Instantiate(prefabSekcii, tochkaPopadaniya, Quaternion.identity);
                novaya.name = "Universalnaya_Sekciya";
            }
            VyborInstrumenta.TekushiyPrefab = null;
        }
    }

    private void PokazatPrizrak(GameObject prefab, Vector3 pos)
    {
        if (prizrak == null)
        {
            prizrak = Instantiate(prefab);
            prizrak.name = "Prizrak_Sekcii";
            UniversalSekciya sek = prizrak.GetComponent<UniversalSekciya>();
            if (sek != null) sek.etoPrizrak = true; 
            foreach (var r in prizrak.GetComponentsInChildren<Renderer>()) r.material = materialPrizraka;
        }
        prizrak.SetActive(true);
        prizrak.transform.position = pos;
    }

    private void UnichtozhitPrizrak() { if (prizrak != null) { Destroy(prizrak); prizrak = null; } }
    private void UstanovitTsvet(Material mat, Color c) { if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c); }
    private void VklyuchitStandartnyeLuchi(bool vklyuchit) { if (standartnyeLuchi != null) foreach (var line in standartnyeLuchi) if (line != null) line.enabled = vklyuchit; }
}
