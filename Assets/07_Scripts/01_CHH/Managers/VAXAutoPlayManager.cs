using Photon.Pun.Demo.Cockpit;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics.Internal;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// MainMenuScene Ÿ��Ʋ ���� ���� ������ ����
/// VAX ���� �뷫���� �ڵ� �÷���
/// </summary>
public class VAXAutoPlayManager : MonoBehaviour
{
    #region Enum

    /// <summary>
    /// VAX �ڵ� �÷��� ���� ����
    /// </summary>
    private enum VaxAutoPlayState
    {
        Init,                   // ��ü �ʱ�ȭ �۾�
        START,                  // ���ǿ� �÷��̾� ����� �Ϲ� ���� ����
        NORMAL_ZOMBIES_DEAD,    // �Ϲ� ���� ��� ������ ���Ǿ� ������ Girl2�� Man1 ����
        MAN1_DEAD,              // Man1�� ������ ������ ������ Girl1�� Man2�� ���Ǿ�(Girl2) ����
        MAFIA_DEAD,             // ���Ǿ� ������ Girl2�� ������ ���� ���� ����
        BOSS_ZOMBIE_DEAD,       // ���� ���� ������ ���� ȹ��
        FIND_VAX,               // �������� �÷��̾� ��ġ �̵�, ������ �� ��� ã��
        RESCUE_REQEUST,         // ������� ��ġ �̵�, Girl1 ��ȣź �߻�
        FINAL_ZOMBIE,           // �Ϲ� ���� ���̺� �߻� �÷��̾�� ���� ����
        MAN2_DEAD,              // Man2�� ������ ��� ����
        RESCUE_SUCCESS,         // Girl1�� ���� �̵� �� Ż��
        END                     // VAX UI �߰� ����
    }

    /// <summary>
    /// �÷��̾� ���� - �ִϸ��̼ǿ� �ݿ�
    /// </summary>
    private enum PlayerState
    {
        IDLE,       // �⺻
        WALK,       // �ȱ�
        RUN,        // �ٱ�
        ATTACK,     // ����
        DAMAGE,     // ������ ����
        SURPRISED,  // ���
        THINKING,   // ������
        JOY,        // ���
        DIE         // ����
    }

    /// <summary>
    /// ���� ���� - �ִϸ��̼ǿ� �ݿ�
    /// </summary>
    private enum ZombieState
    {
        IDLE,       // �⺻
        WALK,       // �ȱ�
        RUN,        // �ٱ�
        ATTACK,     // ����
        DAMAGE,     // ������ ����
        DIE         // ����
    }

    #endregion Enum

    #region Variable

    private VaxAutoPlayState vaxAutoPlayState;                      // VAX �ڵ� �÷��� ���� ����

    [SerializeField] private GameObject mainCamera;                 // ����ī�޶� ������Ʈ
    [SerializeField] private Transform[] mainCameraPos;             // ����ī�޶� ��ġ[����, ����, ���]

    [SerializeField] private float runSpeed = 10f;

    [SerializeField] private GameObject hospitalKey;                // ���� ���͸� ���̸� ������ ���� Ű
    [SerializeField] private Transform hospitalKeyPos;              // ī�޶� ���� �̵��� �̵��� ���� Ű ��ġ
    [SerializeField] private Animator hospitalDoorAnim;             // ���� �� �ִϸ��̼�
    [SerializeField] private GameObject vax;                        // ��� ������Ʈ
    [SerializeField] private GameObject firework;                   // ��ȣź ������Ʈ

    private bool isPlayerInit;                                      // �÷��̾� �ʱ�ȭ �Լ� �Ϸ� ����

    private PlayerState[] playersState;                             // �÷��̾� ����
    [SerializeField] private GameObject[] classPlayers;             // ������3, ���Ǿ�1
    [SerializeField] private Transform[] playersClassPos;           // �÷��̾� ���� ��ġ
    [SerializeField] private GameObject[] classPlayerParentBloods;  // ���� �÷��̾� �� ��ƼŬ �θ� ��ü
    private GameObject[] classPlayerBloods;           // ���� �÷��̾� �� ��ƼŬ(Man1, Girl1, Girl2)
    private NavMeshAgent[] classPlayerAgent;                        // �÷��̾� ��ΰ�� AI ������Ʈ
    private Animator[] classPlayersAnimator;                        // �÷��̾� �ִϸ����� ������Ʈ
    [SerializeField] private ParticleSystem[] classPlayerRifles;    // �÷��̾� �� ��ƼŬ
    private bool isClassPlayerInit;                                 // ���� �÷��̾� �ʱ�ȭ ����

    [SerializeField] private GameObject[] hospitalPlayers;          // Man2(������), Girl1(������)
    [SerializeField] private Transform[] playersHospitalPos;        // �÷��̾� ���� ��ġ[Man2StartPos, Girl2StartPos, Man2TargetPos, Girl2TargetPos]
    private NavMeshAgent[] hospitalPlayerAgent;                     // �÷��̾� ��ΰ�� AI ������Ʈ
    private Animator[] hospitalPlayersAnimator;                     // �÷��̾� �ִϸ����� ������Ʈ
    [SerializeField] private ParticleSystem[] hospitalPlayerRifles; // �÷��̾� �� ��ƼŬ
    private bool isHospitalPlayerInit;                              // ���� �÷��̾� �ʱ�ȭ ����

    [SerializeField] private GameObject[] rescuePlayers;            // Man2(������), Girl1(������)
    [SerializeField] private Transform[] playersRescuePos;          // �÷��̾� Ż�� ��ġ[Man2StartPos, Girl2StartPos, Man2TargetPos, Girl2TargetPos]
    [SerializeField] private GameObject[] rescuePlayerParentBloods; // Ż�� �÷��̾� �� ��ƼŬ �θ� ��ü
    private GameObject[] rescuePlayerBloods;          // ���� �÷��̾� �� ��ƼŬ(Man1, Girl1)
    private NavMeshAgent[] rescuePlayerAgent;                       // �÷��̾� ��ΰ�� AI ������Ʈ
    private Animator[] rescuePlayersAnimator;                       // �÷��̾� �ִϸ����� ������Ʈ
    [SerializeField] private ParticleSystem[] rescuePlayerRifles;   // �÷��̾� �� ��ƼŬ
    private bool isRescuePlayerInit;                                // Ż�� �÷��̾� �ʱ�ȭ ����
    private bool isEscape;                                          // Ż�� ����

    [SerializeField] private GameObject rescueHelicopter;           // ���� �︮����
    [SerializeField] private GameObject rescueHelicopterObj;        // ���� �︮���� ��ü(�ϴÿ� �ִµ� Ÿ�ٿ� �����ϸ� ���� ����)
    private NavMeshAgent rescueHelicopterAgent;                     // ���� �︮���� AI ������Ʈ
    [SerializeField] private Transform rescueHelicopterPos;         // ���� �︮���� ���� ����Ʈ
    private bool isHellcopterArrive;                                // ���� �︮���� ���� ����

