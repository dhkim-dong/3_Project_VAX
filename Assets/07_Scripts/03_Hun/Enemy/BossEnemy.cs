#define ExampleUnity

using Photon.Pun;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.AI;

public class BossEnemy : MonoBehaviourPun
{
    /// <summary>
    /// 적의 상태 종류
    /// </summary>
    public enum EnemyState { Idle, // 대기 상태
                            Move, // 이동 상태
                            Attack, // 공격 상태
                            Damaged, // 맞은 상태
                            Dead // 사망 상태
                            } // 상태에 따라 행동 메소드를 실행할 예정

    /// <summary>
    /// 1페이즈 공격 패턴 2개
    /// 2페이즈 공격 패턴 4개 데미지 업 도?
    /// </summary>
    public enum EnemyPhase // 1페이즈, 2페이즈
    {
        first, 
        second 
    }

    [Header("적 변수")]
    /// <summary>
    /// 적의 현재 상태
    /// </summary>
    [SerializeField]
    [Tooltip("적 상태")]
    private EnemyState enemyState; // 현재 상태를 표현할 변수

    /// <summary>
    /// 적 패턴 바뀌는거 
    /// </summary>
    [SerializeField]
    [Tooltip("적 페이즈")]
    private EnemyPhase enemyPhase; // 현재 페이즈를 표현할 변수

    /// <summary>
    /// 최대 체력 회복용?
    /// </summary>
    [SerializeField]
    [Tooltip("최대 체력")]
    public int maxHp; // 최대 체력
    /// <summary>
    /// 현재 체력
    /// </summary>
    private int currentHp; // 현재 체력

    /// <summary>
    /// 캐릭터 공격력
    /// </summary>
    [SerializeField]
    [Tooltip("공격력")]
    public int atk; // 공격력
    /// <summary>
    /// 캐릭터 방어력 - 필요하면 데미지 계산식?
    /// </summary>
    [SerializeField]
    [Tooltip("방어력")]
    public int def; // 방어력

    /// <summary>
    /// 캐릭터 걷는 속도
    /// </summary>
    [SerializeField]
    [Tooltip("이동 속도")]
    private float moveSpeed; // 걷는 속도

    [SerializeField]
    [Tooltip("페이즈 2 속도")]
    private float runSpeed; // 달리기 속도

    // 부채꼴 반경으로 적 탐지
    /// <summary>
    /// 추적 상태로 바꿀 거리
    /// </summary>
    [SerializeField]
    [Tooltip("시야 거리")]
    private float sightRadius; // 시야 거리
    /// <summary>
    /// 추적 상태로 바꿀 각도
    /// </summary>
    [SerializeField]
    [Tooltip("시야 각도")]
    private float sightAngle; // 시야 반경

    // 부채꼴 반경으로 적 공격
    /// <summary>
    /// 공격 상태로 바꿀 거리
    /// </summary>
    [SerializeField]
    [Tooltip("공격 거리")]
    private float atkRadius; // 공격 거리
    /// <summary>
    /// 공격 상태로 바꿀 각도
    /// </summary>
    [SerializeField]
    [Tooltip("공격 각도")]
    private float atkAngle; // 공격 반경

    // 상태를 확인 할 간격
    [SerializeField]
    [Tooltip("행동 시간")]
    private float actionTime = 0.2f;

    /// <summary>
    /// 현재 공격 상태 5개라 1 ~ 5까지
    /// </summary>
    [SerializeField]
    [Range(1, 5)]
    [Tooltip("페이즈 1 공격 범위")]
    private int phaseNum1;

    /// <summary>
    /// 현재 공격 상태 5개라 1 ~ 5까지
    /// </summary>
    [SerializeField]
    [Range(1, 5)]
    [Tooltip("페이즈 1 공격 범위")]
    private int phaseNum2;

    /// <summary>
    /// 자꾸 생성 안하게 설정
    /// </summary>
    private WaitForSeconds waitTime;

