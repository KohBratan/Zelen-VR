using UnityEngine;

/// <summary>
/// Отладочный скрипт: рисует луч от указанного трансформа и логирует попадание
/// по слою террейна. Используется при настройке посадки деревьев.
/// </summary>
public class DebugRaycast : MonoBehaviour
{
    public Transform rayOrigin;
    public float maxDistance = 10f;
    public LayerMask terrainLayer;

    void Update()
    {
        if (rayOrigin == null) return;

        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, maxDistance, terrainLayer))
        {
            Debug.DrawLine(rayOrigin.position, hit.point, Color.green);
            Debug.Log($"DebugRaycast: попадание в {hit.collider.name} на расстоянии {hit.distance:F2} м");
        }
        else
        {
            Debug.DrawRay(rayOrigin.position, rayOrigin.forward * maxDistance, Color.red);
        }
    }
}
