// #define InUnity

using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NormalEnemy : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// Idle, Tracking, Attack, Damaged, Dead
    /// </summary>
    public enum NormalState 
    {
        Idle, // 대기
        Tracking, // 추적
        Attack, // 공격
        Damaged, // 피격
        Dead // 사망
    }

    #region Variable

    //private PhotonView PV;

    // 체력, 공격력, 방어력, 이속, 행동 간격, 상태 변환용 추적 거리 및 반경, 공격 거리 및 반경
    [SerializeField] protected NormalEnemyStats stats;
    [SerializeField] protected NormalEnemyAnimator animator;    // 일반 좀비 애니메이터
    [SerializeField] protected NormalState state;               // 현재 상태(Idle, tracking, Attack, Damaged, Dead )
    [SerializeField] protected CharacterController cc;          // 캐릭터 컨트롤러
    [SerializeField] protected NavMeshAgent agent;              // 길찾기 AI
    [SerializeField] protected Transform targetTrans;           // 타겟 위치
    [SerializeField] protected string playerTag;                // 적 태그
    [SerializeField] protected LayerMask targetMask;            // 적 레이어 
    [SerializeField] protected LayerMask obstacleMask;          // 건물 레이어

    protected int currentHp;                                    // 현재 체력
    protected bool isDelay;                                     // 공격, 피격 대기 시간

    protected Vector3 lookDir;                                  // 연산용 벡터
    protected WaitForSeconds waitTime;

#if InUnity
    // 확인용 변수
    float lookingAngle;
    Vector3 leftDir;
    Vector3 rightDir;
    Vector3 lookDir;
    Vector3 leftatkDir;
    Vector3 rightatkDir;
