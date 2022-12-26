using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviourPunCallbacks                        // 플레이어에 컨트롤러 스크립트를 넣어주세요
{
    [SerializeField] private PhotonView PV;

    [SerializeField] List<ParkourAction> parkourActions;              // 사용 할 파쿠르액션 리스트
    [SerializeField] ParkourAction jumpDownAction;                    // 하강 파쿠르 애니메이션
    [SerializeField] float autoDropHeight = 1f;                       // 입력하지 않아도 자동으로 내려가는 높이

    bool inAction; // 행동 파악 변수

    // 해당 클래스를 사용하기 위해서는 같은 게임오브젝트에 컴포넌트가 있어야 한다.
    EnvironmentScanner environmentScanner;                    
    Animator animator;
    PlayerController playerController;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (!PV.IsMine) return;

        var hitData = environmentScanner.ObstacleCheck(); // 파쿠르 가능한 오브젝트인지 탐색한 Struct데이터를 불러옴

        if (Input.GetButton("Jump") && !inAction && playerController.IsGrounded) // 파쿠르 행동을 위한 제약사항
        {
            if (hitData.forwardHitFound)
            {
                foreach (var action in parkourActions)            // 컨트롤러에 있는 파쿠르 Data중에 
                {
                    if (action.CheckIfPossible(hitData, transform)) // 조건에 해당하는 파쿠르를 찾아 실행하라
                    {
                        StartCoroutine(DoParkourAction(action));    // 먼저 발견한 파쿠르를 먼저 실행하기 때문에 List에 파쿠르 위치도 중요하다.
                        break;                                      // 예를들어 VaultAction과 JumpUp파쿠르는 제한길이가 같은대 Tag설정된 VaultAction이 List 상 위(앞)에 있어야한다.
                    }
                }
            }
        }

        if (playerController.IsOnLedge && !inAction && !hitData.forwardHitFound)       // Ledge상태에서의 액션 이벤트
        {
            bool shouldJump = true;
            if (playerController.LedgeData.height > autoDropHeight && !Input.GetButton("Jump")) // 점프를 안누르고 있으면 낙하하지 않다가
                shouldJump = false;

            if (shouldJump && playerController.LedgeData.angle <= 50)            // 점프키를 누르면 하강 액션을 실행한다.
            {
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAction(jumpDownAction));
            }
        }
    }

    IEnumerator DoParkourAction(ParkourAction action)
    {
        inAction = true;
        playerController.SetControl(false);

        animator.SetBool("mirrorAction", action.Mirror);  // 파쿠르 대상의 좌우 중앙값 기준 치우쳐진 위치를 계산하여 애니메이션의 미러를 실행시킨다.
        animator.CrossFade(action.AnimName, 0.2f); // 애니 메이션 완료를 알리는 방법? => 직접 애니메이션 실행 시간을 확인한다
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(action.AnimName))
            Debug.LogError("파쿠르 애니메이션의 이름이 올바르지 않습니다.");

        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;

            // player의 rotate를 변환해준다.
            if (action.RotateToObstacle)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.TargetRotation, playerController.RotSpeed * Time.deltaTime);

            if (action.EnableTargetMatching)
                MatchTarget(action);

            if (animator.IsInTransition(0) && timer > 0.5f)
                break;

            yield return null;
        }

        yield return new WaitForSeconds(action.PostActionDelay);

        playerController.SetControl(true);
        inAction = false;
    }

    void MatchTarget(ParkourAction action)    // 타겟 매치 기능
    {
        if (animator.isMatchingTarget) return;

        animator.MatchTarget(action.MatchPos, transform.rotation, action.MatchBodyPart, new MatchTargetWeightMask(action.MatchPosWeight, 0),
            action.MatchStartTime, action.MatchTargetTime);
    }
}
