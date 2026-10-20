using UnityEngine;
using UnityEngine.InputSystem;

// Пока в инвентаре включён режим поворота (нажали "Повернуть"):
// джойстик, зажатый вправо, крутит дерево против часовой стрелки (вид сверху),
// зажатый влево - по часовой.
// Как только игрок наклонит джойстик вверх/вниз (листает другие кнопки),
// режим поворота выключается сам.
//
// Скрипт только читает значение джойстика, навигацию по кнопкам он не трогает.
// Вешается на любой постоянно включённый объект (например, InventoryManager).
public class VrashenieDereva : MonoBehaviour
{
    [Header("Панель действий инвентаря (InventarDeystviy)")]
    public InventarDeystviy inventar;

    [Header("Весь asset с действиями")]
    public InputActionAsset naborDeistvii;

    [Header("Карта и действие джойстика правой руки (Vector2, то же, что у навигации инвентаря)")]
    public string imyaKarty = "XRI Right Interaction";
    public string imyaDeistviya = "MenuJoystick";

    [Header("Скорость поворота, градусов в секунду при полном наклоне (минус - поменять направление)")]
    public float skorostVrascheniya = 90f;

    [Header("Мёртвая зона по горизонтали (слабее - не крутим)")]
    public float mertvayaZona = 0.2f;

    [Header("Наклон вверх/вниз, после которого режим поворота выключается")]
    public float porogVyhoda = 0.6f;

    private InputAction deistvieDzhoystika;

    void Awake()
    {
        if (naborDeistvii == null)
        {
            Debug.LogWarning("VrashenieDereva: naborDeistvii не назначен!");
            return;
        }

        var karta = naborDeistvii.FindActionMap(imyaKarty);
        if (karta != null)
        {
            deistvieDzhoystika = karta.FindAction(imyaDeistviya);
        }

        if (deistvieDzhoystika == null)
        {
            Debug.LogWarning("VrashenieDereva: действие '" + imyaDeistviya + "' не найдено в карте '" + imyaKarty + "'.");
        }
    }

    void OnEnable()
    {
        if (deistvieDzhoystika != null)
        {
            deistvieDzhoystika.Enable();
        }
    }

    void Update()
    {
        if (inventar == null || deistvieDzhoystika == null || !inventar.RezhimVrascheniya)
        {
            return;
        }

        Vector2 znachenie = deistvieDzhoystika.ReadValue<Vector2>();

        // Наклон вверх/вниз - игрок листает кнопки, крутить больше не нужно
        bool naklonVerticalniy = Mathf.Abs(znachenie.y) > porogVyhoda && Mathf.Abs(znachenie.y) > Mathf.Abs(znachenie.x);
        if (naklonVerticalniy)
        {
            inventar.ZakonchitVrashenie();
            return;
        }

        if (Mathf.Abs(znachenie.x) > mertvayaZona)
        {
            // Вправо (x > 0) - против часовой, поэтому знак минус
            float gradusy = -znachenie.x * skorostVrascheniya * Time.deltaTime;
            inventar.Vrashat(gradusy);
        }
    }
}