    [Header("적 컴포넌트 및 오브젝트")]
    /// <summary>
    /// 캐릭터 컨트롤러 - 어디에 쓰는지 모르겠다.
    /// </summary>
    [SerializeField]
    [Tooltip("캐릭터 컨트롤러")]
    private CharacterController cc; // 계단 언덕 오를 때 이용되는 컴포넌트

    /// <summary>
    /// 행동 설정 컴포넌트 
    /// 주의할 것은 공격과 맞는거, 사망은 트리거로 작동 
    /// </summary>
    [SerializeField]
    [Tooltip("애니메이터")]
    private Animator anim; // 모션 컴포넌트

    /// <summary>
    /// 길찾기용 컴포넌트
    /// </summary>
    [SerializeField]
    [Tooltip("Ai?")]
    private NavMeshAgent agent; // 길 찾기용? 컴포넌트

    [SerializeField]
    [Tooltip("무기 색상")]
    private MeshRenderer weaponMesh; // 무기 색상

    /// <summary>
    /// 경계 반경 안에 들어오면 플레이어의 위치값을 갖는다
    /// </summary>
    [SerializeField]
    [Tooltip("타겟 위치")]
    private Transform targetTrans; // 타겟 위치

    /// <summary>
    /// 플레이어의 위치 - 적 위치 벡터, 즉 플레이어 방향을 바라보는 단위 벡터
    /// </summary>
    private Vector3 _dir; // 계산용 방향
                          // 플레이어 위치 - 적 위치 
    
    /// <summary>
    /// 정면이 위로 못 향하게 설정, 플레이어의 위치 - 적 위치 벡터에서 y값만 0으로 설정한 벡터
    /// </summary>
    private Vector3 dir; // 시야 방향
                         // dir에서 y 값 = 0인 길이가 1인 벡터
    
    /// <summary>
    /// 공격과 피격 시 1턴간 대기를 줄 꺼!
    /// </summary>
    private bool isDelay; // 한턴 대기 

    /// <summary>
    /// 적으로 인식할 태그
    /// </summary>
    [SerializeField]
    [Tooltip("적으로 인식할 태그 이름")]
    private string playerTag = "Player"; // 적으로 인식할 태그

    /// <summary>
    /// 적으로 인식할 레이어 이름
    /// </summary>
    [SerializeField]
    [Tooltip("적으로 인식할 레이어")]
    private LayerMask targetMask; // 적으로 인식할 레이어

    
    /// <summary>
    /// 장애물으로 인식할 레이어 이름
    /// ex 건물의 외벽
    /// </summary>
    [SerializeField]
    [Tooltip("장애물으로 인식할 레이어")]
    private LayerMask obstacleMask; // 장애물로 인식할 레이어 


#if ExampleUnity
    // 확인용 변수
    float lookingAngle;
    Vector3 leftDir;
    Vector3 rightDir;
    Vector3 lookDir;
    Vector3 leftatkDir;
    Vector3 rightatkDir;
#endif

