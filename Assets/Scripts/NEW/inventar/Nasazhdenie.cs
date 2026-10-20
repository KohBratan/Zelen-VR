using UnityEngine;

// "Паспорт" насаждения — вешается на каждый посаженный объект
// (дерево, куст, секцию газона, клумбу), хранит его данные.
public class Nasazhdenie : MonoBehaviour
{
    [Header("Название вида, например 'Дуб' или 'Газон обычный'")]
    public string vidNasazhdeniya;

    [Header("Категория: Клумбы / Газон / Деревья / Кусты")]
    public string kategoriya;

    [Header("Зафиксирован ли объект")]
    public bool zafiksirovano = false;

    // Размер объекта в момент появления на сцене. Слайдер размера в инвентаре
    // меняет размер ОТНОСИТЕЛЬНО него (1 = как при посадке)
    [HideInInspector]
    public Vector3 ishodnyiRazmer = Vector3.one;

    // Префаб, из которого был создан этот объект. Нужен для панели
    // "Заменить вид" - чтобы понять, какой вид сейчас у дерева.
    [HideInInspector]
    public GameObject sourcePrefab;

    void Awake()
    {
        ishodnyiRazmer = transform.localScale;
    }
}