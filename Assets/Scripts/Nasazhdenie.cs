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
}