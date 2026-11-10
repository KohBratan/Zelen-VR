// ---------------------------------------------------------------------------
// Zelen-VR
// File: InstrukciyaPanel.cs
// Panel instruktsii.
// ---------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class InstrukciyaPanel : MonoBehaviour
{
    [Header("������ �� ������ ������ (Main Camera)")]
    public Transform golova;

    [Header("���������� �� ������")]
    public float distanciya = 1.2f;

    [Header("������ ������������ ���� (����� = ����)")]
    public float vysota = -0.1f;

    [Header("�������� ����������")]
    public float skorostSlezheniya = 5f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private bool zahvachena = false;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // ������������� �� ������� ������� � ����������
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);

        if (golova == null && Camera.main != null)
        {
            golova = Camera.main.transform;
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        zahvachena = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        zahvachena = false;
    }

    // ���������� ��� �������� ������, ����� ��� ����� ���������� ����� �������
    public void PostavitPeredIgrokom()
    {
        if (golova == null) return;

        Vector3 targetPos = golova.position + golova.forward * distanciya + golova.up * vysota;
        transform.position = targetPos;

        // ������������ ������ � ������, �� ����� �� ��������� (��� ��������)
        Vector3 lookDir = transform.position - golova.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);
    }

    void LateUpdate()
    {
        // ���� ������ � ����� - ��� �� ��������� �� �������, �� �� ���� ������
        if (zahvachena || golova == null) return;

        // ������ ������� ������ � ����� ����� �������
        Vector3 targetPos = golova.position + golova.forward * distanciya + golova.up * vysota;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * skorostSlezheniya);

        // ������ ������������ � ������
        Vector3 lookDir = transform.position - golova.position;
        lookDir.y = 0;
        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * skorostSlezheniya);
    }
}