using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigDoorOpen : MonoBehaviourPun
{
   enum DoorState          // 현재 문의 상태에 따라 로직 구현
    {
        Closed,
        Opened,
        Locked
    }

    [Tooltip("사용할 Door의 애니메이터를 넣어주세요")]
    [SerializeField] private Animator anim;           
    [Tooltip("인스팩터창에 Animator의 Bool형식 Parameter 이름을 입력하세요")]
    [SerializeField] private string animBoolName;       

    private string hashName;                       // 인스팩터창에 Animator의 Bool형식 Parameter 이름을 받아올 string 변수
    private DoorState state;                       // 문의 상태에 접근하기 위해 선언
                                                   // bool과 state각자 구현한 이유 : 특정 이벤트로 문을 열고 닫는 기능의 확장성을 위해 구현
    // Door의 상태를 관리
    private bool open;                              // 문을 열고 닫기 위해 필요한 bool 변수 
    private bool canUse;      

    private AudioSource audio;                                      // 오디오 재생 플레이어
    [SerializeField] private AudioClip lockOpenClip;                // 잠김 문 오픈 사운드
    [SerializeField] private AudioClip doorOpen;                    // 문 열 때 사운드클립
    [SerializeField] private AudioClip doorClose;                   // 문 닫을 때 사운드클립

    public PhotonView PV;

    // 열쇠 관련 로직 컴포넌트 구현 예정
    [Tooltip("열쇠가 필요한 문이라면 체크해주세요")]
    [SerializeField] private bool isNeedKey;        // 열쇠가 필요한 문이라면 불값 체크
    private bool hasKey;                            // 열쇠를 보유 중인지 불값 체크

    public bool IsNeedKey => isNeedKey;            // 외부에 불값 반환

    // 해당 문에 해당하는 열쇠 Class를 받아올 변수 선언하기


    private void Awake()                      // 애니메이션 관리를 쉽게 하기 위해서 구현
    {
        hashName = animBoolName;              // 반드시 게임 실행 컴포넌트에서 애니메이션 불값 이름 넣어줘야 함, test씬에서는 "isOpen" 사용
        open = anim.GetBool(hashName);        // hashName값을 불러옴
        audio = GetComponent<AudioSource>();  

        if (isNeedKey) state = DoorState.Locked; // 시작할 때 isNeedKey가 True이면 잠긴 상태로 시작 ( 최종 퀘스트 아이템 보관 문에 사용 예정)
        else state = DoorState.Closed;           // isNeedKey가 아니면 디폴트 값으로 닫혀 있게
    }


    [PunRPC]
    public void TryHandle()                       // 외부에서 이 메서드를 통해서 실제로 문 애니메이션을 실행
    {
        if (state == DoorState.Closed)            // open (true,false) -> State변경 -> 애니메이션 출력 형식
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

    // 열쇠 관련 로직으로 사용할 메서드
    #region KeyDoor

    [PunRPC]
    public void CheckHasKey()                      // 열쇠 습득 조건에 대한 이벤트 구현
    {
        state = DoorState.Closed;
        isNeedKey = false;
        //PlaySE(lockOpenClip);
    }

    public void Open()                     // 문이 열림 이벤트  , 문을 여는 조건을 관리하는 불값 추가할 것
    {
        if(state != DoorState.Opened)
        {
            open = true;
            HandleDoor();
        }
    }

    public void Closed()                   // 문이 닫힘 이벤트  
    {
        if (state != DoorState.Closed)
        {
            open = false;
            HandleDoor();
        }
    }
    #endregion

    private void HandleDoor()              // 문의 현재 상태 변경 
    {      
        state = open ? DoorState.Opened : DoorState.Closed;
        canUse = true;
    }

    public void UseDoor()                  // 문의 현재 상태에 따른 문 애니메이션 출력
    {
        switch (state)                          // 열쇠가 필요 없다면 그냥 열 수 있게
        {
            case DoorState.Opened:
                anim.SetBool(hashName, true);  // 애니메이션 출력 -> 소리 출력 방식
                canUse = false;
                //PlaySE(doorOpen);
                // 열리는 매서드
                break;
            case DoorState.Closed:
                anim.SetBool(hashName, false);
                canUse = false;
                //PlaySE(doorClose);
                // 닫히는 매서드
                break;
        }
    }

    private void PlaySE(AudioClip _clip)   // 소리 출력하기 위한 메서드 구현
    {
        audio.clip = _clip;
        audio.Play();
    }
}