    private float playTime;                                         // �Ϲ� ���� ���� �ð�
    private ZombieState[] normalZombieState;                        // �Ϲ� ���� ����
    private NavMeshAgent[] normalZombieAgent;                       // �÷��̾� ��ΰ�� AI ������Ʈ
    private Animator[] normalZombieAnimator;                        // �÷��̾� �ִϸ����� ������Ʈ
    [SerializeField] private GameObject[] normalZombies;            // �Ϲ� ����
    [SerializeField] private Transform[] normalZombiesTargetPos;    // �Ϲ� ���� ��ġ
    [SerializeField] private GameObject[] normalZombieParentBloods; // �Ϲ� ���� �� ��ƼŬ �θ� ��ü
    private GameObject[] normalZombieBloods;          // �Ϲ� ���� �� ��ƼŬ
    private bool isNormalZombieInit;                                // �Ϲ� ���� �ʱ�ȭ ����

    private ZombieState bossZombieState;                            // ���� ���� ����
    private NavMeshAgent bossZombieAgent;                           // ���� ���� ��ΰ�� AI ������Ʈ
    private Animator bossZombieAnimator;                            // ���� ���� �ִϸ����� ������Ʈ
    [SerializeField] private GameObject bossZombie;                 // ���� ����
    [SerializeField] private Transform bossZombieTargetPos;         // ���� ���� ��ġ
    [SerializeField] private GameObject[] bossZombieParentBloods;   // ���� ���� �� ��ƼŬ �θ� ��ü
    private GameObject[] bossZombieBloods;            // ���� ���� �� ��ƼŬ 
    private bool isBossZombieDie;                                   // ���� ���� ���� ����
    private bool isBossZombieInit;                                  // ���� ���� �ʱ�ȭ ����

    private ZombieState[] waveZombieState;                          // ������̺� �� ����� ����
    private NavMeshAgent[] waveZombieAgent;                         // ������̺� �� ����� ��ΰ�� AI ������Ʈ
    private Animator[] waveZombieAnimator;                          // ������̺� �� ����� �ִϸ����� ������Ʈ
    [SerializeField] private GameObject[] waveZombies;              // ������̺� �� ������ �����
    [SerializeField] private Transform[] waveZombiesTargetPos;      // ������̺� �� ����� ��ġ
    [SerializeField] private GameObject[] waveZombieParentBloods;   // ������̺� �� ����� �� ��ƼŬ
    private GameObject[] waveZombieBloods;            // ������̺� �� �����  �� ��ƼŬ 
    private bool[] isWaveZombeDie;                                  // ���� ���� ���� Ȯ��
    private bool isWaveZombieInit;                                  // ������̺� �� ���� �ʱ�ȭ ����

    [SerializeField] GameObject endingPanel;                        // ���� UI

    private bool isEnd = false;

    #endregion Variable

    #region Unity Method

    /// <summary>
    /// ������Ʈ �ʱ�ȭ
    /// </summary>
    private void Awake()
    {
        vaxAutoPlayState = VaxAutoPlayState.Init;

        // �ʱ�ȭ �Ϸ� ���� false�� �ʱ�ȭ
        isPlayerInit = false;
        isClassPlayerInit = false;
        isHospitalPlayerInit = false;
        isRescuePlayerInit = false;
        isNormalZombieInit = false;
        isBossZombieInit = false;
        isWaveZombieInit = false;
    }

    private void Start()
    {
        isEnd = false;
    }

    private void Update()
    {
        if (isEnd == false)
        {
            // ���� ���� �÷���
            AutoPaly();

            // �÷��̾� ��ġ �̵� ���� üũ
            //PlayerPosChange();
        }
        else
        {
            if (endingPanel.activeSelf == false)
            {
                endingPanel.SetActive(true);
            }
        }
    }

    #endregion Unity Method

    #region Method

    /// <summary>
    ///  ��ü �ʱ�ȭ
    /// </summary>
    private void AllInit()
    {
        // �ϳ��� �ʱ�ȭ�� �ȵǸ� �� ������ �� ����
        while (true)
        {
            // �÷��̾� ��ġ, ���� �� �ʱ�ȭ �۾�
            PlayerInit();

            // �Ϲ� ���� ��ġ, ���� �� �ʱ�ȭ �۾�
            NormalZombieInit();

            // ���� ���� ��ġ, ���� �� �ʱ�ȭ �۾�
            BossZombieInit();

            // ���� ���̺� �� ���� ��ġ, ���� �� �ʱ�ȭ �۾�
            WaveZombieInit();

            if (!isPlayerInit || !isClassPlayerInit || !isHospitalPlayerInit || !isRescuePlayerInit
            || !isNormalZombieInit || !isBossZombieInit || !isWaveZombieInit)
            {
                continue;
            }
            else
            {
                vaxAutoPlayState = VaxAutoPlayState.START;
                break;
            }
        }
    }

