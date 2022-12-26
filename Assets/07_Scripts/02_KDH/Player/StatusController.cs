using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusController : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView PV;

    [SerializeField]   private int maxHp;                    // �ִ� ü���� ������ �ּ���
    [SerializeField]   private int curHp;                    // �����Ϳ��� Ȯ���ϱ� ���� ����

    [SerializeField]   private int maxSp;                    // �ִ� ���׹̳ʸ� ������ �ּ���
    [SerializeField]   private int curSp;                    // ���׹̳ʴ� Update 60������ �ӵ��� ������ �������Ƿ� ���� 1000���� ũ�� �ϴ� ���� ��õ

    [SerializeField]  private int spIncreaseSpeed;           // ���׹̳� ȸ�� �ӵ�

    [SerializeField]  private int spRechargeTime;            // ���׹̳� ȸ���ϴ� ����� �ð�
    private int curretSpRechargeTime;                        // �ð� üũ�� ����

    private bool spUsed; // Spȸ�� üũ�� ����

    [SerializeField] private Image[] images_Gauge;           // ĳ���� �������ͽ� UI 

    private const int HP = 0, SP = 1;                        // �������� ���� ����� HP, SP ����

    // ���� ���� ���
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

        images_Gauge[HP].fillAmount = (float)curHp / maxHp;   // ü�� UI ǥ��
        images_Gauge[SP].fillAmount = (float)curSp / maxSp;
        SPRecharge();
        SPRecover();
        DefaultSetting();
    }

    private void DefaultSetting()        // �ִ� ü��, �ִ� ���׹̳� �������ֱ� ���� �޼���
    {
        if (curSp >= maxSp)
            curSp = maxSp;
        if (curHp >= maxHp)
            curHp = maxHp;
    }

    private void SPRecharge()             // ���׹̳ʰ� ȸ���� �ʿ����� �ƴ��� üũ���ִ� �޼���
    {
        if (spUsed)                       // �ܺ� �޼���� ���ؼ� spUsed�� true�� ���� �� 
        {
            if (curretSpRechargeTime < spRechargeTime) // current���� �������� spRechargeTime�� �����ϸ� SPRecover�� Update���� ����ǰ���
                curretSpRechargeTime++;
            else
                spUsed = false;
        }
    }

    private void SPRecover()                  // spUsed���°� �ƴϸ� �ִ� ���׹̳��� �ɶ����� ȸ����
    {
        if (!spUsed && curSp < maxSp)
        {
            curSp += spIncreaseSpeed;
        }
    }

    // �ܺο��� �������ͽ��� �����ϱ� ���� ���Ǵ� �޼���� HP ~ SP

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
        Debug.Log(_count + " ��ŭ SP ȸ��");

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
        Debug.Log(_count + " ��ŭ HP ȸ��");

        if (curHp + _count > maxHp)
            curHp = maxHp;
        else
            curHp += _count;
    }

    private void PlayerDie()                     // ���� ���� ���� �ʿ���
    {
        Debug.Log("�÷��̾ ����Ͽ����ϴ�.");
    }
}
