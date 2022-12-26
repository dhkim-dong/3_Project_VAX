using System;
using System.Collections.Generic;

/// <summary>
/// JSON 포맷으로 직렬화하여 플레이어 정보 리스트 데이터 전달
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
/// 플레이어 정보를 저장하는 클래스
/// </summary>
[Serializable]
public class PlayerInfo
{
    #region Variable

    public string nickName;         // 플레이어 닉네임
    public int actorNum;            // 포톤에서 플레이어를 구분하기 위한 고유 식별자
    public int randCharacterNum;    // 플레이어 캐릭터 랜덤 넘버
    public int hp;                  // 플레이어 체력
    public int zombieKillCount;     // 좀비 킬 카운트
    public int playerKillCount;     // 플레이어를 죽인 킬 카운트
    public int score;               // 랭크에 올라갈 점수
    public int voteMafiaCount;      // 마피아로 투표 (지목) 당한 수
    public double clearTime;        // 게임 클리어 타임
    public bool isMafia;            // 마피아 여부
    public bool isVoteMafiaSelect;  // 투표를 통해 마피아로 지정 되었는지 여부
    public bool isDie;              // 죽음 여부
    public bool isEcapeSuccess;     // 탈출 성공 여부

    /// <summary>
    /// 플레이어 정보 초기화
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