    /// <summary>
    /// �÷��̾� ��ġ, ���� �� �ʱ�ȭ �۾�
    /// </summary>
    private void PlayerInit()
    {
        Debug.Log("PlayerInit ����");

        // VAX �ڵ� �÷��� ���� ���¸� �������� �ʱ�ȭ
        vaxAutoPlayState = VaxAutoPlayState.START;

        // ���� �÷��̾� �� ��ƼŬ �ʱ�ȭ
        classPlayerBloods = new GameObject[classPlayerParentBloods.Length];

        // ����ī�޶� ��ġ �б�-���Ƿ� �ʱ�ȭ
        mainCamera.transform.position = mainCameraPos[0].position;

        // �÷��̾� ���� �ʱ�ȭ
        playersState = new PlayerState[classPlayers.Length];

        // �÷��̾� ������Ʈ �ʱ�ȭ
        classPlayerAgent = new NavMeshAgent[classPlayers.Length - 1];
        hospitalPlayerAgent = new NavMeshAgent[hospitalPlayers.Length];
        rescuePlayerAgent = new NavMeshAgent[rescuePlayers.Length];

        // �÷��̾� �ִϸ����� �ʱ�ȭ
        classPlayersAnimator = new Animator[classPlayers.Length];
        hospitalPlayersAnimator = new Animator[hospitalPlayers.Length];
        rescuePlayersAnimator = new Animator[rescuePlayers.Length];

        // �÷��̾� ���°� �ʱ�ȭ
        playersState[0] = PlayerState.ATTACK;        // Man1 ���� ����
        playersState[1] = PlayerState.THINKING;      // Man2 ĥ�Ǿտ��� ������
        playersState[2] = PlayerState.ATTACK;        // Girl1 ���� ����
        playersState[3] = PlayerState.ATTACK;        // Girl2-Maria ���� ����

        // ���� �÷��̾� �ʱ�ȭ
        for (int i = 0; i < classPlayers.Length; i++)
        {
            // �÷��̾� ��ġ �б�-���Ƿ� �ʱ�ȭ
            classPlayers[i].transform.position = playersClassPos[i].position;

            // �� ��ƼŬ �ڽ� ������Ʈ ��������
            classPlayerBloods[i] = classPlayerParentBloods[i].transform.Find("Blood").gameObject;

            // �÷��̾� �ִϸ����� �ʱ�ȭ
            classPlayersAnimator[i] = classPlayers[i].GetComponent<Animator>();

            if (i == 1)
            {
                classPlayersAnimator[i].SetBool("IsThinking", true);
            }

            // Girl2 = ���Ǿƴ� �׺�޽� ����
            if (i != classPlayers.Length - 1)
            {
                // �÷��̾� ������Ʈ �ʱ�ȭ
                classPlayerAgent[i] = classPlayers[i].GetComponent<NavMeshAgent>();

                // �÷��̾� ������Ʈ ���ǵ� ����
                classPlayerAgent[i].speed = runSpeed;

                // �÷��̾� ������ ����
                classPlayerAgent[i].isStopped = true;
            }

            // ���� �÷��̾� ���� �Ϸ� �Ǿ��� �� ����
            if (i == classPlayers.Length - 1)
            {
                isClassPlayerInit = true;
            }
        }

        // ���� �÷��̾� �ʱ�ȭ
        for (int i = 0; i < hospitalPlayers.Length; i++)
        {
            // �÷��̾� ��ġ �������� �ʱ�ȭ
            //hospitalPlayers[i].transform.position = playersHospitalPos[i].position;

            // �÷��̾� �ִϸ����� �ʱ�ȭ
            hospitalPlayersAnimator[i] = hospitalPlayers[i].GetComponent<Animator>();

            // �÷��̾� �ȱ�
            hospitalPlayersAnimator[i].SetBool("IsMove", true);

            // �÷��̾� ������Ʈ �ʱ�ȭ
            hospitalPlayerAgent[i] = hospitalPlayers[i].GetComponent<NavMeshAgent>();
            hospitalPlayerAgent[i].SetDestination(playersHospitalPos[i].position);

            // �÷��̾� ������Ʈ ���ǵ� ����
            hospitalPlayerAgent[i].speed = runSpeed * 4;

            // �÷��̾� ������ ����
            hospitalPlayerAgent[i].isStopped = true;

            Debug.Log("�ʱ�ȭ Hospital Player Count : " + i);

            // ���� �÷��̾� ���� �Ϸ� �Ǿ��� �� ����
            if (i == hospitalPlayers.Length - 1)
            {
                isHospitalPlayerInit = true;
            }
        }

        // ���� �� �ʱ�ȭ
        hospitalDoorAnim.SetBool("Door", true);



        // Ż�� �÷��̾� �� ��ƼŬ �ʱ�ȭ
        rescuePlayerBloods = new GameObject[rescuePlayerParentBloods.Length];

        // �� ��ƼŬ �ڽ� ������Ʈ ��������
        for (int i = 0; i < rescuePlayerParentBloods.Length; i++)
        {
            rescuePlayerBloods[i] = rescuePlayerParentBloods[i].transform.Find("Blood").gameObject;
        }

        // �б� ��� Ż�� �÷��̾� �ʱ�ȭ
        for (int i = 0; i < rescuePlayers.Length; i++)
        {
            // �÷��̾� �ִϸ����� �ʱ�ȭ
            rescuePlayersAnimator[i] = rescuePlayers[i].GetComponent<Animator>();

            // �÷��̾� ������Ʈ �ʱ�ȭ
            rescuePlayerAgent[i] = rescuePlayers[i].GetComponent<NavMeshAgent>();
            rescuePlayerAgent[i].SetDestination(playersRescuePos[i].position);

            // �÷��̾� ������ ����
            rescuePlayerAgent[i].isStopped = true;

            // �÷��̾� ������Ʈ ���ǵ� ����
            rescuePlayerAgent[i].speed = runSpeed * 7.7f;

            // Ż�� �÷��̾� ���� �Ϸ� �Ǿ��� �� ����
            if (i == rescuePlayers.Length - 1)
            {
                isRescuePlayerInit = true;
            }
        }

        rescueHelicopterAgent = rescueHelicopter.GetComponent<NavMeshAgent>();
        isHellcopterArrive = false;

        isEscape = false;
        isPlayerInit = true;
    }

    /// <summary>
    /// �Ϲ� ���� ��ġ, ���� �� �ʱ�ȭ �۾�
    /// </summary>
    private void NormalZombieInit()
    {
        // �Ϲ� ���� ���� �ʱ�ȭ
        normalZombieState = new ZombieState[normalZombies.Length];

        // �÷��̾� ������Ʈ �ʱ�ȭ
        normalZombieAgent = new NavMeshAgent[normalZombies.Length];

        // �÷��̾� �ִϸ����� �ʱ�ȭ
        normalZombieAnimator = new Animator[normalZombies.Length];

        // �Ϲ� ���� �� ��ƼŬ �ʱ�ȭ
        normalZombieBloods = new GameObject[normalZombieParentBloods.Length];

        // �Ϲ� ���� �ʱ�ȭ
        for (int i = 0; i < normalZombies.Length; i++)
        {
            // �Ϲ� ���� ���°� �ʱ�ȭ
            normalZombieState[i] = ZombieState.IDLE;

            // �� ��ƼŬ �ڽ� ������Ʈ ��������
            normalZombieBloods[i] = normalZombieParentBloods[i].transform.Find("Blood").gameObject;

            // �Ϲ� ���� ������Ʈ �ʱ�ȭ
            normalZombieAgent[i] = normalZombies[i].GetComponent<NavMeshAgent>();

            // �Ϲ� ���� ������Ʈ ���ǵ� ����
            normalZombieAgent[i].speed = runSpeed;

            // �������� ���ϰ� ����
            normalZombieAgent[i].isStopped = true;

            // �Ϲ� ���� Ÿ�� ��ġ �ʱ�ȭ
            normalZombieAgent[i].SetDestination(normalZombiesTargetPos[i].position);

            // �Ϲ� ���� �ִϸ����� �ʱ�ȭ
            normalZombieAnimator[i] = normalZombies[i].GetComponent<Animator>();

            normalZombieAnimator[i].SetInteger("RandomWalk", Random.Range(0, 8));


            // �Ϲ� ���� ���� �Ϸ� �Ǿ��� �� ����
            if (i == normalZombies.Length - 1)
            {
                isNormalZombieInit = true;
            }
        }

        normalZombieAgent[0].isStopped = false;  // normalZombieAgent[0] ������ �̵� ����
        normalZombieAgent[1].isStopped = false;  // normalZombieAgent[1] ������ �̵� ����
    }

    /// <summary>
    /// ���� ���� ��ġ, ���� �� �ʱ�ȭ �۾�
    /// </summary>
    private void BossZombieInit()
    {
        // ���� ���� ���� ����
        isBossZombieDie = false;

        // ���� ���� ���� �ʱ�ȭ
        bossZombieState = ZombieState.IDLE;

        // ���� ���� �� ��ƼŬ �ʱ�ȭ
        bossZombieBloods = new GameObject[bossZombieParentBloods.Length];
        bossZombieBloods[0] = bossZombieParentBloods[0].transform.Find("Blood").gameObject;
        bossZombieBloods[1] = bossZombieParentBloods[1].transform.Find("Blood").gameObject;

        // ���� ���� ������Ʈ �ʱ�ȭ
        bossZombieAgent = bossZombie.GetComponent<NavMeshAgent>();

        // ���Ǿ� �÷��̾ ������ �̵� ����
        bossZombieAgent.isStopped = true;

        // ���� ���� Ÿ����ġ �ʱ�ȭ
        bossZombieAgent.SetDestination(bossZombieTargetPos.position);

        // �÷��̾� ������Ʈ ���ǵ� ����
        bossZombieAgent.speed = runSpeed * 4;

        // ���� ���� �ִϸ����� �ʱ�ȭ
        bossZombieAnimator = bossZombie.GetComponent<Animator>();

        // ���� ���� ���� �Ϸ� �Ǿ��� �� ����
        isBossZombieInit = true;
    }

