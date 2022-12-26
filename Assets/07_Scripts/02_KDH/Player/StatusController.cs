using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusController : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView PV;

    [SerializeField]   private int maxHp;                    // 최대 체력을 설정해 주세요
    [SerializeField]   private int curHp;                    // 에디터에서 확인하기 위해 선언

    [SerializeField]   private int maxSp;                    // 최대 스테미너를 설정해 주세요
    [SerializeField]   private int curSp;                    // 스테미너는 Update 60프레임 속도로 빠르게 떨어지므로 값이 1000보다 크게 하는 것을 추천

    [SerializeField]  private int spIncreaseSpeed;           // 스테미나 회복 속도

    [SerializeField]  private int spRechargeTime;            // 스테미나 회복하는 재생성 시간
    private int curretSpRechargeTime;                        // 시간 체크용 변수

    private bool spUsed; // Sp회복 체크용 변수

    [SerializeField] private Image[] images_Gauge;           // 캐릭터 스테이터스 UI 

    private const int HP = 0, SP = 1;                        // 가독성을 위해 상수로 HP, SP 선언

    // 나의 변수 사용
    public int CurSp => curSp;
    public int CurHp => curHp;

    private void Awake()
    {
        curHp = maxHp;
        curSp = maxSp;
    }


    private void Update()
    {
        if (!PV.IsMine) return;

        images_Gauge[HP].fillAmount = (float)curHp / maxHp;   // 체력 UI 표시
        images_Gauge[SP].fillAmount = (float)curSp / maxSp;
        SPRecharge();
        SPRecover();
        DefaultSetting();
    }

    private void DefaultSetting()        // 최대 체력, 최대 스테미나 고정해주기 위한 메서드
    {
        if (curSp >= maxSp)
            curSp = maxSp;
        if (curHp >= maxHp)
            curHp = maxHp;
    }

    private void SPRecharge()             // 스테미너가 회복이 필요한지 아닌지 체크해주는 메서드
    {
        if (spUsed)                       // 외부 메서드로 인해서 spUsed가 true가 됬을 때 
        {
            if (curretSpRechargeTime < spRechargeTime) // current값을 증가시켜 spRechargeTime에 도달하면 SPRecover가 Update에서 실행되게함
                curretSpRechargeTime++;
            else
                spUsed = false;
        }
    }

    private void SPRecover()                  // spUsed상태가 아니면 최대 스테미나가 될때까지 회복함
    {
        if (!spUsed && curSp < maxSp)
        {
            curSp += spIncreaseSpeed;
        }
    }

    // 외부에서 스테이터스에 관여하기 위해 사용되는 메서드들 HP ~ SP

    public void DecreaseSP(int _count)   
    {
        spUsed = true;
        curretSpRechargeTime = 0;
        if (curSp - _count > 0)
            curSp -= _count;
        else
            curSp = 0;
    }

    public void IncreaseSP(int _count)   
    {
        Debug.Log(_count + " 만큼 SP 회복");

        if (curSp + _count > maxSp)
            curSp = maxHp;
        else
            curSp += _count;
    }

    public void DecreaseHP(int _count)
    {
        spUsed = true;
        curretSpRechargeTime = 0;
        if (curHp - _count > 0)
            curHp -= _count;
        else
            curHp = 0;

        if(curHp <= 0)
        {
            PlayerDie();
        }
    }

    public void IncreaseHP(int _count)
    {
        Debug.Log(_count + " 만큼 HP 회복");

        if (curHp + _count > maxHp)
            curHp = maxHp;
        else
            curHp += _count;
    }

    private void PlayerDie()                     // 죽음 상태 구현 필요함
    {
        Debug.Log("플레이어가 사망하였습니다.");
    }
}
