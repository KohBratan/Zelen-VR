using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PrefabSpawner : MonoBehaviour
{
    public enum Tool
    {
        SinglePlace,
        BrushPlace,
        Move,
        Destroy
    }

    [Header("Prefabs")]
    public List<GameObject> prefabs = new List<GameObject>();
    private int selectedIndex = 0;

    [Header("Input Action Asset (XRI Default)")]
    public InputActionAsset inputActionAsset;

    [Header("Spawning")]
    public Transform rayOrigin;
    public float raycastMaxDistance = 10f;
    public float defaultSpawnDistance = 2f;

    [Header("Placement (while action held)")]
    public InputActionReference placementActionRef;
    public GameObject activeWhileHeldObject;
    public GameObject placementIndicator;

    private InputAction placementAction;

    [Header("Tool System")]
    public Tool currentTool = Tool.SinglePlace;
    public TMP_Text toolText;
    public float brushSpawnInterval = 0.1f;
    public bool randomYRotation = true;

    [Header("Debug")]
    public bool debugMode = false;

    [Header("Activate Action (Trigger)")]
    public InputActionReference activateActionRef;

    private InputAction activateAction;
    private bool triggerHeld = false;
    private float lastBrushSpawnTime;
    private GameObject heldMoveObject;

    [Header("UI Indicators")]
    public GameObject[] indicatorObjects;
    public GameObject[] indicatorHighlights;

    [Header("Audio")]
    public AudioClip ambientClip;
    public AudioClip placementClip;

    private AudioSource ambientSource;

    // Слой, который нельзя двигать и удалять
    private int groundLayer;

    void Start()
    {
        // Кешируем слой Ground
        groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer == -1)
            Debug.LogWarning("Слой 'Ground' не найден в проекте. Проверка отключена.");

        // Setup ambient audio
        if (ambientClip != null)
        {
            ambientSource = GetComponent<AudioSource>();
            if (ambientSource == null)
                ambientSource = gameObject.AddComponent<AudioSource>();

            ambientSource.clip = ambientClip;
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
            ambientSource.spatialBlend = 1f; // 3D sound
            ambientSource.Play();
        }
    }

    void OnEnable()
    {
        // Subscribe to ALL actions in the asset and filter by map/control name
        if (inputActionAsset != null)
        {
            foreach (var map in inputActionAsset.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    action.performed += OnActionPerformed;
                }
            }
            inputActionAsset.Enable();
        }

        // Placement action remains unchanged
        if (placementActionRef != null)
        {
            placementAction = placementActionRef.action;
            placementAction.Enable();
        }

        // Activate action (trigger) for tool usage
        if (activateActionRef != null)
        {
            activateAction = activateActionRef.action;
            activateAction.Enable();
            activateAction.performed += OnActivatePerformed;
            activateAction.canceled += OnActivateCanceled;
        }

        UpdateUI();
        UpdateToolText();
    }

    void OnDisable()
    {
        if (inputActionAsset != null)
        {
            foreach (var map in inputActionAsset.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    action.performed -= OnActionPerformed;
                }
            }
            inputActionAsset.Disable();
        }

        if (placementAction != null)
        {
            placementAction.Disable();
            placementAction = null;
        }

        if (activateAction != null)
        {
            activateAction.performed -= OnActivatePerformed;
            activateAction.canceled -= OnActivateCanceled;
            activateAction.Disable();
            activateAction = null;
        }
    }

    /// <summary>
    /// Called whenever ANY action in the asset is performed.
    /// Only reacts to cycle and tool switch inputs.
    /// </summary>
    private void OnActionPerformed(InputAction.CallbackContext ctx)
    {
        string mapName = ctx.action.actionMap?.name;
        string controlName = ctx.control?.displayName;
        string actionName = ctx.action.name;

        Debug.Log($"Action performed: {actionName} (map: {mapName}) | control: {controlName}");

        // Cycle tools: right grip press (Grab Move from XRI Right Locomotion)
        if (mapName == "XRI Right Locomotion" && controlName == "grippressed" && actionName == "Grab Move" && ctx.ReadValueAsButton())
        {
            CycleTool();
        }
        // Cycle prefab: right secondary button press (only on press, not release)
        else if (mapName == "XRI Right" && controlName == "secondarybutton" && actionName == "Haptic Device" && ctx.ReadValueAsButton())
        {
            CycleNext();
        }
    }

    void Update()
    {
        // Placement indicator logic – works with the separate placementAction
        if (placementAction != null)
        {
            bool isActive = placementAction.IsPressed();
            if (activeWhileHeldObject != null)
                activeWhileHeldObject.SetActive(isActive);

            if (placementIndicator != null)
            {
                placementIndicator.SetActive(isActive);
                if (isActive)
                {
                    if (rayOrigin != null)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, raycastMaxDistance))
                        {
                            placementIndicator.transform.position = hit.point;
                            placementIndicator.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                        }
                        else
                        {
                            placementIndicator.transform.position = rayOrigin.position + rayOrigin.forward * raycastMaxDistance;
                            placementIndicator.transform.rotation = Quaternion.identity;
                        }
                    }
                }
            }
        }

        // Brush spawning (continuous while trigger held)
        if (triggerHeld && currentTool == Tool.BrushPlace)
        {
            if (Time.time - lastBrushSpawnTime >= brushSpawnInterval)
            {
                TrySpawnSelected();
                lastBrushSpawnTime = Time.time;
            }
        }
    }

    public void CycleNext()
    {
        if (prefabs.Count == 0) return;
        selectedIndex = (selectedIndex + 1) % prefabs.Count;
        Debug.Log($"Selected prefab index: {selectedIndex} ({prefabs[selectedIndex].name})");
        UpdateUI();
    }

    public void CyclePrevious()
    {
        if (prefabs.Count == 0) return;
        selectedIndex = (selectedIndex - 1 + prefabs.Count) % prefabs.Count;
        Debug.Log($"Selected prefab index: {selectedIndex} ({prefabs[selectedIndex].name})");
        UpdateUI();
    }

    public void CycleTool()
    {
        int count = System.Enum.GetValues(typeof(Tool)).Length;
        currentTool = (Tool)(((int)currentTool + 1) % count);
        Debug.Log($"Tool switched to: {currentTool}");
        UpdateToolText();
    }

    /// <summary>
    /// Spawns the selected prefab ONLY if the raycast hits something.
    /// Returns true if a tree was spawned, false otherwise.
    /// </summary>
    public bool TrySpawnSelected()
    {
        if (prefabs.Count == 0) return false;

        if (rayOrigin == null)
        {
            Debug.LogWarning("RayOrigin not set. Cannot spawn.");
            return false;
        }

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, raycastMaxDistance))
        {
            Vector3 spawnPos = hit.point;
            var spawned = Instantiate(prefabs[selectedIndex], spawnPos, Quaternion.identity);

            // Apply rotation: flat upright, with optional random yaw
            if (randomYRotation)
                spawned.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            else
                spawned.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            Debug.Log($"Spawned: {spawned.name} at {spawnPos}");

            // Play placement sound
            if (placementClip != null)
                AudioSource.PlayClipAtPoint(placementClip, spawnPos);

            return true;
        }
        else
        {
            Debug.Log("Raycast did not hit anything – tree not spawned.");
            return false;
        }
    }

    // Kept for backward compatibility
    public void SpawnSelected()
    {
        TrySpawnSelected();
    }

    void UpdateUI()
    {
        for (int i = 0; i < indicatorObjects.Length; i++)
        {
            if (indicatorObjects[i] != null)
                indicatorObjects[i].SetActive(true);
        }
        for (int i = 0; i < indicatorHighlights.Length; i++)
        {
            if (indicatorHighlights[i] != null)
                indicatorHighlights[i].SetActive(i == selectedIndex);
        }
    }

    void UpdateToolText()
    {
        if (toolText != null)
        {
            toolText.text = currentTool.ToString();
        }
    }

    private void OnActivatePerformed(InputAction.CallbackContext ctx)
    {
        bool buttonValue = ctx.ReadValueAsButton();
        if (debugMode) Debug.Log($"[DEBUG] Activate performed: value={ctx.ReadValue<float>()}, asButton={buttonValue}, phase={ctx.phase}, tool={currentTool}");

        if (!buttonValue) return;

        triggerHeld = true;
        if (debugMode) Debug.Log($"[DEBUG] triggerHeld set to true, tool={currentTool}");

        switch (currentTool)
        {
            case Tool.SinglePlace:
                if (debugMode) Debug.Log($"[DEBUG] Attempting SinglePlace spawn...");
                TrySpawnSelected();
                break;

            case Tool.BrushPlace:
                if (debugMode) Debug.Log($"[DEBUG] Starting BrushPlace...");
                TrySpawnSelected();
                lastBrushSpawnTime = Time.time;
                break;

            case Tool.Move:
                if (debugMode) Debug.Log($"[DEBUG] Move tool: raycasting...");
                if (rayOrigin != null)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, raycastMaxDistance))
                    {
                        if (debugMode) Debug.Log($"[DEBUG] Move tool hit: {hit.collider.name} (root: {hit.collider.transform.root.name})");
                        GameObject obj = hit.collider.gameObject;

                        // Проверка: нельзя двигать объекты на слое Ground
                        if (groundLayer != -1 && obj.layer == groundLayer)
                        {
                            Debug.Log("Move blocked: target is on Ground layer.");
                            break;
                        }

                        if (obj != null && obj != gameObject && obj.transform.root != transform.root)
                        {
                            heldMoveObject = obj;
                            heldMoveObject.transform.SetParent(rayOrigin, worldPositionStays: true);
                            Debug.Log($"Picked up: {heldMoveObject.name}");
                        }
                        else if (debugMode)
                        {
                            Debug.Log($"[DEBUG] Move tool: invalid target (self or child of spawner)");
                        }
                    }
                    else if (debugMode)
                    {
                        Debug.Log($"[DEBUG] Move tool: raycast hit nothing");
                    }
                }
                else if (debugMode)
                {
                    Debug.Log($"[DEBUG] Move tool: rayOrigin is null!");
                }
                break;

            case Tool.Destroy:
                if (debugMode) Debug.Log($"[DEBUG] Destroy tool: raycasting...");
                if (rayOrigin != null)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, raycastMaxDistance))
                    {
                        if (debugMode) Debug.Log($"[DEBUG] Destroy tool hit: {hit.collider.name}");
                        GameObject obj = hit.collider.gameObject;

                        // Проверка: нельзя удалять объекты на слое Ground
                        if (groundLayer != -1 && obj.layer == groundLayer)
                        {
                            Debug.Log("Destroy blocked: target is on Ground layer.");
                            break;
                        }

                        if (obj != null && obj != gameObject)
                        {
                            Debug.Log($"Destroyed: {obj.name}");
                            Destroy(obj);
                        }
                        else if (debugMode)
                        {
                            Debug.Log($"[DEBUG] Destroy tool: invalid target (self)");
                        }
                    }
                    else if (debugMode)
                    {
                        Debug.Log($"[DEBUG] Destroy tool: raycast hit nothing");
                    }
                }
                else if (debugMode)
                {
                    Debug.Log($"[DEBUG] Destroy tool: rayOrigin is null!");
                }
                break;
        }
    }

    private void OnActivateCanceled(InputAction.CallbackContext ctx)
    {
        triggerHeld = false;

        // Release any held object from move tool
        if (heldMoveObject != null)
        {
            heldMoveObject.transform.SetParent(null, worldPositionStays: true);
            heldMoveObject = null;
        }
    }
}