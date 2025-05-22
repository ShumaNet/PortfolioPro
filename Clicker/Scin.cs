using UnityEngine;
using UnityEngine.UI;

public class Scin : MonoBehaviour
{
    public int cost; // ñòîèìîñòü ñêèíà
    public int scinID; // id ñêèíàêóïëåí ëè ñêèí?
    public bool isBuy; // êóïëåí ëè ñêèí?
    public bool isSelected; // àêòèâèðîâàí ëè ñêèí?

    public Button buttonBuy; // ññûëêà íà êíîïêó "êóïèòü"
    public Button buttonSelect; // ññûëêà íà êíîïêó "ïðèìåíèòü"
    public ScinShop scinShop; // ññûëêà íà ñêðèïò ScinShop ìàãàçèíà, êîòîðûé íàõîäèòñÿ íà îáúåêòå Canvas

    public void Buy()
    {
        if (scinShop.money >= cost)
        {
            scinShop.money -= cost;
            scinShop.textMoney.text = "Ìîíåò: " + scinShop.money.ToString();
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
