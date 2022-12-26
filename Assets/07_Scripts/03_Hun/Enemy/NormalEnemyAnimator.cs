using Photon.Pun;
using UnityEngine;

/// <summary>
/// 일반 좀비 애니메이터 관리
/// </summary>
public class NormalEnemyAnimator : MonoBehaviourPunCallbacks
{
    #region Variable

    //private PhotonView PV;

    /// <summary>
    /// Proto Type - Idle, Move, Attack, Damaged, Dead 상태 밖에 없다.
    /// 애니메이터 params로는 walkBool, atkBool 정도만 존재 
    /// 추후에 같은 상태라도 다른 행동을 보이기 위해 int로 변경할 예정
    /// 0은 false, 양수는 true
    /// </summary>
    [SerializeField][Tooltip("애니메이터")] private Animator anim;

    /// <summary>
    /// 대기, 추적, 공격, 피격, 사망 애니메이션 갯수
    /// 현재는 4, 8, 8, 5, 2 개 있다
    /// </summary>
    public static int[] maxAnimNum = { 4, 8, 8, 5, 2 };

    /// <summary>
    /// 애니메이터 넘버
    /// 대기, 추적, 공격, 피격, 사망 번호이다
    /// </summary>
    [SerializeField]
    [Tooltip("애니메이션 넘버")]
    private int[] animNum = new int[5]; // 애니메이터 넘버

    #endregion Variable


    #region Unity 메소드
    private void Awake()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>(); // 애니메이터 받아오기
        }

        // 포톤뷰 컴포넌트 할당
        //PV = GetComponent<PhotonView>();

        SetAnimNum(); // 애니메이션 선택
    }
    #endregion

    #region 내부 연산
    /// <summary>
    /// 애니메이션 번호 계산
    /// </summary>
    private void SetAnimNum() // 애니메이션 선택
    {
        for (int i = 0; i < animNum.Length; i++) // 애니메이션 번호 랜덤 선택
        {
            animNum[i] = Random.Range(0, maxAnimNum[i]);
        }

        // 애니메이션 넘버 설정
        anim.SetInteger("idleInt", animNum[0]); // 대기 번호
        anim.SetInteger("trackingInt", animNum[1]); // 추적 번호
        anim.SetInteger("atkInt", animNum[2]); // 공격 번호
        anim.SetInteger("dmgInt", animNum[3]); // 피격 번호
        anim.SetInteger("deadInt", animNum[4]); // 사망 번호
    }

    #endregion


    #region FSM에서 쓸 메소드
    /// <summary>
    /// Idle 상태에서 Move 상태로 변할 때 실행할 애니메이션 메소드
    /// </summary>
    public void IdleToMove()
    {
        anim.SetBool("walkBool", true); 
    }

    /// <summary>
    /// Move 상태에서 Idle 상태로 변할 때 실행할 애니메이션 메소드
    /// </summary>
    public void MoveToIdle()
    {
        anim.SetBool("atkBool", false);
        anim.SetBool("walkBool", false); 
    }

    /// <summary>
    /// Move 상태에서 Attack 상태로 변할 때 실행할 애니메이션 메소드
    /// </summary>
    public void MoveToAttack()
    {
        anim.SetBool("atkBool", true);
    }

    /// <summary>
    /// Attack 상태에서 Move 상태로 변할 때 실행할 애니메이션 메소드
    /// </summary>
    public void AttackToMove()
    {
        anim.SetBool("atkBool", false);
    }

    /// <summary>
    /// Damaged 상태에서 실행할 애니메이션 메소드
    /// </summary>
    public void SetDamaged()
    {
        anim.SetTrigger("dmgTrigger");
    }

    /// <summary>
    /// Dead 상태에서 실행할 애니메이션 메소드
    /// </summary>
    public void SetDead()
    {
        anim.SetTrigger("dieTrigger");
    }
    #endregion
}
