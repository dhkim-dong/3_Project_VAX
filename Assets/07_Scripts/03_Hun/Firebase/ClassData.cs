using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;


[Obsolete("PlayerInfo �����ϱ�!")]
[Serializable] // ��ü���� ��������ִ� ����ȭ ��Ʈ����Ʈ
public class User // ���� ��ü
{
    // ���Ŀ� �ʿ��ϸ� �Ӽ��� �ν��Ͻ� ���� �߰�
    public string name;
    public int win; // ��
    public int lose; // ��
    public int score; // ����

    public User(string name = "", int win = 0, int lose = 0, int score = 100) // ���������� �����ڵ� �߰�
    {
        this.name = name;
        this.win = win;
        this.lose = lose;
        this.score = score;
    }

    public double GetWinRate()
    {
        return Math.Round(((double)win / (double)(win + lose))*100f, 1);
    }
}