    /// <summary>
    /// ���� ���̺� �� ������ġ, ���� �� �ʱ�ȭ �۾�
    /// </summary>
    private void WaveZombieInit()
    {

        // ������̺� ���� ���� �ʱ�ȭ
        waveZombieState = new ZombieState[waveZombies.Length];

        // ������̺� �� ��ƼŬ �ʱ�ȭ
        waveZombieBloods = new GameObject[waveZombieParentBloods.Length];

        // ���� ���� ���� �ʱ�ȭ
        isWaveZombeDie = new bool[waveZombies.Length];

        // ������̺� ����  ������Ʈ �ʱ�ȭ
        waveZombieAgent = new NavMeshAgent[waveZombies.Length];

        // ������̺� ����  �ִϸ����� �ʱ�ȭ
        waveZombieAnimator = new Animator[waveZombies.Length];

        // ������̺� ���� �ʱ�ȭ
        for (int i = 0; i < waveZombies.Length; i++)
        {
            // ������̺� ���°� �ʱ�ȭ
            waveZombieState[i] = ZombieState.IDLE;

            // �� ��ƼŬ �ڽ� ������Ʈ ��������
            waveZombieBloods[i] = waveZombieParentBloods[i].transform.Find("Blood").gameObject;

            // ������̺� ������Ʈ �ʱ�ȭ
            waveZombieAgent[i] = waveZombies[i].GetComponent<NavMeshAgent>();

            // ������̺� ������Ʈ ���ǵ� ����
            waveZombieAgent[i].speed = runSpeed;

            // ������̺� ������Ʈ �̵� ���ϰ� ����
            waveZombieAgent[i].isStopped = true;

            // ������̺� ��ġ �ʱ�ȭ
            waveZombieAgent[i].SetDestination(waveZombiesTargetPos[i].position);

            // ������̺� �ִϸ����� �ʱ�ȭ
            waveZombieAnimator[i] = waveZombies[i].GetComponent<Animator>();

            waveZombieAnimator[i].SetInteger("RandomWalk", Random.Range(0, 8));

            // ������̺� ���� �Ϸ� �Ǿ��� �� ����
            if (i == waveZombies.Length - 1)
            {
                isWaveZombieInit = true;
            }
        }
    }

