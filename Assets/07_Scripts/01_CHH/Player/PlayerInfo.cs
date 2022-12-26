using System;
using System.Collections.Generic;

/// <summary>
/// JSON �������� ����ȭ�Ͽ� �÷��̾� ���� ����Ʈ ������ ����
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class Serialization<T>
{
    public List<T> playerInfos;

    public Serialization(List<T> _playerInfos)
    {
        this.playerInfos = _playerInfos;
    }
}

/// <summary>
/// �÷��̾� ������ �����ϴ� Ŭ����
/// </summary>
[Serializable]
public class PlayerInfo
{
    #region Variable

    public string nickName;         // �÷��̾� �г���
    public int actorNum;            // ���濡�� �÷��̾ �����ϱ� ���� ���� �ĺ���
    public int randCharacterNum;    // �÷��̾� ĳ���� ���� �ѹ�
    public int hp;                  // �÷��̾� ü��
    public int zombieKillCount;     // ���� ų ī��Ʈ
    public int playerKillCount;     // �÷��̾ ���� ų ī��Ʈ
    public int score;               // ��ũ�� �ö� ����
    public int voteMafiaCount;      // ���ǾƷ� ��ǥ (����) ���� ��
    public double clearTime;        // ���� Ŭ���� Ÿ��
    public bool isMafia;            // ���Ǿ� ����
    public bool isVoteMafiaSelect;  // ��ǥ�� ���� ���ǾƷ� ���� �Ǿ����� ����
    public bool isDie;              // ���� ����
    public bool isEcapeSuccess;     // Ż�� ���� ����

    /// <summary>
    /// �÷��̾� ���� �ʱ�ȭ
    /// </summary>
    public PlayerInfo(string _nickName, int _actorNum, int _randCharacterNum, bool _isMafia)
    {
        nickName = _nickName;
        actorNum = _actorNum;
        randCharacterNum = _randCharacterNum;
        isMafia= _isMafia;
        hp = 100;
        zombieKillCount = 0;
        playerKillCount = 0;
        score = 0;
        voteMafiaCount = 0;
        clearTime = 0f;
        isVoteMafiaSelect = false;
        isDie = false;
        isEcapeSuccess = false;
    }

    #endregion Variable
}
