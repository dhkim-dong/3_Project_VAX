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
        Idle, // ���
        Tracking, // ����
        Attack, // ����
        Damaged, // �ǰ�
        Dead // ���
    }

    #region Variable

    //private PhotonView PV;

    // ü��, ���ݷ�, ����, �̼�, �ൿ ����, ���� ��ȯ�� ���� �Ÿ� �� �ݰ�, ���� �Ÿ� �� �ݰ�
    [SerializeField] protected NormalEnemyStats stats;
    [SerializeField] protected NormalEnemyAnimator animator;    // �Ϲ� ���� �ִϸ�����
    [SerializeField] protected NormalState state;               // ���� ����(Idle, tracking, Attack, Damaged, Dead )
    [SerializeField] protected CharacterController cc;          // ĳ���� ��Ʈ�ѷ�
    [SerializeField] protected NavMeshAgent agent;              // ��ã�� AI
    [SerializeField] protected Transform targetTrans;           // Ÿ�� ��ġ
    [SerializeField] protected string playerTag;                // �� �±�
    [SerializeField] protected LayerMask targetMask;            // �� ���̾� 
    [SerializeField] protected LayerMask obstacleMask;          // �ǹ� ���̾�

    protected int currentHp;                                    // ���� ü��
    protected bool isDelay;                                     // ����, �ǰ� ��� �ð�

    protected Vector3 lookDir;                                  // ����� ����
    protected WaitForSeconds waitTime;

#if InUnity
    // Ȯ�ο� ����
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
    /// ������Ʈ �ʱ�ȭ
    /// </summary>
    protected void Awake()
    {
        //PV = GetComponent<PhotonView>();

        // ĳ���� ��Ʈ�ѷ� �޾ƿ���
        cc = GetComponent<CharacterController>();

        // �̵� ai �޾ƿ���
        agent = GetComponent<NavMeshAgent>();

        // �ִϸ����� ������ �޾ƿ���
        if (animator == null) 
        { 
            animator = GetComponent<NormalEnemyAnimator>();
        }

        // �ൿ Ÿ�� 0 ���ϸ� 0.2�� �����ع�����
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

        // �ൿ ���� ����
        waitTime = new WaitForSeconds(stats.actionTime); 

        StartCoroutine(Action());
    }

    private void OnDisable()
    {
        // ������ ����� �����!
    }
    #endregion Unity Method

    #region InUnity

#if InUnity
    /// <summary>
    /// �ð� �������� ������ŭ ȸ���� xz���� ������ ���� ���͸� ��ȯ
    /// </summary>
    /// <param name="angle">����( ���� : �� )</param>
    /// <returns>����</returns>
    Vector3 AngleToDir(float angle) // ���� ���� �޼ҵ�
    {
        float radian = angle * Mathf.Deg2Rad; // ���� ������ ��ȯ
        return new Vector3(Mathf.Sin(radian), 0f, Mathf.Cos(radian)); // xz ���� ȸ�� ����
    }

    /// <summary>
    /// ���� ����(�Ķ�), ���� ���� ǥ��(����)
    /// Gizmos�� ����� ���� ����
    /// </summary>
    private void SetLine() 
    {
        // Gizmos�� Ȯ��
        lookingAngle = transform.eulerAngles.y;
        rightDir = AngleToDir(lookingAngle + stats.trackingAngle * 0.5f);
        leftDir = AngleToDir(lookingAngle - stats.trackingAngle * 0.5f);
        lookDir = AngleToDir(lookingAngle);
        rightatkDir = AngleToDir(lookingAngle + stats.atkAngle * 0.5f);
        leftatkDir = AngleToDir(lookingAngle - stats.atkAngle * 0.5f);
    }

    /// <summary>
    /// �þ� ǥ�� �޼ҵ�
    /// </summary>
    private void OnDrawGizmos()
    {
        // �þ� ����
        Gizmos.DrawWireSphere(transform.position, stats.trackingRadius);
        Debug.DrawRay(transform.position, rightDir * stats.trackingRadius, Color.blue);
        Debug.DrawRay(transform.position, leftDir * stats.trackingRadius, Color.blue);
        Debug.DrawRay(transform.position, lookDir * stats.trackingRadius, Color.cyan);

        // ���� ����
        Gizmos.DrawWireSphere(transform.position, stats.atkRadius);
        Debug.DrawRay(transform.position, rightatkDir * stats.atkRadius, Color.red);
        Debug.DrawRay(transform.position, leftatkDir * stats.atkRadius, Color.red);
    }
