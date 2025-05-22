using UnityEngine;
using TMPro;

public class ScinShop : MonoBehaviour
{
    public int money = 500; // ���������� ����� ����� ��� ������ ����
    public TextMeshProUGUI textMoney; // ������ �� ����� �� ������� �����, � ������� ���������� ���������� �����.
    public Scin[] scins; // ������ �� ��� ���� ����� (������� Scin1, Scin2, Scin3)
    public int activeScinID = 0; // ����� �����, ������� ������ � ����������� ����������

    private void Start()
    {
        //PlayerPrefs.DeleteAll();

        if (PlayerPrefs.HasKey("money"))
        {
            money = PlayerPrefs.GetInt("money");
        }

        if (PlayerPrefs.HasKey("scinsID"))
        {
            activeScinID = PlayerPrefs.GetInt("scinsID");
        }
        scins[activeScinID].isBuy = true;
        scins[activeScinID].isSelected = true;
        PlayerPrefs.SetInt("buy" + activeScinID, 1);
        PlayerPrefs.SetInt("scinsID", activeScinID);

        textMoney.text = "�����: " + money.ToString();

        for (int j = 0; j < scins.Length; j++)
        {

            if (PlayerPrefs.GetInt("buy" + j) == 1)
            {
                scins[j].isBuy = true;
            }

            if (scins[j].isBuy == true)
            {
                scins[j].buttonBuy.gameObject.SetActive(false);
            }

            if (scins[j].isSelected == true || scins[j].isBuy == false)
            {
                scins[j].buttonSelect.gameObject.SetActive(false);
            }
        }
    }
}