using UnityEngine;
using UnityEngine.UI;

public class Scin : MonoBehaviour
{
    public int cost; // ��������� �����
    public int scinID; // id ����������� �� ����?
    public bool isBuy; // ������ �� ����?
    public bool isSelected; // ����������� �� ����?

    public Button buttonBuy; // ������ �� ������ "������"
    public Button buttonSelect; // ������ �� ������ "���������"
    public ScinShop scinShop; // ������ �� ������ ScinShop ��������, ������� ��������� �� ������� Canvas

    public void Buy()
    {
        if (scinShop.money >= cost)
        {
            scinShop.money -= cost;
            scinShop.textMoney.text = "�����: " + scinShop.money.ToString();
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