#endif
    #endregion

    #region �浹 ����
    /// <summary>
    /// ���� ��ä�� ������ �浹�ϴ� ��ü ã��
    /// </summary>
    /// <param name="Radius">�Ÿ�</param>
    /// <param name="Angle">����</param>
    /// <returns>������ true, ������ false</returns>
    private bool FindTarget(float Radius, float Angle) 
    {
        // ���� targetMask�� �������� ã�´�
        Collider[] cols = Physics.OverlapSphere(transform.position + cc.center, Radius, targetMask);

        // ��� 1�� �̻� ã�� ���
        if (cols.Length > 0) 
        {
            // �Ѱ��� �˻�
            foreach (var item in cols) 
            {
                // ���� ��ü�� ���� ��ü���� Ȯ�� �� true�� ��� �н��Ѵ�.
                if (item.gameObject == gameObject) 
                {
                    continue; 
                }

                // �ʹ� �����
                lookDir = item.gameObject.transform.position - transform.position; 

                if (Vector3.Angle(lookDir, transform.forward) < 0.5f * Angle) 
                {

                    RaycastHit hit;

                    Physics.Raycast(cc.center + transform.position, lookDir, out hit, Radius, targetMask | obstacleMask);

                    // �÷��̾�� �浹���� �� ����
                    if (hit.transform.CompareTag("Player"))
                    {
                        targetTrans = item.gameObject.transform;

                        lookDir.y = 0;
                        lookDir = lookDir.normalized; // �ٶ� �������� ����

                        return true; // �����ϴ� �ְ� �־� true�� Ż��
                    }
                }
            }
        }
        targetTrans = null;
        return false; // �����ϴ� �ְ� ���� ��� ����� ���⿡ false
    }
    #endregion  �浹 ����

    #region FSM
    /// <summary>
    /// �ൿ �޼ҵ�
    /// </summary>
    /// <returns>�տ��� ������ �ð���ŭ ���!</returns>
    private IEnumerator Action()
    {
        while (true)
        {

#if InUnity
            SetLine(); // �� �����ֱ� - �׽�Ʈ ��
#endif

            ChkAction(); // ���� Ȯ��

            yield return waitTime;
        }
    }

    /// <summary>
    /// FSM �޼ҵ�
    /// ���� ���¿� ���� �ൿ �޼ҵ� ����
    /// </summary>
    private void ChkAction() // ���� Ȯ��
    {
        switch (state) // ���� ���¸� ����
        {
            case NormalState.Idle: // ��� ����
                                   // ��� ���� �ȿ� �� �� ���� ������ �ִ´�
                StateIdle();
                break;

            case NormalState.Tracking: // ���� ����
                                       // ����� �Ѿư���
                StateTracking();       
                break;

            case NormalState.Attack: // ���� ����
                                     // �����Ѵ�
                StateAttack();
                break;

            case NormalState.Damaged: // �ǰ� ����
            case NormalState.Dead: // ��� ����
                                   // �ƹ� �͵� ���Ѵ�
                break;
        }
    }

    /// <summary>
    /// ��� ���¿��� �ൿ �ϴ� �޼ҵ�
    /// </summary>
    private void StateIdle() // ��� ���¿��� �ൿ�ϴ� �޼ҵ�
    {
        if (FindTarget(stats.trackingRadius, stats.trackingAngle)) // ��� ���� ����
        {
            state = NormalState.Tracking; // ���� ���� ����
            animator.IdleToMove(); // �̵� ��� ����
        }
    }

    /// <summary>
    /// ���� ���¿��� �ൿ �ϴ� �޼ҵ�
    /// </summary>
    private void StateTracking() // ���� ���¿��� �ൿ�ϴ� �޼ҵ�
    {
        if (isDelay) // 1�ϰ� ���
        {
            isDelay = false;
        }
        else
        {
            if (!agent.enabled) // agent �ϴ� Ű�� ����
            {
                agent.enabled = true;
            }

            if (targetTrans != null) // ����� �ִ� ��� �̵�
            {
                agent.destination = targetTrans.position;
            }

            if(FindTarget(stats.atkRadius, stats.atkAngle)) // ���� ���� �ȿ� �ִ� ���
            {
                state = NormalState.Attack; // ���� ���·� ����
                animator.MoveToAttack(); // ���� ��� ����

                return;
            }

            if (!FindTarget(stats.trackingRadius, stats.trackingAngle)) // ��� ���·� ���ؾ� �ϴ��� üũ
            {
                state = NormalState.Idle; // ��� ���� ����
                agent.enabled = false; // agent ����
                animator.MoveToIdle(); // ��� ���� ��� ����
                return;
            }
        }
    }

    /// <summary>
    /// ���� ���¿��� �ൿ �ϴ� �޼ҵ�
    /// </summary>
    private void StateAttack() // ���� ���¿��� �ൿ�ϴ� �޼ҵ�
    {
        if (agent.enabled) // agent�� ����
        {
            agent.enabled = false;
        }

        if (!FindTarget(stats.atkRadius, stats.atkAngle)) // ���� ���� ������ üũ
        {
            isDelay = true; // ��� �� ����
            state = NormalState.Tracking; // ���� ���·� ����
            animator.AttackToMove(); // ���ݿ��� ���� ���·� ���� �� ��� �ִϸ��̼� ����
            return;
        }

        transform.LookAt(transform.position + lookDir); // ����� �ٶ󺸰� �Ѵ�
    }

    #endregion

    #region �ִϸ����� �̺�Ʈ �޼ҵ�

    /// <summary>
    /// 1�� Ÿ�ٰ����� ����
    /// </summary>
    public void AttackRay() 
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + cc.center, lookDir, out hit, stats.atkRadius, targetMask)) 
        {
            // Ÿ�ٿ� �ǰ� �Լ� ������ �ȴ�
            hit.transform.gameObject.GetComponent<NormalEnemy>()?.OnDamagedProcess(stats.atk); 
        }
    }

    

    /// <summary>
    /// �ǰ� ���� Ż�⿡ �� �޼ҵ�
    /// ���� ���·� ����
    /// </summary>
    public void EscapeDamaged()
    {
        state = NormalState.Tracking;
    }

    /// <summary>
    /// ��� ���� Ż�� �޼ҵ�
    /// ����� ��Ȱ��ȭ�� ������ - �ı��Ϸ��� Destroy!
    /// </summary>
    public void EscapeDie()
    {
        gameObject.SetActive(false);
    }
    #endregion

    #region �ܺο� ��ȣ�ۿ��� �޼ҵ�
    /// <summary>
    /// ������ ��� �� �ǰ� or ��� ����
    /// </summary>
    /// <param name="attack">���� ����� ���ݷ�</param>
    public void OnDamagedProcess(int attack = 0) 
    {
        currentHp -= CalcDmg(attack);
        if (currentHp > 0) // hp�� ����� �ǰ�
        {
            //ChkDamaged(); // �ǰ� 
        }
        else // hp�� 0���ϸ� ���
        {
            //ChkDie(); // ���
        }

        return;
    }

    /// <summary>
    /// ������ ��� �� �ּ� ������ 1 ����
    /// ���ݷ� - ���� = ������  
    /// </summary>
    /// <param name="attack">���� ���ݷ�</param>
    /// <returns>1 �̻��� ������</returns>
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
    /// �ǰ� �� ���ϴ� �޼ҵ�
    /// </summary>
    private void ChkDamaged() 
    {
        if (!isDelay
            && state != NormalState.Damaged // �ǰ� ������� �����ϱ� ���� ���µ� ���⿡ �߰��ϸ� �ȴ� 
            && state != NormalState.Attack) 
        {
            isDelay = true; 
            state = NormalState.Damaged; // �ߺ� ���� ����
            animator.SetDamaged(); // �ǰ� ���
        }
    }

    /// <summary>
    /// ��� �� ���ϴ� �޼ҵ�
    /// </summary>
    private void ChkDie()
    {
        currentHp = 0; // ������ ������
        if (state != NormalState.Dead) // �ߺ� ���� ����
        {
            state = NormalState.Dead; // ��� ����
            animator.SetDead(); // ��� ���
        }
    }

    /// <summary>
    /// ���� ���� �� ������ �޼ҵ�
    /// </summary>
    private void GameOver() // ���� ���� �� ������ �޼ҵ�
    {
        StopAllCoroutines(); // ��� �ൿ ����
    }
    #endregion
}


