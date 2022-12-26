#define ExampleUnity

using Photon.Pun;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.AI;

public class BossEnemy : MonoBehaviourPun
{
    /// <summary>
    /// ���� ���� ����
    /// </summary>
    public enum EnemyState { Idle, // ��� ����
                            Move, // �̵� ����
                            Attack, // ���� ����
                            Damaged, // ���� ����
                            Dead // ��� ����
                            } // ���¿� ���� �ൿ �޼ҵ带 ������ ����

    /// <summary>
    /// 1������ ���� ���� 2��
    /// 2������ ���� ���� 4�� ������ �� ��?
    /// </summary>
    public enum EnemyPhase // 1������, 2������
    {
        first, 
        second 
    }

    [Header("�� ����")]
    /// <summary>
    /// ���� ���� ����
    /// </summary>
    [SerializeField]
    [Tooltip("�� ����")]
    private EnemyState enemyState; // ���� ���¸� ǥ���� ����

    /// <summary>
    /// �� ���� �ٲ�°� 
    /// </summary>
    [SerializeField]
    [Tooltip("�� ������")]
    private EnemyPhase enemyPhase; // ���� ����� ǥ���� ����

    /// <summary>
    /// �ִ� ü�� ȸ����?
    /// </summary>
    [SerializeField]
    [Tooltip("�ִ� ü��")]
    public int maxHp; // �ִ� ü��
    /// <summary>
    /// ���� ü��
    /// </summary>
    private int currentHp; // ���� ü��

    /// <summary>
    /// ĳ���� ���ݷ�
    /// </summary>
    [SerializeField]
    [Tooltip("���ݷ�")]
    public int atk; // ���ݷ�
    /// <summary>
    /// ĳ���� ���� - �ʿ��ϸ� ������ ����?
    /// </summary>
    [SerializeField]
    [Tooltip("����")]
    public int def; // ����

    /// <summary>
    /// ĳ���� �ȴ� �ӵ�
    /// </summary>
    [SerializeField]
    [Tooltip("�̵� �ӵ�")]
    private float moveSpeed; // �ȴ� �ӵ�

    [SerializeField]
    [Tooltip("������ 2 �ӵ�")]
    private float runSpeed; // �޸��� �ӵ�

    // ��ä�� �ݰ����� �� Ž��
    /// <summary>
    /// ���� ���·� �ٲ� �Ÿ�
    /// </summary>
    [SerializeField]
    [Tooltip("�þ� �Ÿ�")]
    private float sightRadius; // �þ� �Ÿ�
    /// <summary>
    /// ���� ���·� �ٲ� ����
    /// </summary>
    [SerializeField]
    [Tooltip("�þ� ����")]
    private float sightAngle; // �þ� �ݰ�

    // ��ä�� �ݰ����� �� ����
    /// <summary>
    /// ���� ���·� �ٲ� �Ÿ�
    /// </summary>
    [SerializeField]
    [Tooltip("���� �Ÿ�")]
    private float atkRadius; // ���� �Ÿ�
    /// <summary>
    /// ���� ���·� �ٲ� ����
    /// </summary>
    [SerializeField]
    [Tooltip("���� ����")]
    private float atkAngle; // ���� �ݰ�

    // ���¸� Ȯ�� �� ����
    [SerializeField]
    [Tooltip("�ൿ �ð�")]
    private float actionTime = 0.2f;

    /// <summary>
    /// ���� ���� ���� 5���� 1 ~ 5����
    /// </summary>
    [SerializeField]
    [Range(1, 5)]
    [Tooltip("������ 1 ���� ����")]
    private int phaseNum1;

    /// <summary>
    /// ���� ���� ���� 5���� 1 ~ 5����
    /// </summary>
    [SerializeField]
    [Range(1, 5)]
    [Tooltip("������ 1 ���� ����")]
    private int phaseNum2;

    /// <summary>
    /// �ڲ� ���� ���ϰ� ����
    /// </summary>
    private WaitForSeconds waitTime;

