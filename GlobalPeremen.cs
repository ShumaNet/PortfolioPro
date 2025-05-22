using UnityEngine;

public static class GlobalPeremen
{
    public static int Money = 0; //деньги
    public static int TapOnOneClick = 1;  // за один тап
    public static int PassiveMoneyOnOneSecond = 1;  // за одну секунду

    public static int DoblePassiveMoneyOnOneSecond = PassiveMoneyOnOneSecond; // удвоить деньги равна денег за одну секунду
    public static int DoubleMoneyOnOneClick = TapOnOneClick; // удвоить за один клик равно за один клик

    public static int DPMOOS = PassiveMoneyOnOneSecond * 2;
    public static int DMOOC = TapOnOneClick * 2;
}



