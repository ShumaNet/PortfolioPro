using UnityEngine;

public static class GlobalPeremen
{
    public static int Money = 0; //äåíüãè
    public static int TapOnOneClick = 1;  // çà îäèí òàï
    public static int PassiveMoneyOnOneSecond = 1;  // çà îäíó ñåêóíäó

    public static int DoblePassiveMoneyOnOneSecond = PassiveMoneyOnOneSecond; // óäâîèòü äåíüãè ðàâíà äåíåã çà îäíó ñåêóíäó
    public static int DoubleMoneyOnOneClick = TapOnOneClick; // óäâîèòü çà îäèí êëèê ðàâíî çà îäèí êëèê

    public static int DPMOOS = PassiveMoneyOnOneSecond * 2;
    public static int DMOOC = TapOnOneClick * 2;
}