    [Header("�� ������Ʈ �� ������Ʈ")]
    /// <summary>
    /// ĳ���� ��Ʈ�ѷ� - ��� ������ �𸣰ڴ�.
    /// </summary>
    [SerializeField]
    [Tooltip("ĳ���� ��Ʈ�ѷ�")]
    private CharacterController cc; // ��� ��� ���� �� �̿�Ǵ� ������Ʈ

    /// <summary>
    /// �ൿ ���� ������Ʈ 
    /// ������ ���� ���ݰ� �´°�, ����� Ʈ���ŷ� �۵� 
    /// </summary>
    [SerializeField]
    [Tooltip("�ִϸ�����")]
    private Animator anim; // ��� ������Ʈ

    /// <summary>
    /// ��ã��� ������Ʈ
    /// </summary>
    [SerializeField]
    [Tooltip("Ai?")]
    private NavMeshAgent agent; // �� ã���? ������Ʈ

    [SerializeField]
    [Tooltip("���� ����")]
    private MeshRenderer weaponMesh; // ���� ����

    /// <summary>
    /// ��� �ݰ� �ȿ� ������ �÷��̾��� ��ġ���� ���´�
    /// </summary>
    [SerializeField]
    [Tooltip("Ÿ�� ��ġ")]
    private Transform targetTrans; // Ÿ�� ��ġ

    /// <summary>
    /// �÷��̾��� ��ġ - �� ��ġ ����, �� �÷��̾� ������ �ٶ󺸴� ���� ����
    /// </summary>
    private Vector3 _dir; // ���� ����
                          // �÷��̾� ��ġ - �� ��ġ 
    
    /// <summary>
    /// ������ ���� �� ���ϰ� ����, �÷��̾��� ��ġ - �� ��ġ ���Ϳ��� y���� 0���� ������ ����
    /// </summary>
    private Vector3 dir; // �þ� ����
                         // dir���� y �� = 0�� ���̰� 1�� ����
    
    /// <summary>
    /// ���ݰ� �ǰ� �� 1�ϰ� ��⸦ �� ��!
    /// </summary>
    private bool isDelay; // ���� ��� 

    /// <summary>
    /// ������ �ν��� �±�
    /// </summary>
    [SerializeField]
    [Tooltip("������ �ν��� �±� �̸�")]
    private string playerTag = "Player"; // ������ �ν��� �±�

    /// <summary>
    /// ������ �ν��� ���̾� �̸�
    /// </summary>
    [SerializeField]
    [Tooltip("������ �ν��� ���̾�")]
    private LayerMask targetMask; // ������ �ν��� ���̾�

    
    /// <summary>
    /// ��ֹ����� �ν��� ���̾� �̸�
    /// ex �ǹ��� �ܺ�
    /// </summary>
    [SerializeField]
    [Tooltip("��ֹ����� �ν��� ���̾�")]
    private LayerMask obstacleMask; // ��ֹ��� �ν��� ���̾� 


#if ExampleUnity
    // Ȯ�ο� ����
    float lookingAngle;
    Vector3 leftDir;
    Vector3 rightDir;
    Vector3 lookDir;
    Vector3 leftatkDir;
    Vector3 rightatkDir;
#endif

