using UnityEngine;

public static class GlobalPeremen
{
    public static int Money = 0; //������
    public static int TapOnOneClick = 1;  // �� ���� ���
    public static int PassiveMoneyOnOneSecond = 1;  // �� ���� �������

    public static int DoblePassiveMoneyOnOneSecond = PassiveMoneyOnOneSecond; // ������� ������ ����� ����� �� ���� �������
    public static int DoubleMoneyOnOneClick = TapOnOneClick; // ������� �� ���� ���� ����� �� ���� ����

    public static int DPMOOS = PassiveMoneyOnOneSecond * 2;
    public static int DMOOC = TapOnOneClick * 2;
}