    /// <summary>
    /// ���� ���൵�� ���� �׺�޽� Ÿ�� ����
    /// </summary>
    private void AutoPaly()
    {
        if (vaxAutoPlayState == VaxAutoPlayState.Init)
        {
            AllInit();
        }
        // ���� ���� : �Ϲ����� ���̱�
        if (vaxAutoPlayState == VaxAutoPlayState.START)
        {
            // ���� ���� �ð�
            playTime += Time.deltaTime;

            // �Ϲ� ���� �� ��ŭ �ݺ�
            for (int i = 0; i < normalZombies.Length; i++)
            {
                // agent.remainingDistance : ���� agent ��ġ�� ���ϴ� ��ġ�� ���̰Ÿ� ��
                // agent.stoppingDistance : �������� �Ÿ�
                // ������ ��ġ�� �����ϸ� ���� �ִϸ��̼�
                if (normalZombieAgent[i].remainingDistance <= normalZombieAgent[i].stoppingDistance
                    && normalZombieAnimator[i].GetBool("IsDie") == false)
                {
                    normalZombieAnimator[i].SetBool("IsAttack", true);
                }
            }

            // �÷���Ÿ�� ����
            if (playTime > 4 && playTime < 5)
            {
                // ���� �÷��̾� �� ��ŭ �ݺ�
                for (int i = 0; i < classPlayers.Length; i++)
                {
                    // Man2(������) �÷��̾� �����ϰ� ���� �ִϸ��̼� Ȱ��ȭ
                    if (classPlayersAnimator[i].GetBool("IsAttack") == false && i != 1
                        && normalZombieAnimator[2].GetBool("IsDie") == false)
                    {
                        classPlayersAnimator[i].SetBool("IsAttack", true);
                    }
                }

                // ���� ��ƼŬ Ȱ��ȭ
                if (classPlayerRifles[0].isPlaying == false)
                {
                    classPlayerRifles[0].Play();
                    classPlayerRifles[2].Play();

                    normalZombieBloods[0].SetActive(true);
                    normalZombieBloods[1].SetActive(true);
                }
            }
            else if (playTime >= 7 && playTime < 13)
            {
                // �Ϲ� ����1 �ִϸ��̼� ���� ó��
                if (normalZombieAnimator[0].GetBool("IsDie") == false)
                {
                    normalZombieAnimator[0].SetBool("IsDie", true);
                    normalZombieBloods[0].GetComponent<BloodParticleReactivator>().Stop();
                }

                // ����3 ���
                normalZombieAgent[2].isStopped = false;
                normalZombieBloods[2].SetActive(true);
            }
            else if (playTime >= 16 && playTime < 16.5)
            {
                // ����2 �ִϸ��̼� ���� ó��
                if (normalZombieAnimator[1].GetBool("IsDie") == false)
                {
                    normalZombieAnimator[1].SetBool("IsDie", true); 
                    normalZombieBloods[1].GetComponent<BloodParticleReactivator>().Stop();
                }

                // ����4 ���
                normalZombieAgent[3].isStopped = false;
                normalZombieBloods[3].SetActive(true);
            }
            else if (playTime >= 17 && playTime < 22)
            {
                // ����3 �ִϸ��̼� ���� ó��
                if (normalZombieAnimator[2].GetBool("IsDie") == false)
                {
                    normalZombieAnimator[2].SetBool("IsDie", true);
                    normalZombieBloods[2].GetComponent<BloodParticleReactivator>().Stop();

                    // ����3 �Ĵٺ����� ����
                    classPlayers[0].transform.LookAt(normalZombieAnimator[3].transform);
                    classPlayers[3].transform.LookAt(normalZombieAnimator[3].transform);
                }
            }
            else if (playTime >= 25)
            {
                // ����4 �ִϸ��̼� ���� ó��
                if (normalZombieAnimator[3].GetBool("IsDie") == false)
                {
                    // Man1(������), Girl1(������), Girl2(���Ǿ�) ���� �ִϸ��̼� ��Ȱ��ȭ
                    classPlayersAnimator[0].SetBool("IsAttack", false);
                    classPlayersAnimator[2].SetBool("IsAttack", false);
                    classPlayersAnimator[3].SetBool("IsAttack", false);

                    // ���� �÷��̾� ���� ��ƼŬ ����
                    classPlayerRifles[0].Stop();
                    classPlayerRifles[2].Stop();

                    normalZombieAnimator[3].SetBool("IsDie", true);
                    normalZombieBloods[3].GetComponent<BloodParticleReactivator>().Stop();

                    vaxAutoPlayState = VaxAutoPlayState.NORMAL_ZOMBIES_DEAD;
                    playTime = 0;
                }
            }
        }
        // �Ϲ� ���� ���� �� : Girl2(���ľ�)�� Man1(������) ����
        else if (vaxAutoPlayState == VaxAutoPlayState.NORMAL_ZOMBIES_DEAD)
        {
            // ���� ���� �ð�
            playTime += Time.deltaTime;

            // Man1 �����ڰ� ���� �ʾ��� �� ����
            if (classPlayersAnimator[0].GetBool("IsDie") == false)
            {
                // Grirl2(���Ǿ�)�� Man1 ������ ���� ����
                if (playTime > 2 && playTime < 3.5)
                {
                    // Girl2(���Ǿ�)�� ���� ��� Man1�� ������ ����
                    classPlayers[3].transform.LookAt(classPlayers[0].transform);

                    // Girl2(���Ǿ�)�� ���� �ִϸ��̼��� ���������� �ѱ�
                    if (classPlayersAnimator[3].GetBool("IsAttack") == false)
                    {
                        classPlayersAnimator[3].SetBool("IsAttack", true);

                        // Girl2(���Ǿ�) ���� ��ƼŬ ���
                        if (classPlayerRifles[2].isPlaying == false) classPlayerRifles[2].Play();

                        // Man1(������) �� ��ƼŬ Ȱ��ȭ
                        classPlayerBloods[0].SetActive(true);
                    }
                }
                // ���ݴ��� Man1�� Girl2(���Ǿ�) ����
                else if (playTime >= 3.5 && playTime < 6)
                {
                    // Man1(������)�� Girl2(���Ǿ�)�� ������ ����
                    classPlayers[0].transform.LookAt(classPlayers[3].transform);

                    // Man1(������)�� ���� �ִϸ��̼��� ���������� �ѱ�
                    if (classPlayersAnimator[0].GetBool("IsAttack") == false)
                    {
                        classPlayersAnimator[0].SetBool("IsAttack", true);

                        // Man1(������) ���� ��ƼŬ ���
                        if (classPlayerRifles[0].isPlaying == false) classPlayerRifles[0].Play();
                    }
                }

                // Girl2(���Ǿ�)�� Man1 �����ڸ� �����ϴ� ����� �ٸ� �����ڰ� ���� ���
                if (playTime >= 5.5f && playTime < 8)
                {
                    // Man2(������)�� Girl2(���Ǿ�)�� ������ ����
                    classPlayers[1].transform.LookAt(classPlayers[3].transform);
                    // Girl1(������)�� Girl2(���Ǿ�)�� ������ ����
                    classPlayers[2].transform.LookAt(classPlayers[3].transform);

                    // Man1(������) ������ �ִϸ��̼� ����, ��� �ִϸ��̼� ���
                    classPlayersAnimator[1].SetBool("IsThinking", false);
                    classPlayersAnimator[1].SetBool("IsSurprised", true);

                    // Girl1(������) ��� �ִϸ��̼� ���
                    classPlayersAnimator[2].SetBool("IsSurprised", true);
                }

                if (playTime >= 10)
                {
                    // Man1 �������� ����
                    classPlayersAnimator[0].SetBool("IsDie", true);

                    // Main1(������) �� ��ƼŬ ����
                    classPlayerBloods[0].SetActive(false);
                    classPlayerBloods[0].GetComponent<BloodParticleReactivator>().Stop();

                    classPlayerRifles[0].Stop();

                    // Girl2(���Ǿ�)�� ���� ��� Girl1 ������ ����
                    classPlayers[3].transform.LookAt(classPlayers[2].transform);

                    // Girl2(���Ǿ�) ��� ���� ����
                    classPlayersAnimator[3].SetBool("IsAttack", false);

                    // Girl2(���Ǿ�) ���� ��ƼŬ ��Ȱ��ȭ
                    classPlayerRifles[2].Stop();
                }
            }
            // Man1(������)�� �׾��� �� ����
            else
            {
                if (playTime >= 11 && playTime < 16)
                {
                    // Man2(������) ��� �ִϸ��̼� ��Ȱ��ȭ, ���� �ִϸ��̼� Ȱ��ȭ
                    classPlayersAnimator[1].SetBool("IsSurprised", false);
                    classPlayersAnimator[1].SetBool("IsAttack", true);

                    // Man2(������) ���� ��ƼŬ Ȱ��ȭ
                    if (classPlayerRifles[1].isPlaying == false)
                    {
                        classPlayerRifles[1].Play();

                        // Girl2(���Ǿ�) �� ��ƼŬ Ȱ��ȭ
                        classPlayerBloods[3].SetActive(true);
                    }

                    // Girl1(������) ��� �ִϸ��̼� ��Ȱ��ȭ, ���� �ִϸ��̼� Ȱ��ȭ
                    classPlayersAnimator[2].SetBool("IsSurprised", false);
                    classPlayersAnimator[2].SetBool("IsAttack", true);

                    // Girl2(���Ǿ�) ���� �ִϸ��̼� Ȱ��ȭ
                    classPlayersAnimator[3].SetBool("IsAttack", true);

                    // Girl2(���Ǿ�) ���� ��ƼŬ Ȱ��ȭ
                    if (classPlayerRifles[2].isPlaying == false)
                    {
                        classPlayerRifles[2].Play();

                        // Girl1(������) �� ��ƼŬ Ȱ��ȭ
                        classPlayerBloods[2].SetActive(true);
                    }
                }
                else if (playTime >= 17)
                {
                    // �׺�޽� Ÿ�� Girl2(���Ǿ�) ����
                    classPlayerAgent[2].SetDestination(classPlayers[3].transform.position);
                    // �׺�޽� �߰� ����
                    classPlayerAgent[2].isStopped = false;

                    // �������� ���� ����
                    vaxAutoPlayState = VaxAutoPlayState.MAN1_DEAD;

                    // �÷��� Ÿ�� �ʱ�ȭ
                    playTime = 0;
                }
            }
        }
        // Man1(������) ���� �� ���� �����ڵ��� Girl2(���Ǿ�) ����
        else if (vaxAutoPlayState == VaxAutoPlayState.MAN1_DEAD)
        {
            // ���� ���� �ð�
            playTime += Time.deltaTime;

            // Girl2(���Ǿ�)�� ���� �ʾ��� �� ����
            if (classPlayersAnimator[3].GetBool("IsDie") == false)
            {
                if (playTime > 4)
                {
                    // Girl2(���Ǿ�) ���� ó��
                    classPlayersAnimator[3].SetBool("IsDie", true);
                    // ���� ��ƼŬ ��Ȱ��ȭ
                    classPlayerRifles[2].Stop();

                    // Man2(������), Girl1(������) ���� ���� �ٶ󺸵��� ����
                    classPlayers[1].transform.LookAt(bossZombie.transform);
                    classPlayers[2].transform.LookAt(bossZombie.transform);

                    // Man2(������), Girl1(������) ���� �ִϸ��̼� ��Ȱ��ȭ
                    classPlayersAnimator[1].SetBool("IsAttack", false);
                    classPlayersAnimator[2].SetBool("IsAttack", false);

                    // ���� ��ƼŬ ��Ȱ��ȭ
                    classPlayerRifles[1].Stop();

                    // Girl1(������), Girl2(���Ǿ�) �� ��ƼŬ ����
                    classPlayerBloods[2].GetComponent<BloodParticleReactivator>().Stop();
                    classPlayerBloods[2].SetActive(false);
                    classPlayerBloods[3].GetComponent<BloodParticleReactivator>().Stop();
                    classPlayerBloods[3].SetActive(false);
                }
            }
            // Girl2(���Ǿ�)�� �׾��� �� ����
            else
            {
                if (playTime >= 6.5f && playTime < 8f)
                {
                    // Man2(������), Girl1(������) ��� �ִϸ��̼� ����
                    classPlayersAnimator[1].SetBool("IsSurprised", true);
                    classPlayersAnimator[2].SetBool("IsSurprised", true);
                }
                else if (playTime >= 8)
                {
                    // Man2(������), Girl1(������) ��� �ִϸ��̼� ��Ȱ��ȭ
                    classPlayersAnimator[1].SetBool("IsSurprised", false);
                    classPlayersAnimator[2].SetBool("IsSurprised", false);

                    // ���� ���� ���� ���Ǿ� ���� �ķ� ����
                    vaxAutoPlayState = VaxAutoPlayState.MAFIA_DEAD;

                    // ���� ���� ���
                    bossZombieAgent.isStopped = false;

                    // �÷���Ÿ�� �ʱ�ȭ
                    playTime = 0;
                }
            }
        }
        // Girl2(���Ǿ�) ���� �� ������ ���� ���� ���̱�
        else if (vaxAutoPlayState == VaxAutoPlayState.MAFIA_DEAD)
        {
            //Debug.Log("MAN1_DEAD ���� ���� ����");

            // ���� ���� �ð�
            playTime += Time.deltaTime;

            // agent.remainingDistance : ���� agent ��ġ�� ���ϴ� ��ġ�� ���̰Ÿ� ��
            // agent.stoppingDistance : �������� �Ÿ�
            // ������ ��ġ�� �����ϸ� ���� �ִϸ��̼�
            if (bossZombieAgent.remainingDistance <= bossZombieAgent.stoppingDistance
                && isBossZombieDie == false)
            {
                // ���� ���� ���� �ִϸ��̼� Ȱ��ȭ
                bossZombieAnimator.SetBool("IsAttack", true);

                // Man2(������), Girl1(������) ���� ���񿡰� ���� ����
                classPlayersAnimator[1].SetBool("IsAttack", true);
                classPlayersAnimator[2].SetBool("IsAttack", true);

                // Man2(������) ���� ��ƼŬ Ȱ��ȭ
                if (classPlayerRifles[1].isPlaying == false) classPlayerRifles[1].Play();

                // Man2(������), Girl1(������), ���� ���� �� ��ƼŬ Ȱ��ȭ
                classPlayerBloods[1].SetActive(true);
                bossZombieBloods[0].SetActive(true);
                bossZombieBloods[1].SetActive(true);

                // Girl1(������) ���� ���� �׺�޽��� Ÿ������ ����
                classPlayerAgent[2].SetDestination(bossZombie.transform.position);

                // Girl1(������) �߰� ����
                if (classPlayerAgent[2].remainingDistance > 0.2f)
                {
                    classPlayerAgent[2].isStopped = true;
                }
            }

            // ���� ���� ���� �ʾ��� �� ����
            if (isBossZombieDie == false)
            {
                // Man2(������), Girl1(������) ���� ���� �ٶ󺸵��� ����
                classPlayers[1].transform.LookAt(bossZombie.transform);
                classPlayers[2].transform.LookAt(bossZombie.transform);

                // Girl1(������) ���� ���� �׺�޽��� Ÿ������ ����
                classPlayerAgent[2].SetDestination(bossZombie.transform.position);

                // Girl1(������) ���� ������� �Ÿ��� 0.5f �̸����� �������� ����
                if (classPlayerAgent[2].remainingDistance < 0.5f)
                {
                    classPlayerAgent[2].isStopped = false;
                }

                // �÷���Ÿ�� 18�� �̻� �Ǹ� ���� ���� ����
                if (playTime > 18)
                {
                    isBossZombieDie = true;

                    // Man2(������), Girl1(������), ���� ���� �� ��ƼŬ ��Ȱ��ȭ
                    classPlayerBloods[1].GetComponent<BloodParticleReactivator>().Stop();
                    classPlayerBloods[1].SetActive(false);
                    bossZombieBloods[0].GetComponent<BloodParticleReactivator>().Stop();
                    bossZombieBloods[0].SetActive(false);
                    bossZombieBloods[1].GetComponent<BloodParticleReactivator>().Stop();
                    bossZombieBloods[1].SetActive(false);
                }
            }
            // ���� ���� �׾��� �� ����
            else if (isBossZombieDie == true)
            {
                // ����Ű ��Ȱ��ȭ ������ �� ����
                if (hospitalKey.activeSelf == false && playTime < 22)
                {
                    // ���� ���� ���� �ִϸ��̼� Ȱ��ȭ
                    bossZombieAnimator.SetBool("IsDie", true);

                    // Man2(������), Girl1(������) ���� �ִϸ��̼� ��Ȱ��ȭ
                    classPlayersAnimator[1].SetBool("IsAttack", false);
                    classPlayersAnimator[2].SetBool("IsAttack", false);

                    // Man2(������) ���� ��ƼŬ ��Ȱ��ȭ
                    classPlayerRifles[1].Stop();

                    // ī�޶� ���� ������ ����
                    classPlayers[1].transform.LookAt(mainCamera.transform.position);
                    classPlayers[2].transform.LookAt(mainCamera.transform.position);

                    // Man2(������), Girl1(������) ��� �ִϸ��̼� Ȱ��ȭ
                    classPlayersAnimator[1].SetBool("IsJoy", true);
                    classPlayersAnimator[2].SetBool("IsJoy", true);

                    // ���� Ű Ȱ��ȭ
                    hospitalKey.SetActive(true);
                }
                // ����Ű Ȱ��ȭ �Ǿ��� �� ����
                else
                {
                    if (playTime > 25)
                    {
                        // ���� Ű ��Ȱ��ȭ
                        hospitalKey.SetActive(false);

                        // �б� ���� �÷��̾� ������Ʈ ����
                        for (int i = 0; i < classPlayers.Length; i++)
                        {
                            Destroy(classPlayers[i].gameObject, 2f);
                        }

                        // �б� �Ϲ� ���� ������Ʈ ����
                        for (int i = 0; i < normalZombies.Length; i++)
                        {
                            Destroy(normalZombies[i].gameObject, 2f);
                        }

                        // ���� ���� ����
                        Destroy(bossZombie.gameObject, 2f);

                        // ����ī�޶� �������� �̵�
                        mainCamera.transform.position = mainCameraPos[1].position;
                        mainCamera.transform.rotation = mainCameraPos[1].rotation;

                        // ���� ���� Ű ���� ��ġ�� �̵�(������ �뵵�� �����)
                        hospitalKey.transform.position = hospitalKeyPos.position;

                        // ���� �� ����
                        hospitalDoorAnim.SetBool("Door", false);

                        // ���� �÷��̾� Ÿ�� ���� �� �̵� ����
                        //hospitalPlayerAgent[0].SetDestination(playersHospitalPos[0].position);
                        //hospitalPlayerAgent[1].SetDestination(playersHospitalPos[1].position);
                        hospitalPlayerAgent[0].isStopped = false;
                        hospitalPlayerAgent[1].isStopped = false;

                        // ���� ���� ���� ����
                        vaxAutoPlayState = VaxAutoPlayState.BOSS_ZOMBIE_DEAD;

                        // ���� ���� �ѱ�� ���� �÷��� Ÿ�� ���� ����
                        playTime = 0;
                    }
                }
            }
        }
        // ���� ���� �׾����� ���� ����� ������ ���� ���� ��� ã�´�.
        else if (vaxAutoPlayState == VaxAutoPlayState.BOSS_ZOMBIE_DEAD)
        {
            // ���� ���� �ð�
            playTime += Time.deltaTime;

            // Girl1(������)�� Ÿ�� ����Ʈ�� ���� ���� �� ����
            if (hospitalPlayerAgent[1].remainingDistance <= hospitalPlayerAgent[1].stoppingDistance
                && vax.activeSelf == false && hospitalPlayersAnimator[0].GetBool("IsLookFor") == false)
            {
                // �÷��̾� ���߰� �ȱ� �ִϸ��̼� ��Ȱ��ȭ
                hospitalPlayerAgent[0].isStopped = true;
                hospitalPlayerAgent[1].isStopped = true;
                hospitalPlayersAnimator[0].SetBool("IsMove", false);
                hospitalPlayersAnimator[1].SetBool("IsMove", false);

                // ã�� �ִϸ��̼� Ȱ��ȭ
                hospitalPlayersAnimator[0].SetBool("IsLookFor", true);
                hospitalPlayersAnimator[1].SetBool("IsLookFor", true);
            }
            else
            {
                if (playTime >= 10 && playTime < 17)
                {
                    vax.SetActive(true);

                    // ī�޶� ���� ������ ����
                    hospitalPlayers[0].transform.LookAt(mainCamera.transform.position);
                    hospitalPlayers[1].transform.LookAt(mainCamera.transform.position);

                    // ã�� �ִϸ��̼� ��Ȱ��ȭ�ǰ� �ڵ����� ��� �ִϸ��̼� ����
                    hospitalPlayersAnimator[0].SetBool("IsLookFor", false);
                    hospitalPlayersAnimator[1].SetBool("IsLookFor", false);
                    hospitalPlayersAnimator[0].SetBool("IsJoy", true);
                    hospitalPlayersAnimator[1].SetBool("IsJoy", true);

                    // Ż�� �÷��̾� ��� �ִϸ��̼� Ȱ��ȭ
                    /*rescuePlayersAnimator[0].SetBool("IsJoy", true);
                    rescuePlayersAnimator[1].SetBool("IsJoy", true);*/
                }
                else if (playTime > 17)
                {
                    // ���� ���� ������Ʈ ����
                    Destroy(hospitalPlayers[0].gameObject, 2f);
                    Destroy(hospitalPlayers[1].gameObject, 2f);
                    Destroy(hospitalKey.gameObject, 2f);
                    Destroy(vax.gameObject, 2f);

                    // ī�޶� ��ġ �б�-��� �̵�
                    mainCamera.transform.position = mainCameraPos[2].position;
                    mainCamera.transform.rotation = mainCameraPos[2].rotation;

                    // ��ȣź ������Ʈ Ȱ��ȭ
                    firework.SetActive(true);

                    // ���� ���̺� ����
                    for (int i = 0; i < waveZombies.Length; i++)
                    {
                        waveZombieAgent[i].isStopped = false;
                    }

                    // ���� ������� ����
                    vaxAutoPlayState = VaxAutoPlayState.RESCUE_REQEUST;

                    // �÷��� Ÿ�� �ʱ�ȭ
                    playTime = 0;
                }
            }

        }
        // �б�-��忡�� ��ȣź ��Ʈ�� �� ��Ⱑ �� ������ ��ƴԱ�
        else if (vaxAutoPlayState == VaxAutoPlayState.RESCUE_REQEUST)
        {
            // ���� ���� �ð�
            playTime += Time.deltaTime;

            // Ż�� �������� ��
            if(playTime > 5 && isEscape == true)
            {
                Debug.Log("Ż�� ���� ���� Ʈ���Ϸ� ����");
                isEnd= true;
            }

            // ���� ���̺��� ���� �� ��ŭ �ݺ�
            for (int i = 0; i < waveZombies.Length; i++)
            {
                // �ش� ���� ����ְ� Ÿ�� ����Ʈ�� �����ϸ� ����
                if (waveZombieAgent[i].remainingDistance <= waveZombieAgent[i].stoppingDistance
                && waveZombieAnimator[i].GetBool("IsDie") == false)
                {
                    // ���� �ִϸ��̼��� �ƴ� �� Ȱ��ȭ�� ����
                    if (waveZombieAnimator[i].GetBool("IsAttack") == false)
                    {
                        waveZombieAnimator[i].SetBool("IsAttack", true);
                    }
                    waveZombieAgent[i].isStopped = true;
                }

                // ���� ���� ���� üũ
                if (isWaveZombeDie[i] == true)
                {
                    // ���� �ִϸ��̼��� �ƴ� �� Ȱ��ȭ
                    if (waveZombieAnimator[i].GetBool("IsDie") == false)
                    {
                        waveZombieAnimator[i].SetBool("IsDie", true);
                        waveZombieBloods[i].SetActive(false);
                        waveZombieBloods[i].GetComponent<BloodParticleReactivator>().Stop();
                    }
                }

                // Man2(������)�� ������ ��ü ���� 
                if(rescuePlayersAnimator[0].GetBool("IsDie") == true)
                {
                    // ���� ���� ���� üũ
                    if (isWaveZombeDie[i] == false)
                    {
                        waveZombieBloods[i].SetActive(false);
                        waveZombieBloods[i].GetComponent<BloodParticleReactivator>().Stop();
                    }
                }
            }

            // Man2(������)�� ���� �ʾ��� �� ����
            if (rescuePlayersAnimator[0].GetBool("IsDie") == false)
            {
                if (playTime > 3 && playTime < 5)
                {
                    // ī�޶� ���� ������ ����
                    rescuePlayers[0].transform.LookAt(mainCamera.transform.position);
                    rescuePlayers[1].transform.LookAt(mainCamera.transform.position);

                    rescuePlayersAnimator[0].SetBool("IsJoy", true);
                    rescuePlayersAnimator[1].SetBool("IsJoy", true);
                }
                else if (playTime > 5 && playTime < 6)
                {
                    rescueHelicopter.SetActive(true);

                    // ���� �︮���� �z��
                    rescueHelicopterAgent.SetDestination(rescueHelicopterPos.position);
                    rescueHelicopterAgent.isStopped = false;

                    // �÷��̾� ��� �Ա� ���� ������ ���
                    rescuePlayerAgent[0].isStopped = false;
                    rescuePlayerAgent[1].isStopped = false;

                    // ���� �ִϸ��̼� ��Ȱ��ȭ, �̵� �ִϸ��̼� Ȱ��ȭ
                    rescuePlayersAnimator[0].SetBool("IsJoy", false);
                    rescuePlayersAnimator[1].SetBool("IsJoy", false);
                    rescuePlayersAnimator[0].SetBool("IsMove", true);
                    rescuePlayersAnimator[1].SetBool("IsMove", true);

                    // ���� ���ǵ� ������ ����
                    for (int i = 0; i < waveZombies.Length; i++)
                    {
                        waveZombieAgent[i].speed = runSpeed * 2;
                    }
                }
                // Man2(������)�� ��� �Ա� ���� ������ ���� �ϸ� ���� �ִϸ��̼� Ȱ��ȭ
                if (rescuePlayerAgent[0].remainingDistance <= rescuePlayerAgent[0].stoppingDistance)
                {
                    // ���� �ִϸ��̼��� Ȱ��ȭ �ȵǾ��� ���� ����
                    if (rescuePlayersAnimator[0].GetBool("IsAttack") == false)
                    {
                        // Man2(������) ���߰� ���� ����
                        rescuePlayerAgent[0].isStopped = true;
                        rescuePlayersAnimator[0].SetBool("IsMove", false);
                        rescuePlayersAnimator[0].SetBool("IsAttack", true);
                        rescuePlayerRifles[0].Play();
                        waveZombieBloods[1].SetActive(true);
                        waveZombieBloods[3].SetActive(true);
                        Debug.Log("Man1 ���� ����");

                        rescuePlayerBloods[0].SetActive(true);
                    }
                }

                // Girl1(������)�� ��� �Ա� ���� ������ ���� �ϸ� ���� �ִϸ��̼� Ȱ��ȭ
                if (rescuePlayerAgent[1].remainingDistance <= rescuePlayerAgent[1].stoppingDistance)
                {
                    // ���� �ִϸ��̼��� Ȱ��ȭ �ȵǾ��� ���� ����
                    if (rescuePlayersAnimator[1].GetBool("IsAttack") == false)
                    {
                        // Girl1(������) ���߰� ���� ����
                        //rescuePlayerAgent[1].isStopped = true;
                        rescuePlayersAnimator[1].SetBool("IsMove", false);
                        rescuePlayersAnimator[1].SetBool("IsAttack", true);
                        rescuePlayerBloods[1].SetActive(true);
                        waveZombieBloods[0].SetActive(true);
                        waveZombieBloods[2].SetActive(true);
                        Debug.Log("Girl1 ���� ����");
                    }
                    rescuePlayers[1].transform.LookAt(waveZombiesTargetPos[1]);
                }

                // ���� 1 ����
                if (playTime >= 15 && playTime < 16)
                {
                    isWaveZombeDie[0] = true;
                    Debug.Log("���� 1 ����");
                    waveZombieBloods[4].SetActive(true);
                }
                // ���� 2 ����
                else if (playTime >= 18 && playTime < 19)
                {
                    isWaveZombeDie[1] = true;
                    Debug.Log("���� 2 ����");
                    waveZombieBloods[5].SetActive(true);
                }
                // ���� 3 ����
                else if (playTime >= 21 && playTime < 22)
                {
                    isWaveZombeDie[2] = true;
                    Debug.Log("����3 :����");
                    waveZombieBloods[6].SetActive(true);
                }
                // ���� 3 ����
                else if (playTime >= 25 && playTime < 26)
                {
                    isWaveZombeDie[3] = true;
                    Debug.Log("����4 ����");
                    waveZombieBloods[7].SetActive(true);
                }
                // ���� 5 ����
                else if (playTime >= 28 && playTime < 29)
                {
                    isWaveZombeDie[4] = true;
                    Debug.Log("����5 ����");
                }
                // Man1 ����
                else if (playTime >= 30)
                {
                    rescuePlayersAnimator[0].SetBool("IsDie", true);
                    rescuePlayerRifles[0].Stop();
                    rescuePlayerBloods[0].GetComponent<BloodParticleReactivator>().Stop();
                }
            }
            else
            {
                // ����ִ� ���� Ÿ�� Girl1�� ����
                for (int i = 0; i < isWaveZombeDie.Length; i++)
                {
                    if (isWaveZombeDie[i] == false && playTime > 34)
                    {
                        waveZombieAgent[i].SetDestination(playersRescuePos[2].position);
                        waveZombieAgent[i].isStopped = false;
                    }
                }

                // ���� �︮ ���Ͱ� �������� �� ����
                if (rescueHelicopterAgent.remainingDistance <= rescueHelicopterAgent.stoppingDistance
                    && isHellcopterArrive == false)
                {
                    // Girl1(������) �̵� ������ Ż�� ����Ʈ�� ����
                    rescuePlayerAgent[1].SetDestination(playersRescuePos[2].position);
                    rescuePlayers[1].transform.LookAt(playersRescuePos[2]);
                    // �̵� ����
                    rescuePlayerAgent[1].isStopped = false;
                    // �̵� �ִϸ��̼� Ȱ��ȭ
                    if (rescuePlayersAnimator[1].GetBool("IsMove") == false)
                    {
                        // Girl1(������) �� ��ƼŬ ��Ȱ��ȭ
                        rescuePlayerBloods[1].GetComponent<BloodParticleReactivator>().Stop();

                        rescuePlayersAnimator[1].SetBool("IsAttack", false);
                        rescuePlayersAnimator[1].SetBool("IsMove", true);
                    }

                    // �︮ ���� ���� ����
                    if(rescueHelicopterObj.transform.position.y > 0)
                    {
                        rescueHelicopterObj.transform.position = new Vector3(rescueHelicopterObj.transform.position.x, rescueHelicopterObj.transform.position.y -0.1f, rescueHelicopterObj.transform.position.z);
                    }
                    // �︮ ���� ���� �Ϸ�
                    else
                    {
                        Debug.Log("�︮���� ����");
                        isHellcopterArrive = true;
                    }    
                }

                // Girl1(������)�� Ż�� ����Ʈ�� ���� ���� �� ����
                if (isHellcopterArrive == true && isEscape == false
                    && rescuePlayerAgent[1].remainingDistance <= rescuePlayerAgent[1].stoppingDistance)
                {
                    Debug.Log("Girl1 ��� ����");

                    rescuePlayerAgent[1].isStopped = true;
                    rescuePlayersAnimator[1].SetBool("IsMove", false);
                    rescuePlayersAnimator[1].SetBool("IsJoy", true);

                    // ī�޶� ���� ������ ����
                    rescuePlayers[1].transform.LookAt(mainCamera.transform.position);
                    isEscape = true;
                    playTime = 0;
                }
            }
        }
    }

    #endregion Method
}