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

    [Header("����������� ���������� ����� ������� (0 - ��� ��������)")]
    public float minimalnoeRasstoyanieMezhduCvetami = 0.1f;

    [Header("����")]
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
    private bool mozhnoPosadit;

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
        // ���������� ��������� ������������ �� ������ �����. ����� �����
        // ������� ��������� �� ������ ������ ���������� ������ ����� �
        // ���������� �����, ���� ����� ��� ��� ������ �� ��������.
        mozhnoPosadit = false;
        tekushayaKlumba = null;

        GameObject prefab = VyborInstrumenta.TekushiyPrefab;

        if (prefab == null)
        {
            if (luch.enabled) luch.enabled = false;
            SpryatatPrizrak();
            return;
        }

        Nasazhdenie nas = prefab.GetComponent<Nasazhdenie>();
        if (nas == null || nas.kategoriya != "�����")
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
                // ��������: �� ������ �� �� � ��������
                Vector3 localHit = klumba.transform.InverseTransformPoint(hit.point);

                // ��������� ������ �� ���� (������� ������ + 2 ��)
                float otstup = klumba.tolshinaZabora + 0.02f;
                float maxX = (klumba.shirina / 2f) - otstup;
                float maxZ = (klumba.dlina / 2f) - otstup;

                if (Mathf.Abs(localHit.x) <= maxX && Mathf.Abs(localHit.z) <= maxZ)
                {
                    // ������ ������ ������
                    popal = true;
                    mozhnoPosadit = true;
                    tekushayaKlumba = klumba;
                    tochkaPopadaniya = hit.point;
                    konets = hit.point;
                    PokazatPrizrak(prefab, hit.point);
                }
                else
                {
                    // ������ � �������� ��� ������� ������ � ����
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
        // ������ ������ ���� � ������� ����� ��� ������������� ����� ������
        // ������. ��� ����� ����� ����� ������� ��������� ����� �������������
        // � ����� � ��� �� ����� ��� ������ ������� ��������.
        if (!mozhnoPosadit) return;
        if (VyborInstrumenta.TekushiyPrefab == null || tekushayaKlumba == null) return;

        if (EstCvetRyadom()) return;

        GameObject novoe = Instantiate(VyborInstrumenta.TekushiyPrefab, tochkaPopadaniya, Quaternion.Euler(0, Random.Range(0, 360), 0), tekushayaKlumba.kontejnerCvetov);
        novoe.name = VyborInstrumenta.TekushiyPrefab.name;

        Nasazhdenie nas = novoe.GetComponent<Nasazhdenie>();
        if (nas != null) nas.sourcePrefab = VyborInstrumenta.TekushiyPrefab;
    }

    // ���������, ��� �� ��� ����������� ������ ������� ������ � ����� �������.
    // �������������� ������ �� ��������� ���������� ������ � ����� �����.
    private bool EstCvetRyadom()
    {
        if (minimalnoeRasstoyanieMezhduCvetami <= 0f) return false;

        Collider[] kollajdery = Physics.OverlapSphere(tochkaPopadaniya, minimalnoeRasstoyanieMezhduCvetami);
        foreach (var col in kollajdery)
        {
            Nasazhdenie sosed = col.GetComponentInParent<Nasazhdenie>();
            if (sosed != null && sosed.transform.IsChildOf(tekushayaKlumba.kontejnerCvetov))
                return true;
        }
        return false;
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
