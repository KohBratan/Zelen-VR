using UnityEngine;

// Задача 4.3. Чистая логика замены вида насаждения - без UI и без ввода.
// Создаёт новый префаб на месте старого объекта, сохраняя позицию, поворот и scale,
// затем удаляет старый объект. Статический класс: ничего не нужно вешать на сцену.
public static class ZamenaNasazhdeniya
{
    // Возвращает паспорт нового насаждения или null, если заменить не удалось.
    public static Nasazhdenie Zamenit(Nasazhdenie staroe, GameObject prefab, string novyiVid)
    {
        if (staroe == null)
        {
            Debug.LogWarning("ZamenaNasazhdeniya: нечего заменять (старый объект не задан).");
            return null;
        }

        if (prefab == null)
        {
            Debug.LogWarning("ZamenaNasazhdeniya: для вида '" + novyiVid + "' не назначен префаб.");
            return null;
        }

        Transform staryiTransform = staroe.transform;

        // Позиция и поворот - мировые, родитель тот же, что был у старого объекта
        GameObject novyiObyekt = Object.Instantiate(
            prefab,
            staryiTransform.position,
            staryiTransform.rotation,
            staryiTransform.parent);

        // Scale копируем со старого объекта (его мог менять слайдер размера или "Растянуть"),
        // а не берём из префаба
        novyiObyekt.transform.localScale = staryiTransform.localScale;

        // "Паспорт" - если в префабе его нет, добавляем, чтобы инвентарь мог работать с объектом
        Nasazhdenie pasport = novyiObyekt.GetComponent<Nasazhdenie>();
        if (pasport == null)
        {
            pasport = novyiObyekt.AddComponent<Nasazhdenie>();
        }

        pasport.vidNasazhdeniya = novyiVid;
        pasport.kategoriya = staroe.kategoriya;
        pasport.zafiksirovano = staroe.zafiksirovano;

        // Прячем сразу, а удаляется объект в конце кадра - так старый не успеет поймать луч или коллизию
        staroe.gameObject.SetActive(false);
        Object.Destroy(staroe.gameObject);

        return pasport;
    }
}
