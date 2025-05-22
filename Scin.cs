using UnityEngine;
using UnityEngine.UI;

public class Scin : MonoBehaviour
{
    public int cost; // стоимость скина
    public int scinID; // id скинакуплен ли скин?
    public bool isBuy; // куплен ли скин?
    public bool isSelected; // активирован ли скин?

    public Button buttonBuy; // ссылка на кнопку "купить"
    public Button buttonSelect; // ссылка на кнопку "применить"
    public ScinShop scinShop; // ссылка на скрипт ScinShop магазина, который находится на объекте Canvas

    public void Buy()
    {
        if (scinShop.money >= cost)
        {
            scinShop.money -= cost;
            scinShop.textMoney.text = "Монет: " + scinShop.money.ToString();
            isBuy = true;
            buttonBuy.gameObject.SetActive(false);
            buttonSelect.gameObject.SetActive(true);
            PlayerPrefs.SetInt("money", scinShop.money);
            PlayerPrefs.SetInt("buy" + scinID, 1);
            PlayerPrefs.Save();
        }
    }

    public void Select()
    {
        scinShop.scins[scinShop.activeScinID].buttonSelect.gameObject.SetActive(true);
        scinShop.scins[scinShop.activeScinID].isSelected = false;
        scinShop.activeScinID = scinID;
        isSelected = true;
        buttonSelect.gameObject.SetActive(false);
        PlayerPrefs.SetInt("scinsID", scinID);
        PlayerPrefs.Save();
    }
}