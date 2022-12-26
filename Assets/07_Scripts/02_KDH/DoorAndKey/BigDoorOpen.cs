using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigDoorOpen : MonoBehaviourPun
{
   enum DoorState          // ���� ���� ���¿� ���� ���� ����
    {
        Closed,
        Opened,
        Locked
    }

    [Tooltip("����� Door�� �ִϸ����͸� �־��ּ���")]
    [SerializeField] private Animator anim;           
    [Tooltip("�ν�����â�� Animator�� Bool���� Parameter �̸��� �Է��ϼ���")]
    [SerializeField] private string animBoolName;       

    private string hashName;                       // �ν�����â�� Animator�� Bool���� Parameter �̸��� �޾ƿ� string ����
    private DoorState state;                       // ���� ���¿� �����ϱ� ���� ����
                                                   // bool�� state���� ������ ���� : Ư�� �̺�Ʈ�� ���� ���� �ݴ� ����� Ȯ�强�� ���� ����
    // Door�� ���¸� ����
    private bool open;                              // ���� ���� �ݱ� ���� �ʿ��� bool ���� 
    private bool canUse;      

    private AudioSource audio;                                      // ����� ��� �÷��̾�
    [SerializeField] private AudioClip lockOpenClip;                // ��� �� ���� ����
    [SerializeField] private AudioClip doorOpen;                    // �� �� �� ����Ŭ��
    [SerializeField] private AudioClip doorClose;                   // �� ���� �� ����Ŭ��

    public PhotonView PV;

    // ���� ���� ���� ������Ʈ ���� ����
    [Tooltip("���谡 �ʿ��� ���̶�� üũ���ּ���")]
    [SerializeField] private bool isNeedKey;        // ���谡 �ʿ��� ���̶�� �Ұ� üũ
    private bool hasKey;                            // ���踦 ���� ������ �Ұ� üũ

    public bool IsNeedKey => isNeedKey;            // �ܺο� �Ұ� ��ȯ

    // �ش� ���� �ش��ϴ� ���� Class�� �޾ƿ� ���� �����ϱ�


    private void Awake()                      // �ִϸ��̼� ������ ���� �ϱ� ���ؼ� ����
    {
        hashName = animBoolName;              // �ݵ�� ���� ���� ������Ʈ���� �ִϸ��̼� �Ұ� �̸� �־���� ��, test�������� "isOpen" ���
        open = anim.GetBool(hashName);        // hashName���� �ҷ���
        audio = GetComponent<AudioSource>();  

        if (isNeedKey) state = DoorState.Locked; // ������ �� isNeedKey�� True�̸� ��� ���·� ���� ( ���� ����Ʈ ������ ���� ���� ��� ����)
        else state = DoorState.Closed;           // isNeedKey�� �ƴϸ� ����Ʈ ������ ���� �ְ�
    }


    [PunRPC]
    public void TryHandle()                       // �ܺο��� �� �޼��带 ���ؼ� ������ �� �ִϸ��̼��� ����
    {
        if (state == DoorState.Closed)            // open (true,false) -> State���� -> �ִϸ��̼� ��� ����
        {           
            open = true;
            HandleDoor();
            UseDoor();
        }
        else if (state == DoorState.Opened)
        {          
            open = false;
            HandleDoor();
            UseDoor();
        }      
    }

    // ���� ���� �������� ����� �޼���
    #region KeyDoor

    [PunRPC]
    public void CheckHasKey()                      // ���� ���� ���ǿ� ���� �̺�Ʈ ����
    {
        state = DoorState.Closed;
        isNeedKey = false;
        //PlaySE(lockOpenClip);
    }

    public void Open()                     // ���� ���� �̺�Ʈ  , ���� ���� ������ �����ϴ� �Ұ� �߰��� ��
    {
        if(state != DoorState.Opened)
        {
            open = true;
            HandleDoor();
        }
    }

    public void Closed()                   // ���� ���� �̺�Ʈ  
    {
        if (state != DoorState.Closed)
        {
            open = false;
            HandleDoor();
        }
    }
    #endregion

    private void HandleDoor()              // ���� ���� ���� ���� 
    {      
        state = open ? DoorState.Opened : DoorState.Closed;
        canUse = true;
    }

    public void UseDoor()                  // ���� ���� ���¿� ���� �� �ִϸ��̼� ���
    {
        switch (state)                          // ���谡 �ʿ� ���ٸ� �׳� �� �� �ְ�
        {
            case DoorState.Opened:
                anim.SetBool(hashName, true);  // �ִϸ��̼� ��� -> �Ҹ� ��� ���
                canUse = false;
                //PlaySE(doorOpen);
                // ������ �ż���
                break;
            case DoorState.Closed:
                anim.SetBool(hashName, false);
                canUse = false;
                //PlaySE(doorClose);
                // ������ �ż���
                break;
        }
    }

    private void PlaySE(AudioClip _clip)   // �Ҹ� ����ϱ� ���� �޼��� ����
    {
        audio.clip = _clip;
        audio.Play();
    }
}