    // �� AI�� �ʱ� ������ �����ϴ� �¾� �޼���
    [PunRPC]
    public void Setup(int maxHp, int atk, int def, float moveSpeed) // ���� �޸��� �̵��� ��� �޸��� �ӵ� ����
    {
        Debug.Log("Setup �޼��� ����");

        // ü�� ����
        this.maxHp = maxHp;
        
        SetHp(); // currentHp <= 0 �̸� �ٷ� ���� ����
        
        weaponMesh.material.color = Color.white; // ���� ���� ����

        // ����޽� ������Ʈ�� �̵� �ӵ� ����
        // this.runSpeed = runSpeed;
        this.moveSpeed = moveSpeed;

        // ������
        this.atk = atk;

        // ����
        this.def = def;

        if (currentHp <= 0)
        {
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// cc�� agent 1���� �ҷ��͵� ����ϴ� �����ؼ� ����� ������
    /// </summary>
    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// ��Ȱ �� �� �ൿ hp �ʱ�ȭ, cc, agent üũ, �̵� �ӵ� ���� �׸��� �ൿ ����
    /// </summary>
    private void OnEnable() // ������Ʈ Ǯ�� ��� ����ؼ� OnEnable 
    {
        // cc = GetComponent<CharacterController>();
        // agent = GetComponent<NavMeshAgent>();
        SetHp();

        if (cc == null || agent == null) // �̵� ����� ���� ��� 
        {
            gameObject.SetActive(false); // ��Ȱ��ȭ
        }

        anim = GetComponent<Animator>(); // �ִϸ����� ��������
        anim?.SetBool("runBool", false);
        SetAtkNum();
        agent.speed = moveSpeed; // ����� �޸��� ��� 
        
        if (waitTime == null)
        {
            waitTime = new WaitForSeconds(actionTime); // �ൿ ���� ����
        }

        StartCoroutine(Action()); // �ൿ ���� !!
    }

    /// <summary>
    /// hp ���� 0���ϸ� ��� ���·� ����� ��
    /// </summary>
    private void SetHp()
    {
        currentHp = maxHp;
        enemyState = currentHp > 0 ? EnemyState.Idle : EnemyState.Dead; // 0���� ũ�� ��� ����, ������ ��� ���� 
        enemyPhase = EnemyPhase.first; // 1 ������ ���� �� ���� ġ �޸� 2������� �Ѿ��
        
    }


    private void OnDisable() // ��� �� �۵�
    {
        // ������ ����?
    }

    /// <summary>
    /// FSM �˰���
    /// </summary>
    /// <returns>����� �ð�</returns>
    private IEnumerator Action() // Action ...
    {
        while (true)
        {
#if ExampleUnity
            // Gizmos�� Ȯ��
            lookingAngle = transform.eulerAngles.y;
            rightDir = AngleToDir(lookingAngle + sightAngle * 0.5f);
            leftDir = AngleToDir(lookingAngle - sightAngle * 0.5f);
            lookDir = AngleToDir(lookingAngle);
            rightatkDir = AngleToDir(lookingAngle + atkAngle * 0.5f);
            leftatkDir = AngleToDir(lookingAngle - atkAngle * 0.5f);
#endif

            ChkAction(); // ���¿� ���� �ൿ 
     
            if (enemyState == EnemyState.Dead)
            {
                Debug.Log("�ڷ�ƾ ����");
                yield break;
            }

            yield return waitTime; // �տ��� ������ �ð���ŭ ���
        }
    }

#if ExampleUnity
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
    /// �þ� ǥ�� �޼ҵ�
    /// </summary>
    private void OnDrawGizmos()
    {
        // �þ� ����
        Gizmos.DrawWireSphere(transform.position, sightRadius);
        Debug.DrawRay(transform.position, rightDir * sightRadius, Color.blue);
        Debug.DrawRay(transform.position, leftDir * sightRadius, Color.blue);
        Debug.DrawRay(transform.position, lookDir * sightRadius, Color.cyan);
        
        // ���� ����
        Gizmos.DrawWireSphere(transform.position, atkRadius);
        Debug.DrawRay(transform.position, rightatkDir * atkRadius, Color.red);
        Debug.DrawRay(transform.position, leftatkDir * atkRadius, Color.red);
    }
#endif


    #region ���� ���� �޼ҵ�
    /// <summary>
    /// FSM ��� ���¸� ��� �ൿ �޼ҵ� ����̸� ��� ���� �޼ҵ� ����
    /// </summary>
    private void ChkAction() // ���� Ȯ��
    {
        switch (enemyState) 
        {
            case EnemyState.Idle: // �ʱ� ����
                StateIdle(); // ��� ���� �ȿ� �� ������ ������ ���� 
                break;

            case EnemyState.Move: // �̵� ����
                StateMove(); // ��� �������� ������� Ȥ�� ���� ������ ���Դ��� üũ�ϰ�
                             // ���� �������� �̵�
                break;

            case EnemyState.Attack: // ���� ����
                StateAttack(); // ���� �ൿ ����
                               // Ư�� Ÿ�ֿ̹� ���� ���� �ȿ� ������ �����Ѵ�
                break;

            case EnemyState.Damaged: // �´� ����
            case EnemyState.Dead:    // ��� ����
                                     // �ƹ��͵� ���Ѵ�
                break;
        }
        return;
    }

    /// <summary>
    /// ��� �ൿ �޼ҵ� - ��踸 ����.
    /// </summary>
    private void StateIdle() // ���
                             // ���������� ������ �������� �ȱ�, �θ��� �Ÿ���� ��ü
    {
        if (FindTarget(sightRadius, sightAngle)) // ��� ������ ��������
        {
            // Debug.Log("���� ���� ��ȯ");
            enemyState = EnemyState.Move; // �̵� ���� ��ȯ
            anim?.SetBool("walkBool", true); // �ִϸ��̼� ������ �̵� ����!

            return;
        }
    }

    /// <summary>
    /// �̵� �ൿ �޼ҵ� - ���� ���ư��� ����üũ�ϰ� Ÿ������ �̵��ϴ� �޼ҵ�
    /// </summary>
    private void StateMove() // �̵�
                             // ���Ŀ� �޸��� 
    {
        if (isDelay) // ������ ������ ��
        {
            isDelay = false;
        }
        else // ������ ���°� �ƴ� ��
        {
            if (!agent.enabled) // �̵� �̿��� ��κ��� ���¿����� agent�� ���⿡ ���⼭ �����ִ°� üũ
            {
                agent.enabled = true;
            }

            if (targetTrans != null)
            {
                // �������� Ÿ���� ���
                agent.destination = targetTrans.position;
                // Debug.Log("�̵� ��...");
            }

            // ���� ���� ���̸� ���ݻ���        
            if (FindTarget(atkRadius, atkAngle)) // ���� ���� ���� �ȿ� �ִ��� üũ
            {
                enemyState = EnemyState.Attack; // ���� ���·� ����
                anim?.SetBool("runBool", false); // ���� ������ �����Ƿ� ���� ��� ����
                anim?.SetBool("atkBool", true); // ���� ������ �����Ƿ� ���� ��� ����
                                                // anim?.SetTrigger("atkTrigger");
                Debug.Log("����");

                return;
            }

            // ��� ���� ���� ������
            if (!FindTarget(sightRadius, sightAngle)) // ��� ���� ����� ��� ���·�!
            {
                enemyState = EnemyState.Idle; // ��� ���� ����
                targetTrans = null; // Ÿ���� trans ����
                agent.enabled = false; // agent ����
                anim?.SetBool("walkBool", false); // �̵� ���� ���
                Debug.Log("��� ���� ����");

                return;
            }
        }

    }

    /// <summary>
    /// ���� �ൿ - ���� ��� ���ϰ�, Ÿ���� �������� ������ �ٶ󺻴�.
    /// </summary>
    public void StateAttack() // ����
                              // ���� ��� ���Ŀ� 1, 2 ���� ���
    {
        // agent ���� ���ڸ� ���� �̶� ���� �̵��� ���� �̸� �Ȳ���
        if (agent.enabled)
        {
            // transform.LookAt(targetTrans);
            agent.enabled = false;
        }

        if (!FindTarget(atkRadius, atkAngle)) // ���� ���� ���� �ȿ� �ִ��� üũ
        {
            isDelay = true;
            enemyState = EnemyState.Move; // ���� ���·� ����
            anim?.SetBool("atkBool", false); // ���� ������ �����Ƿ� ���� ��� ����
            anim?.SetBool("walkBool", true); // �̵� ������ ���ư��Ƿ� �̵� ��� ����
            Debug.Log("�̵� ���� ����");

            return;
        }

        // transform.rotation = Quaternion.Lerp(transform.rotation, 
        //                                    Quaternion.LookRotation(dir),0.5f); // ������ �ٶ󺸰� �ϱ�
        transform.LookAt(targetTrans);
    }

    #endregion


    #region Aniamtor Event �޼ҵ�
    /// <summary>
    /// ��ä�� ���� ���� �޼ҵ�
    /// </summary>
    public void AttackSector() // ��ä�� ����
    {
        Collider[] cols = Physics.OverlapSphere(transform.position + cc.center, atkRadius,
                                        targetMask); // targetMask�� ������ ���� ���� �ݰ�ȿ� ���Դ��� Ȯ��

        if (cols.Length > 0) // ���� �ְ� �ִ��� Ȯ��
        {
            foreach (var item in cols)
            {
                if (item as CapsuleCollider == null || item.gameObject == gameObject) // cc, capsuleCollider 2�� �Ǻ��ؼ� �ϳ��� �Ǻ��ϰ� ĳ������
                {                                                                       // ���̾�� �±׸� ������ �ڱ��ڽŵ� ��Ƽ� �����ϴ°� ��� �ڵ�
                    continue;
                }

                _dir = (item.gameObject.transform.position - transform.position).normalized;
                dir = _dir;
                dir.y = 0;
                dir = dir.normalized;

                if (Vector3.Angle(_dir, transform.forward) < atkAngle * 0.5f) // ���� �ݰ� �ȿ� �ִ� ���
                {
                    // ���� �� ���̿� ��ֹ��� �ִ��� üũ
                    // ������ ������ ����!
                    // ���� ��� �÷��̾�� �� ���̿� ���� �ִµ� ������ ���� ��� ���� 
                    RaycastHit hit;

                    if (Physics.Raycast(transform.position + cc.center, _dir, out hit, atkRadius, targetMask | obstacleMask)) // ���� ��ġ���� �÷��̾� �������� �������� ���
                    {
                        if (hit.transform.CompareTag(playerTag)) // hit�� �±׸� ��
                        {
                            int dmg = enemyPhase == EnemyPhase.second ? 5 + atk : atk ; // �� �߰�!
                            Debug.Log(dmg);
                            // item.gameObject.GetComponent<StatusController>()?.DecreaseHP(dmg); // ������ 2�� �������� 2��!

                            // item.gameObject.GetComponent<StatusController>()?.DecreaseHP(atk);  // item�� ��� NormalMonster ������Ʈ�� �ִ��� �ľ��ϰ�
                                                                                                // ������ �ش�
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// ������� ���ݿ� �� ������ ������
    /// </summary>
    public void AttackRay() // ���� ����
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position + cc.center, dir, out hit, atkRadius, targetMask))
        {
            int dmg = enemyPhase == EnemyPhase.second ? 5 + atk : atk; // ����� ���� �� ����

            hit.transform.gameObject.GetComponent<StatusController>()?.DecreaseHP(dmg); // ���� ����
        }
    }

    public void SetAtkNum() // ���� ���� ��� ��ȣ ����
    {
        if (enemyPhase == EnemyPhase.first) // ������ 2���� ȸ���� 3�޺� ���� ����
        {
            anim?.SetInteger("atkInt", UnityEngine.Random.Range(0, phaseNum1));
        }
        else
        {
            anim?.SetInteger("atkInt", UnityEngine.Random.Range(0, phaseNum2));
        }
    }

    /// <summary>
    /// �´� ��� Ż�� �޼ҵ�
    /// </summary>
    public void EscapeDamaged()
    {
        enemyState = EnemyState.Move; // ���ݰ� �ǰ��� �⺻ �ൿ�� �̵�����!
    }

    /// <summary>
    /// ��� ��� Ż�� �޼ҵ�
    /// </summary>
    public void EscapeDie() // ��� �� Ż���� �޼ҵ�
    {
        gameObject.SetActive(false); // ��Ȱ��ȭ
    }
    #endregion

    /// <summary>
    /// �����ȿ� �ִ��� �Ǻ��ϴ� �޼ҵ�
    /// </summary>
    /// <param name="Radius">�Ÿ�</param>
    /// <param name="Angle">����</param>
    /// <returns>������ true,  ������ false�̰� Ÿ���� transform�� ��´�</returns>
    private bool FindTarget(float Radius, float Angle) // Ÿ�� ã��
                                                       // args : �Ÿ�, ����
    {
        Collider[] cols = Physics.OverlapSphere(transform.position + cc.center, Radius, targetMask);

        dir = Vector3.zero; // �ʱ�ȭ
        // ��ֹ��� �ִ°� ��� X ���Ŀ� ��ֹ��� ����ؼ� �� �����ϱ�

        if (cols.Length > 0)
        {
            foreach(var item in cols)
            {
                if (item.gameObject == gameObject) // ������ ����� ���ϱ⿡ casting ����
                    continue;

                _dir = (item.gameObject.transform.position - transform.position);
                dir = _dir;
                dir.y = 0;
                dir = dir.normalized;


                if (Vector3.Angle(_dir,
                                transform.forward) < Angle * 0.5f)
                {
                    // ��� ���̿� ���̳� ��ֹ��� �ִ��� ���� X �ڵ�
                    RaycastHit hit;

                    if (Physics.Raycast(transform.position + cc.center, _dir, out hit, Radius, targetMask | 0 << 6)) // ���� ��ġ���� �÷��̾� �������� �������� ���
                    {

                        if (hit.transform.CompareTag(playerTag)) // �÷��̾ �����ϸ�
                        {
                            if (enemyState == EnemyState.Idle) // ��� ���¿����� ������ transform�� ��´�
                            {
                                targetTrans = item.gameObject.transform; // ����� transform ����
                            }

                            return true;
                        }
                    }
                }
            }
        }
        
        return false;
    }


    #region �ܺο� ��ȣ�ۿ��� �޼ҵ�
    /// <summary>
    /// �ǰ� �޼ҵ�
    /// </summary>
    /// <param name="attack">������ ����� ���ݷ�!</param>
    public void OnDamagedProcess(int attack = 0)
    {
        // �ּ� ������ 1 ���� ������ ���� 
        // ���ݷ� - ���¸�ŭ ��´�
        currentHp -= CalcDmg(attack);

        if (enemyPhase == EnemyPhase.first)
        {
            ChkPhase(); // ������ üũ
        }

        if (currentHp > 0) // hp�� 0 �̻��̸� ü�¸� ��´�
        {
            ChkDamaged(); // ������ ��� ������ üũ
        }
        else // hp�� 0 �����̹Ƿ� ��� ���� ó��
        {
            ChkDie(); // ��� ��� ������ üũ
        }

        return;
    }

    /// <summary>
    /// ������ ���� �ּҵ����� 1 ����!
    /// </summary>
    /// <param name="attack">���� ���ݷ�</param>
    /// <returns>���ݷ� - ����, �ּ� ������ 1����</returns>
    private int CalcDmg(int attack)
    {
        int dmg = attack - this.def;
        if (dmg < 1) dmg = 1;

        return dmg;
    }

    private void ChkPhase() // 70% �̸��̸� ������ ���� 
    {
        if (((10 *currentHp) / maxHp )  <= 6) // 7 - 1 �� ������ �ȴ�
        {
            enemyPhase = EnemyPhase.second; // 2������ ����
            weaponMesh.material.color = Color.red; // ���� ���� ������!
            anim?.SetBool("runBool", true); // �޸��� ��� ���� �׳� �ȴ°� ����
            agent.speed = runSpeed; // �޸��� �ӵ� ����
        }
    }

    /// <summary>
    /// �ǰ� ��� ���ϰ� �ϴ� �޼ҵ�
    /// </summary>
    private void ChkDamaged()
    {
        if (enemyState != EnemyState.Damaged && !isDelay) // ������ ���� �ߺ� ���� ������
        {
            isDelay = true;
            enemyState = EnemyState.Damaged; // �ǰ� ���� 
            anim?.SetTrigger("dmgTrigger"); // �ǰ� ���
        }
        return;
    }


    /// <summary>
    /// ��� ��� ���ϴ� �޼ҵ�
    /// </summary>
    private void ChkDie()
    {
        currentHp = 0; // ���� �� ���� ��
        if (enemyState != EnemyState.Dead) // ��� ���� �ߺ� ���� ������
        {
            enemyState = EnemyState.Dead; // ��� ����
            anim?.SetTrigger("dieTrigger"); // ��� ���
        }
        return;
    }
    #endregion
}
