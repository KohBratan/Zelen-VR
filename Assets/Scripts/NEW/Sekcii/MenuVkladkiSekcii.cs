using UnityEngine;

public class MenuVkladkiSekcii : MenuVkladki
{
    public override void ShagVniz()
    {
        int current = TekushayaVkladka;
        if (current == 9) return; // Слайдер не листаем вниз

        if (current == 0) OtkrytVkladku(1);
        else
        {
            int next = current + 2;
            if (next < 9) OtkrytVkladku(next);
            else OtkrytVkladku(current % 2 == 1 ? 1 : 2); // По кругу
        }
    }

    public override void ShagVverkh()
    {
        int current = TekushayaVkladka;
        if (current == 9) return; // Слайдер не листаем вверх

        if (current == 1 || current == 2) OtkrytVkladku(0);
        else if (current > 2) OtkrytVkladku(current - 2);
    }

    public override bool ShagVpravo()
    {
        int current = TekushayaVkladka;
        if (current == 0 || current == 9) return false;

        if (current % 2 == 1) // Из левой колонки в правую
        {
            OtkrytVkladku(current + 1);
            return true;
        }
        else if (current % 2 == 0) // Из правой колонки на слайдер
        {
            OtkrytVkladku(9);
            return true;
        }
        return false;
    }

    public override bool ShagVlevo()
    {
        int current = TekushayaVkladka;
        if (current == 0) return false;

        if (current == 9) // Со слайдера в правую колонку
        {
            OtkrytVkladku(8);
            return true;
        }

        if (current % 2 == 0) // Из правой колонки в левую
        {
            OtkrytVkladku(current - 1);
            return true;
        }
        return false;
    }
}