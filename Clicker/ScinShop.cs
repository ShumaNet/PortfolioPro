using UnityEngine;
using TMPro;

public class ScinShop : MonoBehaviour
{
    public int money = 500; // êîëè÷åñòâî âàøèõ äåíåã ïðè ñòàðòå èãðû
    public TextMeshProUGUI textMoney; // ññûëêà íà òåêñò íà èãðîâîé ñöåíå, â êîòîðîì îòîáðàæåíî êîëè÷åñòâî äåíåã.
    public Scin[] scins; // ññûëêè íà âñå âàøè ñêèíû (îáúåêòû Scin1, Scin2, Scin3)
    public int activeScinID = 0; // íîìåð ñêèíà, êîòîðûé êóïëåí è àêòèâèðîâàí èçíà÷àëüíî

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

        textMoney.text = "Ìîíåò: " + money.ToString();

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
