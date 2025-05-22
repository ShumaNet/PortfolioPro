using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeManager : MonoBehaviour
{
    
    public Text TextMoney; // перевожу деньги в текст

    public Text TextPassiveMoneyOnOneSecond; //текст пасива  за одну секунду
    public Text TextDoublePassiveMoneyOnOneSecond; //текст удваивания пассива  за одну секунду

    public Text TextMoneyOnOneClick;// текст  за один тап
    public Text TextDoubleMoneyOnOneClick; // текст удваивания за один тап

    public Text TextPriceA;  //PriceDoublePassiveMoneyOnOneSecond
    public Text TextPriceB; //PriceDoubleMoneyOnOneClick

    public Text TextDPMOOS;
    public Text TextDMOOC;

    public int CostDoublePassivMoneyOnOneSecond = 50;
    public int CostDoubleMoneyOnOneClick = 125;

    private int PriceDoublePassiveMoneyOnOneSecond = 75;
    private int PriceDoubleMoneyOnOneClick = 75;

    private void Start()
    {
        Invoke("DoSomething", 1f);
    }

    void DoSomething()
    {
        GlobalPeremen.Money += GlobalPeremen.PassiveMoneyOnOneSecond;
    }

    public void GoHome()
    {
        SceneManager.LoadScene("Market");
    }

    public void DoubleMoneyOnClick()
    {
        if(GlobalPeremen.Money > PriceDoubleMoneyOnOneClick)
        {
            GlobalPeremen.TapOnOneClick += GlobalPeremen.DoubleMoneyOnOneClick;
            PriceDoubleMoneyOnOneClick += CostDoubleMoneyOnOneClick;
        }
        
    }

    public void DoublPassiveMoneyOnOneSecond()
    {
        if(GlobalPeremen.Money > PriceDoublePassiveMoneyOnOneSecond)
        {
            GlobalPeremen.PassiveMoneyOnOneSecond += GlobalPeremen.DoblePassiveMoneyOnOneSecond;
            PriceDoublePassiveMoneyOnOneSecond += CostDoublePassivMoneyOnOneSecond;
        }
       
    }

    public void ButtonOnClick()
    {
        GlobalPeremen.Money += GlobalPeremen.TapOnOneClick;
    }

    private void Update()
    {
        TextMoney.text = ("Money:" + GlobalPeremen.Money);
        TextPassiveMoneyOnOneSecond.text = ("PassiveMoney" + "+" + GlobalPeremen.PassiveMoneyOnOneSecond);

        TextDoublePassiveMoneyOnOneSecond.text = ("DoublePassive:"+ "+" + GlobalPeremen.DoblePassiveMoneyOnOneSecond);
        TextPriceA.text = ("Cost" + PriceDoublePassiveMoneyOnOneSecond);

        TextPriceB.text = ("Cost:" + PriceDoubleMoneyOnOneClick);
        TextDoubleMoneyOnOneClick.text = ("DoubleOneTap:" + "+" + GlobalPeremen.DoubleMoneyOnOneClick);

        TextDPMOOS.text = "UpPassive:" + "+" + GlobalPeremen.DPMOOS;
        TextDMOOC.text = "UpClick:" + "+" + GlobalPeremen.DMOOC;
    }
}