    // 적 AI의 초기 스펙을 결정하는 셋업 메서드
    [PunRPC]
    public void Setup(int maxHp, int atk, int def, float moveSpeed) // 현재 달리기 이동이 없어서 달리기 속도 제외
    {
        Debug.Log("Setup 메서드 들어옴");

        // 체력 설정
        this.maxHp = maxHp;
        
        SetHp(); // currentHp <= 0 이면 바로 죽음 상태
        
        weaponMesh.material.color = Color.white; // 원래 색상 변형

        // 내비메쉬 에이전트의 이동 속도 설정
        // this.runSpeed = runSpeed;
        this.moveSpeed = moveSpeed;

        // 데미지
        this.atk = atk;

        // 방어력
        this.def = def;

        if (currentHp <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// cc랑 agent 1번만 불러와도 충분하다 생각해서 여기로 빼놓음
    /// </summary>
    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// 부활 시 할 행동 hp 초기화, cc, agent 체크, 이동 속도 설정 그리고 행동 시작
    /// </summary>
    private void OnEnable() // 오브젝트 풀링 기법 고려해서 OnEnable 
    {
        // cc = GetComponent<CharacterController>();
        // agent = GetComponent<NavMeshAgent>();
        SetHp();

        if (cc == null || agent == null) // 이동 방법이 없는 경우 
        {
            gameObject.SetActive(false); // 비활성화
        }

        anim = GetComponent<Animator>(); // 애니메이터 가져오기
        anim?.SetBool("runBool", false);
        SetAtkNum();
        agent.speed = moveSpeed; // 현재는 달리기 없어서 
        
        if (waitTime == null)
        {
            waitTime = new WaitForSeconds(actionTime); // 행동 간격 설정
        }

        StartCoroutine(Action()); // 행동 시작 !!
    }

    /// <summary>
    /// hp 세팅 0이하면 사망 상태로 만들어 줌
    /// </summary>
    private void SetHp()
    {
        currentHp = maxHp;
        enemyState = currentHp > 0 ? EnemyState.Idle : EnemyState.Dead; // 0보다 크면 대기 상태, 작으면 사망 상태 
        enemyPhase = EnemyPhase.first; // 1 페이즈 시작 피 일정 치 달면 2페이즈로 넘어간다
        
    }


    private void OnDisable() // 사망 시 작동
    {
        // 아이템 생성?
    }

    /// <summary>
    /// FSM 알고리즘
    /// </summary>
    /// <returns>대기할 시간</returns>
    private IEnumerator Action() // Action ...
    {
        while (true)
        {
#if ExampleUnity
            // Gizmos로 확인
            lookingAngle = transform.eulerAngles.y;
            rightDir = AngleToDir(lookingAngle + sightAngle * 0.5f);
            leftDir = AngleToDir(lookingAngle - sightAngle * 0.5f);
            lookDir = AngleToDir(lookingAngle);
            rightatkDir = AngleToDir(lookingAngle + atkAngle * 0.5f);
            leftatkDir = AngleToDir(lookingAngle - atkAngle * 0.5f);
#endif

            ChkAction(); // 상태에 따라 행동 
     
            if (enemyState == EnemyState.Dead)
            {
                Debug.Log("코루틴 종료");
                yield break;
            }

            yield return waitTime; // 앞에서 설정한 시간만큼 대기
        }
    }

#if ExampleUnity
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
    /// 시야 표현 메소드
    /// </summary>
    private void OnDrawGizmos()
    {
        // 시야 범위
        Gizmos.DrawWireSphere(transform.position, sightRadius);
        Debug.DrawRay(transform.position, rightDir * sightRadius, Color.blue);
        Debug.DrawRay(transform.position, leftDir * sightRadius, Color.blue);
        Debug.DrawRay(transform.position, lookDir * sightRadius, Color.cyan);
        
        // 공격 범위
        Gizmos.DrawWireSphere(transform.position, atkRadius);
        Debug.DrawRay(transform.position, rightatkDir * atkRadius, Color.red);
        Debug.DrawRay(transform.position, leftatkDir * atkRadius, Color.red);
    }
#endif


    #region 상태 관련 메소드
    /// <summary>
    /// FSM 대기 상태면 대기 행동 메소드 사망이면 사망 상태 메소드 실행
    /// </summary>
    private void ChkAction() // 상태 확인
    {
        switch (enemyState) 
        {
            case EnemyState.Idle: // 초기 상태
                StateIdle(); // 경계 범위 안에 올 때까지 가만히 있음 
                break;

            case EnemyState.Move: // 이동 상태
                StateMove(); // 경계 범위에서 벗어났는지 혹은 공격 범위에 들어왔는지 체크하고
                             // 적의 방향으로 이동
                break;

            case EnemyState.Attack: // 공격 상태
                StateAttack(); // 공격 행동 취함
                               // 특정 타이밍에 공격 범위 안에 있으면 공격한다
                break;

            case EnemyState.Damaged: // 맞는 상태
            case EnemyState.Dead:    // 사망 상태
                                     // 아무것도 안한다
                break;
        }
        return;
    }

    /// <summary>
    /// 대기 행동 메소드 - 경계만 선다.
    /// </summary>
    private void StateIdle() // 대기
                             // 버전업에선 무작위 지점으로 걷기, 두리번 거리기로 대체
    {
        if (FindTarget(sightRadius, sightAngle)) // 경계 범위에 들어왔으면
        {
            // Debug.Log("공격 상태 변환");
            enemyState = EnemyState.Move; // 이동 상태 변환
            anim?.SetBool("walkBool", true); // 애니메이션 있으면 이동 시작!

            return;
        }
    }

    /// <summary>
    /// 이동 행동 메소드 - 경계로 돌아갈지 공격체크하고 타겟으로 이동하는 메소드
    /// </summary>
    private void StateMove() // 이동
                             // 추후에 달리기 
    {
        if (isDelay) // 딜레이 상태일 때
        {
            isDelay = false;
        }
        else // 딜레이 상태가 아닐 시
        {
            if (!agent.enabled) // 이동 이외의 대부분의 상태에서는 agent를 끄기에 여기서 꺼져있는가 체크
            {
                agent.enabled = true;
            }

            if (targetTrans != null)
            {
                // 목적지는 타겟의 장소
                agent.destination = targetTrans.position;
                // Debug.Log("이동 중...");
            }

            // 공격 범위 안이면 공격상태        
            if (FindTarget(atkRadius, atkAngle)) // 먼저 공격 범위 안에 있는지 체크
            {
                enemyState = EnemyState.Attack; // 공격 상태로 변경
                anim?.SetBool("runBool", false); // 공격 범위에 있으므로 공격 모션 실행
                anim?.SetBool("atkBool", true); // 공격 범위에 있으므로 공격 모션 실행
                                                // anim?.SetTrigger("atkTrigger");
                Debug.Log("공격");

                return;
            }

            // 경계 범위 벗어 났는지
            if (!FindTarget(sightRadius, sightAngle)) // 경계 범위 벗어나면 대기 상태로!
            {
                enemyState = EnemyState.Idle; // 대기 상태 변경
                targetTrans = null; // 타겟의 trans 제거
                agent.enabled = false; // agent 끄기
                anim?.SetBool("walkBool", false); // 이동 상태 취소
                Debug.Log("대기 상태 변경");

                return;
            }
        }

    }

    /// <summary>
    /// 공격 행동 - 공격 모션 취하고, 타겟의 방향으로 서서히 바라본다.
    /// </summary>
    public void StateAttack() // 공격
                              // 공격 모션 추후에 1, 2 랜덤 사용
    {
        // agent 끄기 제자리 공격 이라서 끈다 이동형 공격 이면 안끈다
        if (agent.enabled)
        {
            // transform.LookAt(targetTrans);
            agent.enabled = false;
        }

        if (!FindTarget(atkRadius, atkAngle)) // 먼저 공격 범위 안에 있는지 체크
        {
            isDelay = true;
            enemyState = EnemyState.Move; // 공격 상태로 변경
            anim?.SetBool("atkBool", false); // 공격 범위에 없으므로 공격 모션 실행
            anim?.SetBool("walkBool", true); // 이동 범위로 돌아가므로 이동 모션 실행
            Debug.Log("이동 상태 변경");

            return;
        }

        // transform.rotation = Quaternion.Lerp(transform.rotation, 
        //                                    Quaternion.LookRotation(dir),0.5f); // 서서히 바라보게 하기
        transform.LookAt(targetTrans);
    }

    #endregion


    #region Aniamtor Event 메소드
    /// <summary>
    /// 부채꼴 판정 공격 메소드
    /// </summary>
    public void AttackSector() // 부채꼴 공격
    {
        Collider[] cols = Physics.OverlapSphere(transform.position + cc.center, atkRadius,
                                        targetMask); // targetMask에 설정된 적이 구형 반경안에 들어왔는지 확인

        if (cols.Length > 0) // 들어온 애가 있는지 확인
        {
            foreach (var item in cols)
            {
                if (item as CapsuleCollider == null || item.gameObject == gameObject) // cc, capsuleCollider 2개 판별해서 하나만 판별하게 캐스팅함
                {                                                                       // 레이어랑 태그만 맞으면 자기자신도 잡아서 자해하는거 방어 코드
                    continue;
                }

                _dir = (item.gameObject.transform.position - transform.position).normalized;
                dir = _dir;
                dir.y = 0;
                dir = dir.normalized;

                if (Vector3.Angle(_dir, transform.forward) < atkAngle * 0.5f) // 공격 반경 안에 있는 경우
                {
                    // 대상과 적 사이에 장애물이 있는지 체크
                    // 있으면 데미지 안줌!
                    // 예를 들어 플레이어와 적 사이에 벽이 있는데 데미지 들어가는 경우 방지 
                    RaycastHit hit;

                    if (Physics.Raycast(transform.position + cc.center, _dir, out hit, atkRadius, targetMask | obstacleMask)) // 적의 위치에서 플레이어 방향으로 레이저를 쏜다
                    {
                        if (hit.transform.CompareTag(playerTag)) // hit의 태그를 비교
                        {
                            int dmg = enemyPhase == EnemyPhase.second ? 5 + atk : atk ; // 딜 추가!
                            Debug.Log(dmg);
                            // item.gameObject.GetComponent<StatusController>()?.DecreaseHP(dmg); // 페이즈 2면 데미지도 2배!

                            // item.gameObject.GetComponent<StatusController>()?.DecreaseHP(atk);  // item의 대상에 NormalMonster 컴포넌트가 있는지 파악하고
                                                                                                // 데미지 준다
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 내려찍기 공격에 쓸 데미지 판정법
    /// </summary>
    public void AttackRay() // 레이 공격
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position + cc.center, dir, out hit, atkRadius, targetMask))
        {
            int dmg = enemyPhase == EnemyPhase.second ? 5 + atk : atk; // 페이즈에 따라 딜 증가

            hit.transform.gameObject.GetComponent<StatusController>()?.DecreaseHP(dmg); // 근접 공격
        }
    }

    public void SetAtkNum() // 다음 공격 모션 번호 설정
    {
        if (enemyPhase == EnemyPhase.first) // 페이즈 2부터 회전과 3콤보 공격 실행
        {
            anim?.SetInteger("atkInt", UnityEngine.Random.Range(0, phaseNum1));
        }
        else
        {
            anim?.SetInteger("atkInt", UnityEngine.Random.Range(0, phaseNum2));
        }
    }

    /// <summary>
    /// 맞는 모션 탈출 메소드
    /// </summary>
    public void EscapeDamaged()
    {
        enemyState = EnemyState.Move; // 공격과 피격의 기본 행동은 이동으로!
    }

    /// <summary>
    /// 사망 모션 탈출 메소드
    /// </summary>
    public void EscapeDie() // 사망 시 탈출할 메소드
    {
        gameObject.SetActive(false); // 비활성화
    }
    #endregion

    /// <summary>
    /// 범위안에 있는지 판별하는 메소드
    /// </summary>
    /// <param name="Radius">거리</param>
    /// <param name="Angle">각도</param>
    /// <returns>있으면 true,  없으면 false이고 타겟의 transform을 담는다</returns>
    private bool FindTarget(float Radius, float Angle) // 타겟 찾기
                                                       // args : 거리, 각도
    {
        Collider[] cols = Physics.OverlapSphere(transform.position + cc.center, Radius, targetMask);

        dir = Vector3.zero; // 초기화
        // 장애물이 있는거 고려 X 추후에 장애물도 고려해서 식 수정하기

        if (cols.Length > 0)
        {
            foreach(var item in cols)
            {
                if (item.gameObject == gameObject) // 어차피 대상을 향하기에 casting 안함
                    continue;

                _dir = (item.gameObject.transform.position - transform.position);
                dir = _dir;
                dir.y = 0;
                dir = dir.normalized;


                if (Vector3.Angle(_dir,
                                transform.forward) < Angle * 0.5f)
                {
                    // 대상 사이에 벽이나 장애물이 있는지 관통 X 코드
                    RaycastHit hit;

                    if (Physics.Raycast(transform.position + cc.center, _dir, out hit, Radius, targetMask | 0 << 6)) // 적의 위치에서 플레이어 방향으로 레이저를 쏜다
                    {

                        if (hit.transform.CompareTag(playerTag)) // 플레이어가 존재하면
                        {
                            if (enemyState == EnemyState.Idle) // 대기 상태에서만 상대방의 transform을 담는다
                            {
                                targetTrans = item.gameObject.transform; // 대상의 transform 참조
                            }

                            return true;
                        }
                    }
                }
            }
        }
        
        return false;
    }


    #region 외부와 상호작용할 메소드
    /// <summary>
    /// 피격 메소드
    /// </summary>
    /// <param name="attack">공격한 대상의 공격력!</param>
    public void OnDamagedProcess(int attack = 0)
    {
        // 최소 데미지 1 보정 데미지 계산식 
        // 공격력 - 방어력만큼 깎는다
        currentHp -= CalcDmg(attack);

        if (enemyPhase == EnemyPhase.first)
        {
            ChkPhase(); // 페이즈 체크
        }

        if (currentHp > 0) // hp가 0 이상이면 체력만 깎는다
        {
            ChkDamaged(); // 데미지 모션 취할지 체크
        }
        else // hp가 0 이하이므로 사망 상태 처리
        {
            ChkDie(); // 사망 모션 취할지 체크
        }

        return;
    }

    /// <summary>
    /// 데미지 계산식 최소데미지 1 보정!
    /// </summary>
    /// <param name="attack">적의 공격력</param>
    /// <returns>공격력 - 방어력, 최소 데미지 1보정</returns>
    private int CalcDmg(int attack)
    {
        int dmg = attack - this.def;
        if (dmg < 1) dmg = 1;

        return dmg;
    }

    private void ChkPhase() // 70% 미만이면 페이즈 변경 
    {
        if (((10 *currentHp) / maxHp )  <= 6) // 7 - 1 값 넣으면 된다
        {
            enemyPhase = EnemyPhase.second; // 2페이즈 돌입
            weaponMesh.material.color = Color.red; // 무기 색상 빨갛게!
            anim?.SetBool("runBool", true); // 달리기 모션 돌입 그냥 걷는거 없다
            agent.speed = runSpeed; // 달리기 속도 적용
        }
    }

    /// <summary>
    /// 피격 모션 취하게 하는 메소드
    /// </summary>
    private void ChkDamaged()
    {
        if (enemyState != EnemyState.Damaged && !isDelay) // 데미지 상태 중복 진입 방지용
        {
            isDelay = true;
            enemyState = EnemyState.Damaged; // 피격 상태 
            anim?.SetTrigger("dmgTrigger"); // 피격 모션
        }
        return;
    }


    /// <summary>
    /// 사망 모션 취하는 메소드
    /// </summary>
    private void ChkDie()
    {
        currentHp = 0; // 음수 값 보정 식
        if (enemyState != EnemyState.Dead) // 사망 상태 중복 진입 방지용
        {
            enemyState = EnemyState.Dead; // 사망 상태
            anim?.SetTrigger("dieTrigger"); // 사망 모션
        }
        return;
    }
    #endregion
}