#endif

    #endregion Variable

    #region Unity Method

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    protected void Awake()
    {
        //PV = GetComponent<PhotonView>();

        // 캐릭터 컨트롤러 받아오기
        cc = GetComponent<CharacterController>();

        // 이동 ai 받아오기
        agent = GetComponent<NavMeshAgent>();

        // 애니메이터 없으면 받아오기
        if (animator == null) 
        { 
            animator = GetComponent<NormalEnemyAnimator>();
        }

        // 행동 타임 0 이하면 0.2로 설정해버린다
        if (stats.actionTime <= 0) 
        {
            stats.actionTime = 0.2f;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        state = NormalState.Idle;
        currentHp = stats.maxHp;
        agent.speed = stats.moveSpd;

        // 행동 간격 설정
        waitTime = new WaitForSeconds(stats.actionTime); 

        StartCoroutine(Action());
    }

    private void OnDisable()
    {
        // 아이템 드랍은 여기로!
    }
    #endregion Unity Method

    #region InUnity

#if InUnity
    /// <summary>
    /// 시계 방향으로 각도만큼 회전한 xz평면과 평행한 단위 벡터를 반환
    /// </summary>
    /// <param name="angle">각도( 단위 : 도 )</param>
    /// <returns>벡터</returns>
    Vector3 AngleToDir(float angle) // 방향 설정 메소드
    {
        float radian = angle * Mathf.Deg2Rad; // 라디안 단위로 변환
        return new Vector3(Mathf.Sin(radian), 0f, Mathf.Cos(radian)); // xz 축의 회전 정도
    }

    /// <summary>
    /// 추적 범위(파랑), 공격 범위 표현(빨강)
    /// Gizmos에 사용할 직선 갱신
    /// </summary>
    private void SetLine() 
    {
        // Gizmos로 확인
        lookingAngle = transform.eulerAngles.y;
        rightDir = AngleToDir(lookingAngle + stats.trackingAngle * 0.5f);
        leftDir = AngleToDir(lookingAngle - stats.trackingAngle * 0.5f);
        lookDir = AngleToDir(lookingAngle);
        rightatkDir = AngleToDir(lookingAngle + stats.atkAngle * 0.5f);
        leftatkDir = AngleToDir(lookingAngle - stats.atkAngle * 0.5f);
    }

    /// <summary>
    /// 시야 표현 메소드
    /// </summary>
    private void OnDrawGizmos()
    {
        // 시야 범위
        Gizmos.DrawWireSphere(transform.position, stats.trackingRadius);
        Debug.DrawRay(transform.position, rightDir * stats.trackingRadius, Color.blue);
        Debug.DrawRay(transform.position, leftDir * stats.trackingRadius, Color.blue);
        Debug.DrawRay(transform.position, lookDir * stats.trackingRadius, Color.cyan);

        // 공격 범위
        Gizmos.DrawWireSphere(transform.position, stats.atkRadius);
        Debug.DrawRay(transform.position, rightatkDir * stats.atkRadius, Color.red);
        Debug.DrawRay(transform.position, leftatkDir * stats.atkRadius, Color.red);
    }
#endif
    #endregion

    #region 충돌 감지
    /// <summary>
    /// 구의 부채꼴 범위와 충돌하는 객체 찾기
    /// </summary>
    /// <param name="Radius">거리</param>
    /// <param name="Angle">각도</param>
    /// <returns>있으면 true, 없으면 false</returns>
    private bool FindTarget(float Radius, float Angle) 
    {
        // 구형 targetMask를 기준으로 찾는다
        Collider[] cols = Physics.OverlapSphere(transform.position + cc.center, Radius, targetMask);

        // 적어도 1개 이상 찾는 경우
        if (cols.Length > 0) 
        {
            // 한개씩 검사
            foreach (var item in cols) 
            {
                // 현재 객체와 같은 객체인지 확인 후 true일 경우 패스한다.
                if (item.gameObject == gameObject) 
                {
                    continue; 
                }

                // 초반 연산용
                lookDir = item.gameObject.transform.position - transform.position; 

                if (Vector3.Angle(lookDir, transform.forward) < 0.5f * Angle) 
                {

                    RaycastHit hit;

                    Physics.Raycast(cc.center + transform.position, lookDir, out hit, Radius, targetMask | obstacleMask);

                    // 플레이어와 충돌했을 때 실행
                    if (hit.transform.CompareTag("Player"))
                    {
                        targetTrans = item.gameObject.transform;

                        lookDir.y = 0;
                        lookDir = lookDir.normalized; // 바라볼 방향으로 설정

                        return true; // 만족하는 애가 있어 true로 탈출
                    }
                }
            }
        }
        targetTrans = null;
        return false; // 만족하는 애가 없는 경우 여기로 오기에 false
    }
    #endregion  충돌 감지

    #region FSM
    /// <summary>
    /// 행동 메소드
    /// </summary>
    /// <returns>앞에서 설정한 시간만큼 대기!</returns>
    private IEnumerator Action()
    {
        while (true)
        {

#if InUnity
            SetLine(); // 선 보여주기 - 테스트 용
#endif

            ChkAction(); // 상태 확인

            yield return waitTime;
        }
    }

    /// <summary>
    /// FSM 메소드
    /// 현재 상태에 따라 행동 메소드 실행
    /// </summary>
    private void ChkAction() // 상태 확인
    {
        switch (state) // 현재 상태를 기준
        {
            case NormalState.Idle: // 대기 상태
                                   // 경계 범위 안에 올 때 까지 가만히 있는다
                StateIdle();
                break;

            case NormalState.Tracking: // 추적 상태
                                       // 대상을 쫓아간다
                StateTracking();       
                break;

            case NormalState.Attack: // 공격 상태
                                     // 공격한다
                StateAttack();
                break;

            case NormalState.Damaged: // 피격 상태
            case NormalState.Dead: // 사망 상태
                                   // 아무 것도 안한다
                break;
        }
    }

    /// <summary>
    /// 대기 상태에서 행동 하는 메소드
    /// </summary>
    private void StateIdle() // 대기 상태에서 행동하는 메소드
    {
        if (FindTarget(stats.trackingRadius, stats.trackingAngle)) // 경계 범위 판정
        {
            state = NormalState.Tracking; // 추적 상태 변경
            animator.IdleToMove(); // 이동 모션 시작
        }
    }

    /// <summary>
    /// 추적 상태에서 행동 하는 메소드
    /// </summary>
    private void StateTracking() // 추적 상태에서 행동하는 메소드
    {
        if (isDelay) // 1턴간 대기
        {
            isDelay = false;
        }
        else
        {
            if (!agent.enabled) // agent 일단 키고 본다
            {
                agent.enabled = true;
            }

            if (targetTrans != null) // 대상이 있는 경우 이동
            {
                agent.destination = targetTrans.position;
            }

            if(FindTarget(stats.atkRadius, stats.atkAngle)) // 공격 범위 안에 있는 경우
            {
                state = NormalState.Attack; // 공격 상태로 변경
                animator.MoveToAttack(); // 공격 모션 실행

                return;
            }

            if (!FindTarget(stats.trackingRadius, stats.trackingAngle)) // 대기 상태로 변해야 하는지 체크
            {
                state = NormalState.Idle; // 대기 상태 변경
                agent.enabled = false; // agent 끄기
                animator.MoveToIdle(); // 대기 상태 모션 실행
                return;
            }
        }
    }

    /// <summary>
    /// 공격 상태에서 행동 하는 메소드
    /// </summary>
    private void StateAttack() // 공격 상태에서 행동하는 메소드
    {
        if (agent.enabled) // agent를 끈다
        {
            agent.enabled = false;
        }

        if (!FindTarget(stats.atkRadius, stats.atkAngle)) // 공격 범위 밖인지 체크
        {
            isDelay = true; // 대기 텀 존재
            state = NormalState.Tracking; // 추적 상태로 변경
            animator.AttackToMove(); // 공격에서 추적 상태로 변형 된 경우 애니메이션 실행
            return;
        }

        transform.LookAt(transform.position + lookDir); // 대상을 바라보게 한다
    }

    #endregion

    #region 애니메이터 이벤트 메소드

    /// <summary>
    /// 1인 타겟감안한 공격
    /// </summary>
    public void AttackRay() 
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + cc.center, lookDir, out hit, stats.atkRadius, targetMask)) 
        {
            // 타겟에 피격 함수 넣으면 된다
            hit.transform.gameObject.GetComponent<NormalEnemy>()?.OnDamagedProcess(stats.atk); 
        }
    }

    

    /// <summary>
    /// 피격 상태 탈출에 쓸 메소드
    /// 추적 상태로 변경
    /// </summary>
    public void EscapeDamaged()
    {
        state = NormalState.Tracking;
    }

    /// <summary>
    /// 사망 상태 탈출 메소드
    /// 현재는 비활성화로 돌린다 - 파괴하려면 Destroy!
    /// </summary>
    public void EscapeDie()
    {
        gameObject.SetActive(false);
    }
    #endregion

    #region 외부와 상호작용할 메소드
    /// <summary>
    /// 데미지 계산 및 피격 or 사망 판정
    /// </summary>
    /// <param name="attack">공격 대상의 공격력</param>
    public void OnDamagedProcess(int attack = 0) 
    {
        currentHp -= CalcDmg(attack);
        if (currentHp > 0) // hp가 양수면 피격
        {
            //ChkDamaged(); // 피격 
        }
        else // hp가 0이하면 사망
        {
            //ChkDie(); // 사망
        }

        return;
    }

    /// <summary>
    /// 데미지 계산 식 최소 데미지 1 보정
    /// 공격력 - 방어력 = 데미지  
    /// </summary>
    /// <param name="attack">적의 공격력</param>
    /// <returns>1 이상의 데미지</returns>
    private int CalcDmg(int attack) // 
    {
        int dmg = attack - stats.def; // 
        if (dmg < 1) // 
        {
            dmg = 1; //
        }
        return dmg; // 
    }

    /// <summary>
    /// 피격 시 취하는 메소드
    /// </summary>
    private void ChkDamaged() 
    {
        if (!isDelay
            && state != NormalState.Damaged // 피격 모션으로 진입하기 싫은 상태들 여기에 추가하면 된다 
            && state != NormalState.Attack) 
        {
            isDelay = true; 
            state = NormalState.Damaged; // 중복 진입 막기
            animator.SetDamaged(); // 피격 모션
        }
    }

    /// <summary>
    /// 사망 시 취하는 메소드
    /// </summary>
    private void ChkDie()
    {
        currentHp = 0; // 음수값 방지용
        if (state != NormalState.Dead) // 중복 진입 방지
        {
            state = NormalState.Dead; // 사망 상태
            animator.SetDead(); // 사망 모션
        }
    }

    /// <summary>
    /// 게임 끝날 때 실행할 메소드
    /// </summary>
    private void GameOver() // 게임 끝날 때 실행할 메소드
    {
        StopAllCoroutines(); // 모든 행동 멈춤
    }
    #endregion
}


