using Photon.Pun.Demo.Cockpit;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics.Internal;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// MainMenuScene Å¸ÀÌÆ² ºñµğ¿À ¿µ»ó Á¦ÀÛÀ» À§ÇÑ
/// VAX °ÔÀÓ ´ë·«ÀûÀÌ ÀÚµ¿ ÇÃ·¹ÀÌ
/// </summary>
public class VAXAutoPlayManager : MonoBehaviour
{
    #region Enum

    /// <summary>
    /// VAX ÀÚµ¿ ÇÃ·¹ÀÌ ÁøÇà »óÅÂ
    /// </summary>
    private enum VaxAutoPlayState
    {
        Init,                   // ÀüÃ¼ ÃÊ±âÈ­ ÀÛ¾÷
        START,                  // ±³½Ç¿¡ ÇÃ·¹ÀÌ¾î ÀÔÀå°ú ÀÏ¹İ Á»ºñ µîÀå
        NORMAL_ZOMBIES_DEAD,    // ÀÏ¹İ Á»ºñ°¡ ¸ğµÎ Á×À¸¸é ¸¶ÇÇ¾Æ ¿ªÇÒÀÎ Girl2°¡ Man1 °ø°İ
        MAN1_DEAD,              // Man1ÀÌ Á×À¸¸é »ıÁ¸ÀÚ ¿ªÇÒÀÎ Girl1°ú Man2°¡ ¸¶ÇÇ¾Æ(Girl2) °ø°İ
        MAFIA_DEAD,             // ¸¶ÇÇ¾Æ ¿ªÇÒÀÎ Girl2°¡ Á×À¸¸é º¸½º Á»ºñ µîÀå
        BOSS_ZOMBIE_DEAD,       // º¸½º Á»ºñ°¡ Á×À¸¸é ¿­¼è È¹µæ
        FIND_VAX,               // º´¿øÀ¸·Î ÇÃ·¹ÀÌ¾î À§Ä¡ ÀÌµ¿, ¹®¿­°í µé¾î°¨ ¹é½Å Ã£À½
        RESCUE_REQEUST,         // ¿îµ¿ÀåÀ¸·Î À§Ä¡ ÀÌµ¿, Girl1 ½ÅÈ£Åº ¹ß»ç
        FINAL_ZOMBIE,           // ÀÏ¹İ Á»ºñ ¿şÀÌºê ¹ß»ı ÇÃ·¹ÀÌ¾î´Â Á»ºñ °ø°İ
        MAN2_DEAD,              // Man2°¡ Á×À¸¸é Çï±â µµÂø
        RESCUE_SUCCESS,         // Girl1°¡ Çï±â·Î ÀÌµ¿ ÈÄ Å»Ãâ
        END                     // VAX UI ¶ß°í Á¾·á
    }

    /// <summary>
    /// ÇÃ·¹ÀÌ¾î »óÅÂ - ¾Ö´Ï¸ŞÀÌ¼Ç¿¡ ¹İ¿µ
    /// </summary>
    private enum PlayerState
    {
        IDLE,       // ±âº»
        WALK,       // °È±â
        RUN,        // ¶Ù±â
        ATTACK,     // °ø°İ
        DAMAGE,     // µ¥¹ÌÁö ÀÔÀ½
        SURPRISED,  // ³î¶÷
        THINKING,   // »ı°¢Áß
        JOY,        // ±â»İ
        DIE         // Á×À½
    }

    /// <summary>
    /// Á»ºñ »óÅÂ - ¾Ö´Ï¸ŞÀÌ¼Ç¿¡ ¹İ¿µ
    /// </summary>
    private enum ZombieState
    {
        IDLE,       // ±âº»
        WALK,       // °È±â
        RUN,        // ¶Ù±â
        ATTACK,     // °ø°İ
        DAMAGE,     // µ¥¹ÌÁö ÀÔÀ½
        DIE         // Á×À½
    }

    #endregion Enum

    #region Variable

    private VaxAutoPlayState vaxAutoPlayState;                      // VAX ÀÚµ¿ ÇÃ·¹ÀÌ ÁøÇà »óÅÂ

    [SerializeField] private GameObject mainCamera;                 // ¸ŞÀÎÄ«¸Ş¶ó ¿ÀºêÁ§Æ®
    [SerializeField] private Transform[] mainCameraPos;             // ¸ŞÀÎÄ«¸Ş¶ó À§Ä¡[±³½Ç, º´¿ø, ¿îµ¿Àå]

    [SerializeField] private float runSpeed = 10f;

    [SerializeField] private GameObject hospitalKey;                // º¸½º ¸ó½ºÅÍ¸¦ Á×ÀÌ¸é ³ª¿À´Â º´¿ø Å°
    [SerializeField] private Transform hospitalKeyPos;              // Ä«¸Ş¶ó º´¿ø ÀÌµ¿ÈÄ ÀÌµ¿ÇÒ º´¿ø Å° À§Ä¡
    [SerializeField] private Animator hospitalDoorAnim;             // º´¿ø ¹® ¾Ö´Ï¸ŞÀÌ¼Ç
    [SerializeField] private GameObject vax;                        // ¹é½Å ¿ÀºêÁ§Æ®
    [SerializeField] private GameObject firework;                   // ½ÅÈ£Åº ¿ÀºêÁ§Æ®

    private bool isPlayerInit;                                      // ÇÃ·¹ÀÌ¾î ÃÊ±âÈ­ ÇÔ¼ö ¿Ï·á ¿©ºÎ

    private PlayerState[] playersState;                             // ÇÃ·¹ÀÌ¾î »óÅÂ
    [SerializeField] private GameObject[] classPlayers;             // »ıÁ¸ÀÚ3, ¸¶ÇÇ¾Æ1
    [SerializeField] private Transform[] playersClassPos;           // ÇÃ·¹ÀÌ¾î ±³½Ç À§Ä¡
    [SerializeField] private GameObject[] classPlayerParentBloods;  // ±³½Ç ÇÃ·¹ÀÌ¾î ÇÇ ÆÄÆ¼Å¬ ºÎ¸ğ °´Ã¼
    private GameObject[] classPlayerBloods;           // ±³½Ç ÇÃ·¹ÀÌ¾î ÇÇ ÆÄÆ¼Å¬(Man1, Girl1, Girl2)
    private NavMeshAgent[] classPlayerAgent;                        // ÇÃ·¹ÀÌ¾î °æ·Î°è»ê AI ¿¡ÀÌÀüÆ®
    private Animator[] classPlayersAnimator;                        // ÇÃ·¹ÀÌ¾î ¾Ö´Ï¸ŞÀÌÅÍ ÄÄÆ÷³ÍÆ®
    [SerializeField] private ParticleSystem[] classPlayerRifles;    // ÇÃ·¹ÀÌ¾î ÃÑ ÆÄÆ¼Å¬
    private bool isClassPlayerInit;                                 // ±³½Ç ÇÃ·¹ÀÌ¾î ÃÊ±âÈ­ ¿©ºÎ

    [SerializeField] private GameObject[] hospitalPlayers;          // Man2(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ)
    [SerializeField] private Transform[] playersHospitalPos;        // ÇÃ·¹ÀÌ¾î º´¿ø À§Ä¡[Man2StartPos, Girl2StartPos, Man2TargetPos, Girl2TargetPos]
    private NavMeshAgent[] hospitalPlayerAgent;                     // ÇÃ·¹ÀÌ¾î °æ·Î°è»ê AI ¿¡ÀÌÀüÆ®
    private Animator[] hospitalPlayersAnimator;                     // ÇÃ·¹ÀÌ¾î ¾Ö´Ï¸ŞÀÌÅÍ ÄÄÆ÷³ÍÆ®
    [SerializeField] private ParticleSystem[] hospitalPlayerRifles; // ÇÃ·¹ÀÌ¾î ÃÑ ÆÄÆ¼Å¬
    private bool isHospitalPlayerInit;                              // º´¿ø ÇÃ·¹ÀÌ¾î ÃÊ±âÈ­ ¿©ºÎ

    [SerializeField] private GameObject[] rescuePlayers;            // Man2(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ)
    [SerializeField] private Transform[] playersRescuePos;          // ÇÃ·¹ÀÌ¾î Å»Ãâ À§Ä¡[Man2StartPos, Girl2StartPos, Man2TargetPos, Girl2TargetPos]
    [SerializeField] private GameObject[] rescuePlayerParentBloods; // Å»Ãâ ÇÃ·¹ÀÌ¾î ÇÇ ÆÄÆ¼Å¬ ºÎ¸ğ °´Ã¼
    private GameObject[] rescuePlayerBloods;          // ±³½Ç ÇÃ·¹ÀÌ¾î ÇÇ ÆÄÆ¼Å¬(Man1, Girl1)
    private NavMeshAgent[] rescuePlayerAgent;                       // ÇÃ·¹ÀÌ¾î °æ·Î°è»ê AI ¿¡ÀÌÀüÆ®
    private Animator[] rescuePlayersAnimator;                       // ÇÃ·¹ÀÌ¾î ¾Ö´Ï¸ŞÀÌÅÍ ÄÄÆ÷³ÍÆ®
    [SerializeField] private ParticleSystem[] rescuePlayerRifles;   // ÇÃ·¹ÀÌ¾î ÃÑ ÆÄÆ¼Å¬
    private bool isRescuePlayerInit;                                // Å»Ãâ ÇÃ·¹ÀÌ¾î ÃÊ±âÈ­ ¿©ºÎ
    private bool isEscape;                                          // Å»Ãâ ¿©ºÎ

    [SerializeField] private GameObject rescueHelicopter;           // ±¸Á¶ Çï¸®ÄßÅÍ
    [SerializeField] private GameObject rescueHelicopterObj;        // ±¸Á¶ Çï¸®ÄßÅÍ º»Ã¼(ÇÏ´Ã¿¡ ÀÖ´Âµ¥ Å¸°Ù¿¡ µµÂøÇÏ¸é ¶¥¿¡ Âø·ú)
    private NavMeshAgent rescueHelicopterAgent;                     // ±¸Á¶ Çï¸®ÄßÅÍ AI ¿¡ÀÌÀüÆ®
    [SerializeField] private Transform rescueHelicopterPos;         // ±¸Á¶ Çï¸®ÄßÅÍ µµÂø Æ÷ÀÎÆ®
    private bool isHellcopterArrive;                                // ±¸Á¶ Çï¸®ÄßÅÍ µµÂø ¿©ºÎ

    private float playTime;                                         // ÀÏ¹İ Á»ºñ Á×À» ½Ã°£
    private ZombieState[] normalZombieState;                        // ÀÏ¹İ Á»ºñ »óÅÂ
    private NavMeshAgent[] normalZombieAgent;                       // ÇÃ·¹ÀÌ¾î °æ·Î°è»ê AI ¿¡ÀÌÀüÆ®
    private Animator[] normalZombieAnimator;                        // ÇÃ·¹ÀÌ¾î ¾Ö´Ï¸ŞÀÌÅÍ ÄÄÆ÷³ÍÆ®
    [SerializeField] private GameObject[] normalZombies;            // ÀÏ¹İ Á»ºñ
    [SerializeField] private Transform[] normalZombiesTargetPos;    // ÀÏ¹İ Á»ºñ À§Ä¡
    [SerializeField] private GameObject[] normalZombieParentBloods; // ÀÏ¹İ Á»ºñ ÇÇ ÆÄÆ¼Å¬ ºÎ¸ğ °´Ã¼
    private GameObject[] normalZombieBloods;          // ÀÏ¹İ Á»ºñ ÇÇ ÆÄÆ¼Å¬
    private bool isNormalZombieInit;                                // ÀÏ¹İ Á»ºñ ÃÊ±âÈ­ ¿©ºÎ

    private ZombieState bossZombieState;                            // º¸½º Á»ºñ »óÅÂ
    private NavMeshAgent bossZombieAgent;                           // º¸½º Á»ºñ °æ·Î°è»ê AI ¿¡ÀÌÀüÆ®
    private Animator bossZombieAnimator;                            // º¸½º Á»ºñ ¾Ö´Ï¸ŞÀÌÅÍ ÄÄÆ÷³ÍÆ®
    [SerializeField] private GameObject bossZombie;                 // º¸½º Á»ºñ
    [SerializeField] private Transform bossZombieTargetPos;         // º¸½º Á»ºñ À§Ä¡
    [SerializeField] private GameObject[] bossZombieParentBloods;   // º¸½º Á»ºñ ÇÇ ÆÄÆ¼Å¬ ºÎ¸ğ °´Ã¼
    private GameObject[] bossZombieBloods;            // º¸½º Á»ºñ ÇÇ ÆÄÆ¼Å¬ 
    private bool isBossZombieDie;                                   // º¸½º Á»ºñ Á×À½ ¿©ºÎ
    private bool isBossZombieInit;                                  // º¸½º Á»ºñ ÃÊ±âÈ­ ¿©ºÎ

    private ZombieState[] waveZombieState;                          // Á»ºñ¿şÀÌºê ¶§ Á»ºñµé »óÅÂ
    private NavMeshAgent[] waveZombieAgent;                         // Á»ºñ¿şÀÌºê ¶§ Á»ºñµé °æ·Î°è»ê AI ¿¡ÀÌÀüÆ®
    private Animator[] waveZombieAnimator;                          // Á»ºñ¿şÀÌºê ¶§ Á»ºñµé ¾Ö´Ï¸ŞÀÌÅÍ ÄÄÆ÷³ÍÆ®
    [SerializeField] private GameObject[] waveZombies;              // Á»ºñ¿şÀÌºê ¶§ »ı¼ºµÉ Á»ºñµé
    [SerializeField] private Transform[] waveZombiesTargetPos;      // Á»ºñ¿şÀÌºê ¶§ Á»ºñµé À§Ä¡
    [SerializeField] private GameObject[] waveZombieParentBloods;   // Á»ºñ¿şÀÌºê ¶§ Á»ºñµé ÇÇ ÆÄÆ¼Å¬
    private GameObject[] waveZombieBloods;            // Á»ºñ¿şÀÌºê ¶§ Á»ºñµé  ÇÇ ÆÄÆ¼Å¬ 
    private bool[] isWaveZombeDie;                                  // Á»ºñ Á×À½ ¿©ºÎ È®ÀÎ
    private bool isWaveZombieInit;                                  // Á»ºñ¿şÀÌºê ¶§ Á»ºñ ÃÊ±âÈ­ ¿©ºÎ

    [SerializeField] GameObject endingPanel;                        // ¿£µù UI

    private bool isEnd = false;

    #endregion Variable

    #region Unity Method

    /// <summary>
    /// ÄÄÆ÷³ÍÆ® ÃÊ±âÈ­
    /// </summary>
    private void Awake()
    {
        vaxAutoPlayState = VaxAutoPlayState.Init;

        // ÃÊ±âÈ­ ¿Ï·á ¿©ºÎ false·Î ÃÊ±âÈ­
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
            // °ÔÀÓ ¿ÀÅä ÇÃ·¹ÀÌ
            AutoPaly();

            // ÇÃ·¹ÀÌ¾î À§Ä¡ ÀÌµ¿ ¿©ºÎ Ã¼Å©
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
    ///  ÀüÃ¼ ÃÊ±âÈ­
    /// </summary>
    private void AllInit()
    {
        // ÇÏ³ª¶óµµ ÃÊ±âÈ­°¡ ¾ÈµÇ¸é µÉ ¶§°¡Áö Àç ½ÇÇà
        while (true)
        {
            // ÇÃ·¹ÀÌ¾î À§Ä¡, »óÅÂ µî ÃÊ±âÈ­ ÀÛ¾÷
            PlayerInit();

            // ÀÏ¹İ Á»ºñ À§Ä¡, »óÅÂ µî ÃÊ±âÈ­ ÀÛ¾÷
            NormalZombieInit();

            // º¸½º Á»ºñ À§Ä¡, »óÅÂ µî ÃÊ±âÈ­ ÀÛ¾÷
            BossZombieInit();

            // Á»ºñ ¿şÀÌºê ¶§ Á»ºñ À§Ä¡, »óÅÂ µî ÃÊ±âÈ­ ÀÛ¾÷
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
    /// ÇÃ·¹ÀÌ¾î À§Ä¡, »óÅÂ µî ÃÊ±âÈ­ ÀÛ¾÷
    /// </summary>
    private void PlayerInit()
    {
        Debug.Log("PlayerInit µé¾î¿È");

        // VAX ÀÚµ¿ ÇÃ·¹ÀÌ ÁøÇà »óÅÂ¸¦ ½ÃÀÛÀ¸·Î ÃÊ±âÈ­
        vaxAutoPlayState = VaxAutoPlayState.START;

        // ±³½Ç ÇÃ·¹ÀÌ¾î ÇÇ ÆÄÆ¼Å¬ ÃÊ±âÈ­
        classPlayerBloods = new GameObject[classPlayerParentBloods.Length];

        // ¸ŞÀÎÄ«¸Ş¶ó À§Ä¡ ÇĞ±³-±³½Ç·Î ÃÊ±âÈ­
        mainCamera.transform.position = mainCameraPos[0].position;

        // ÇÃ·¹ÀÌ¾î »óÅÂ ÃÊ±âÈ­
        playersState = new PlayerState[classPlayers.Length];

        // ÇÃ·¹ÀÌ¾î ¿¡ÀÌÀüÆ® ÃÊ±âÈ­
        classPlayerAgent = new NavMeshAgent[classPlayers.Length - 1];
        hospitalPlayerAgent = new NavMeshAgent[hospitalPlayers.Length];
        rescuePlayerAgent = new NavMeshAgent[rescuePlayers.Length];

        // ÇÃ·¹ÀÌ¾î ¾Ö´Ï¸ŞÀÌÅÍ ÃÊ±âÈ­
        classPlayersAnimator = new Animator[classPlayers.Length];
        hospitalPlayersAnimator = new Animator[hospitalPlayers.Length];
        rescuePlayersAnimator = new Animator[rescuePlayers.Length];

        // ÇÃ·¹ÀÌ¾î »óÅÂ°ª ÃÊ±âÈ­
        playersState[0] = PlayerState.ATTACK;        // Man1 Á»ºñ °ø°İ
        playersState[1] = PlayerState.THINKING;      // Man2 Ä¥ÆÇ¾Õ¿¡¼­ »ı°¢Áß
        playersState[2] = PlayerState.ATTACK;        // Girl1 Á»ºñ °ø°İ
        playersState[3] = PlayerState.ATTACK;        // Girl2-Maria Á»ºñ °ø°İ

        // ±³½Ç ÇÃ·¹ÀÌ¾î ÃÊ±âÈ­
        for (int i = 0; i < classPlayers.Length; i++)
        {
            // ÇÃ·¹ÀÌ¾î À§Ä¡ ÇĞ±³-±³½Ç·Î ÃÊ±âÈ­
            classPlayers[i].transform.position = playersClassPos[i].position;

            // ÇÇ ÆÄÆ¼Å¬ ÀÚ½Ä ÄÄÆ÷³ÍÆ® °¡Á®¿À±â
            classPlayerBloods[i] = classPlayerParentBloods[i].transform.Find("Blood").gameObject;

            // ÇÃ·¹ÀÌ¾î ¾Ö´Ï¸ŞÀÌÅÍ ÃÊ±âÈ­
            classPlayersAnimator[i] = classPlayers[i].GetComponent<Animator>();

            if (i == 1)
            {
                classPlayersAnimator[i].SetBool("IsThinking", true);
            }

            // Girl2 = ¸¶ÇÇ¾Æ´Â ³×ºê¸Ş½¬ ¾øÀ½
            if (i != classPlayers.Length - 1)
            {
                // ÇÃ·¹ÀÌ¾î ¿¡ÀÌÀüÆ® ÃÊ±âÈ­
                classPlayerAgent[i] = classPlayers[i].GetComponent<NavMeshAgent>();

                // ÇÃ·¹ÀÌ¾î ¿¡ÀÌÀüÆ® ½ºÇÇµå ¼³Á¤
                classPlayerAgent[i].speed = runSpeed;

                // ÇÃ·¹ÀÌ¾î ¿òÁ÷ÀÓ Á¤Áö
                classPlayerAgent[i].isStopped = true;
            }

            // ±³½Ç ÇÃ·¹ÀÌ¾î ½ºÆù ¿Ï·á µÇ¾úÀ» ¶§ ½ÇÇà
            if (i == classPlayers.Length - 1)
            {
                isClassPlayerInit = true;
            }
        }

        // º´¿ø ÇÃ·¹ÀÌ¾î ÃÊ±âÈ­
        for (int i = 0; i < hospitalPlayers.Length; i++)
        {
            // ÇÃ·¹ÀÌ¾î À§Ä¡ º´¿øÀ¸·Î ÃÊ±âÈ­
            //hospitalPlayers[i].transform.position = playersHospitalPos[i].position;

            // ÇÃ·¹ÀÌ¾î ¾Ö´Ï¸ŞÀÌÅÍ ÃÊ±âÈ­
            hospitalPlayersAnimator[i] = hospitalPlayers[i].GetComponent<Animator>();

            // ÇÃ·¹ÀÌ¾î °È±â
            hospitalPlayersAnimator[i].SetBool("IsMove", true);

            // ÇÃ·¹ÀÌ¾î ¿¡ÀÌÀüÆ® ÃÊ±âÈ­
            hospitalPlayerAgent[i] = hospitalPlayers[i].GetComponent<NavMeshAgent>();
            hospitalPlayerAgent[i].SetDestination(playersHospitalPos[i].position);

            // ÇÃ·¹ÀÌ¾î ¿¡ÀÌÀüÆ® ½ºÇÇµå ¼³Á¤
            hospitalPlayerAgent[i].speed = runSpeed * 4;

            // ÇÃ·¹ÀÌ¾î ¿òÁ÷ÀÓ Á¤Áö
            hospitalPlayerAgent[i].isStopped = true;

            Debug.Log("ÃÊ±âÈ­ Hospital Player Count : " + i);

            // º´¿ø ÇÃ·¹ÀÌ¾î ½ºÆù ¿Ï·á µÇ¾úÀ» ¶§ ½ÇÇà
            if (i == hospitalPlayers.Length - 1)
            {
                isHospitalPlayerInit = true;
            }
        }

        // º´¿ø ¹® ÃÊ±âÈ­
        hospitalDoorAnim.SetBool("Door", true);



        // Å»Ãâ ÇÃ·¹ÀÌ¾î ÇÇ ÆÄÆ¼Å¬ ÃÊ±âÈ­
        rescuePlayerBloods = new GameObject[rescuePlayerParentBloods.Length];

        // ÇÇ ÆÄÆ¼Å¬ ÀÚ½Ä ÄÄÆ÷³ÍÆ® °¡Á®¿À±â
        for (int i = 0; i < rescuePlayerParentBloods.Length; i++)
        {
            rescuePlayerBloods[i] = rescuePlayerParentBloods[i].transform.Find("Blood").gameObject;
        }

        // ÇĞ±³ ¿îµ¿Àå Å»Ãâ ÇÃ·¹ÀÌ¾î ÃÊ±âÈ­
        for (int i = 0; i < rescuePlayers.Length; i++)
        {
            // ÇÃ·¹ÀÌ¾î ¾Ö´Ï¸ŞÀÌÅÍ ÃÊ±âÈ­
            rescuePlayersAnimator[i] = rescuePlayers[i].GetComponent<Animator>();

            // ÇÃ·¹ÀÌ¾î ¿¡ÀÌÀüÆ® ÃÊ±âÈ­
            rescuePlayerAgent[i] = rescuePlayers[i].GetComponent<NavMeshAgent>();
            rescuePlayerAgent[i].SetDestination(playersRescuePos[i].position);

            // ÇÃ·¹ÀÌ¾î ¿òÁ÷ÀÓ Á¤Áö
            rescuePlayerAgent[i].isStopped = true;

            // ÇÃ·¹ÀÌ¾î ¿¡ÀÌÀüÆ® ½ºÇÇµå ¼³Á¤
            rescuePlayerAgent[i].speed = runSpeed * 7.7f;

            // Å»Ãâ ÇÃ·¹ÀÌ¾î ½ºÆù ¿Ï·á µÇ¾úÀ» ¶§ ½ÇÇà
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
    /// ÀÏ¹İ Á»ºñ À§Ä¡, »óÅÂ µî ÃÊ±âÈ­ ÀÛ¾÷
    /// </summary>
    private void NormalZombieInit()
    {
        // ÀÏ¹İ Á»ºñ »óÅÂ ÃÊ±âÈ­
        normalZombieState = new ZombieState[normalZombies.Length];

        // ÇÃ·¹ÀÌ¾î ¿¡ÀÌÀüÆ® ÃÊ±âÈ­
        normalZombieAgent = new NavMeshAgent[normalZombies.Length];

        // ÇÃ·¹ÀÌ¾î ¾Ö´Ï¸ŞÀÌÅÍ ÃÊ±âÈ­
        normalZombieAnimator = new Animator[normalZombies.Length];

        // ÀÏ¹İ Á»ºñ ÇÇ ÆÄÆ¼Å¬ ÃÊ±âÈ­
        normalZombieBloods = new GameObject[normalZombieParentBloods.Length];

        // ÀÏ¹İ Á»ºñ ÃÊ±âÈ­
        for (int i = 0; i < normalZombies.Length; i++)
        {
            // ÀÏ¹İ Á»ºñ »óÅÂ°ª ÃÊ±âÈ­
            normalZombieState[i] = ZombieState.IDLE;

            // ÇÇ ÆÄÆ¼Å¬ ÀÚ½Ä ÄÄÆ÷³ÍÆ® °¡Á®¿À±â
            normalZombieBloods[i] = normalZombieParentBloods[i].transform.Find("Blood").gameObject;

            // ÀÏ¹İ Á»ºñ ¿¡ÀÌÀüÆ® ÃÊ±âÈ­
            normalZombieAgent[i] = normalZombies[i].GetComponent<NavMeshAgent>();

            // ÀÏ¹İ Á»ºñ ¿¡ÀÌÀüÆ® ½ºÇÇµå ¼³Á¤
            normalZombieAgent[i].speed = runSpeed;

            // ¿òÁ÷ÀÌÁö ¸øÇÏ°Ô ¼³Á¤
            normalZombieAgent[i].isStopped = true;

            // ÀÏ¹İ Á»ºñ Å¸°Ù À§Ä¡ ÃÊ±âÈ­
            normalZombieAgent[i].SetDestination(normalZombiesTargetPos[i].position);

            // ÀÏ¹İ Á»ºñ ¾Ö´Ï¸ŞÀÌÅÍ ÃÊ±âÈ­
            normalZombieAnimator[i] = normalZombies[i].GetComponent<Animator>();

            normalZombieAnimator[i].SetInteger("RandomWalk", Random.Range(0, 8));


            // ÀÏ¹İ Á»ºñ ½ºÆù ¿Ï·á µÇ¾úÀ» ¶§ ½ÇÇà
            if (i == normalZombies.Length - 1)
            {
                isNormalZombieInit = true;
            }
        }

        normalZombieAgent[0].isStopped = false;  // normalZombieAgent[0] Á×À¸¸é ÀÌµ¿ ¿¹Á¤
        normalZombieAgent[1].isStopped = false;  // normalZombieAgent[1] Á×À¸¸é ÀÌµ¿ ¿¹Á¤
    }

    /// <summary>
    /// º¸½º Á»ºñ À§Ä¡, »óÅÂ µî ÃÊ±âÈ­ ÀÛ¾÷
    /// </summary>
    private void BossZombieInit()
    {
        // º¸½º Á»ºñ Á×À½ ¿©ºÎ
        isBossZombieDie = false;

        // º¸½º Á»ºñ »óÅÂ ÃÊ±âÈ­
        bossZombieState = ZombieState.IDLE;

        // º¸½º Á»ºñ ÇÇ ÆÄÆ¼Å¬ ÃÊ±âÈ­
        bossZombieBloods = new GameObject[bossZombieParentBloods.Length];
        bossZombieBloods[0] = bossZombieParentBloods[0].transform.Find("Blood").gameObject;
        bossZombieBloods[1] = bossZombieParentBloods[1].transform.Find("Blood").gameObject;

        // º¸½º Á»ºñ ¿¡ÀÌÀüÆ® ÃÊ±âÈ­
        bossZombieAgent = bossZombie.GetComponent<NavMeshAgent>();

        // ¸¶ÇÇ¾Æ ÇÃ·¹ÀÌ¾î°¡ Á×À¸¸é ÀÌµ¿ ¿¹Á¤
        bossZombieAgent.isStopped = true;

        // º¸½º Á»ºñ Å¸°ÙÀ§Ä¡ ÃÊ±âÈ­
        bossZombieAgent.SetDestination(bossZombieTargetPos.position);

        // ÇÃ·¹ÀÌ¾î ¿¡ÀÌÀüÆ® ½ºÇÇµå ¼³Á¤
        bossZombieAgent.speed = runSpeed * 4;

        // º¸½º Á»ºñ ¾Ö´Ï¸ŞÀÌÅÍ ÃÊ±âÈ­
        bossZombieAnimator = bossZombie.GetComponent<Animator>();

        // º¸½º Á»ºñ ½ºÆù ¿Ï·á µÇ¾úÀ» ¶§ ½ÇÇà
        isBossZombieInit = true;
    }

    /// <summary>
    /// Á»ºñ ¿şÀÌºê ¶§ Á»ºñÀ§Ä¡, »óÅÂ µî ÃÊ±âÈ­ ÀÛ¾÷
    /// </summary>
    private void WaveZombieInit()
    {

        // Á»ºñ¿şÀÌºê Á»ºñ »óÅÂ ÃÊ±âÈ­
        waveZombieState = new ZombieState[waveZombies.Length];

        // Á»ºñ¿şÀÌºê ÇÇ ÆÄÆ¼Å¬ ÃÊ±âÈ­
        waveZombieBloods = new GameObject[waveZombieParentBloods.Length];

        // Á»ºñ Á×À½ ¿©ºÎ ÃÊ±âÈ­
        isWaveZombeDie = new bool[waveZombies.Length];

        // Á»ºñ¿şÀÌºê Á»ºñ  ¿¡ÀÌÀüÆ® ÃÊ±âÈ­
        waveZombieAgent = new NavMeshAgent[waveZombies.Length];

        // Á»ºñ¿şÀÌºê Á»ºñ  ¾Ö´Ï¸ŞÀÌÅÍ ÃÊ±âÈ­
        waveZombieAnimator = new Animator[waveZombies.Length];

        // Á»ºñ¿şÀÌºê Á»ºñ ÃÊ±âÈ­
        for (int i = 0; i < waveZombies.Length; i++)
        {
            // Á»ºñ¿şÀÌºê »óÅÂ°ª ÃÊ±âÈ­
            waveZombieState[i] = ZombieState.IDLE;

            // ÇÇ ÆÄÆ¼Å¬ ÀÚ½Ä ÄÄÆ÷³ÍÆ® °¡Á®¿À±â
            waveZombieBloods[i] = waveZombieParentBloods[i].transform.Find("Blood").gameObject;

            // Á»ºñ¿şÀÌºê ¿¡ÀÌÀüÆ® ÃÊ±âÈ­
            waveZombieAgent[i] = waveZombies[i].GetComponent<NavMeshAgent>();

            // Á»ºñ¿şÀÌºê ¿¡ÀÌÀüÆ® ½ºÇÇµå ¼³Á¤
            waveZombieAgent[i].speed = runSpeed;

            // Á»ºñ¿şÀÌºê ¿¡ÀÌÀüÆ® ÀÌµ¿ ¸øÇÏ°Ô ¼³Á¤
            waveZombieAgent[i].isStopped = true;

            // Á»ºñ¿şÀÌºê À§Ä¡ ÃÊ±âÈ­
            waveZombieAgent[i].SetDestination(waveZombiesTargetPos[i].position);

            // Á»ºñ¿şÀÌºê ¾Ö´Ï¸ŞÀÌÅÍ ÃÊ±âÈ­
            waveZombieAnimator[i] = waveZombies[i].GetComponent<Animator>();

            waveZombieAnimator[i].SetInteger("RandomWalk", Random.Range(0, 8));

            // Á»ºñ¿şÀÌºê ½ºÆù ¿Ï·á µÇ¾úÀ» ¶§ ½ÇÇà
            if (i == waveZombies.Length - 1)
            {
                isWaveZombieInit = true;
            }
        }
    }

    /// <summary>
    /// °ÔÀÓ ÁøÇàµµ¿¡ µû¸¥ ³×ºê¸Ş½¬ Å¸°Ù ¼³Á¤
    /// </summary>
    private void AutoPaly()
    {
        if (vaxAutoPlayState == VaxAutoPlayState.Init)
        {
            AllInit();
        }
        // °ÔÀÓ ½ÃÀÛ : ÀÏ¹İÁ»ºñ Á×ÀÌ±â
        if (vaxAutoPlayState == VaxAutoPlayState.START)
        {
            // °ÔÀÓ ÁøÇà ½Ã°£
            playTime += Time.deltaTime;

            // ÀÏ¹İ Á»ºñ ¼ö ¸¸Å­ ¹İº¹
            for (int i = 0; i < normalZombies.Length; i++)
            {
                // agent.remainingDistance : ÇöÀç agent À§Ä¡¿Í ¿øÇÏ´Â À§Ä¡ÀÇ »çÀÌ°Å¸® °ª
                // agent.stoppingDistance : µµÂøÁöÁ¡ °Å¸®
                // ÁöÁ¤ÇÑ À§Ä¡¿¡ µµÂøÇÏ¸é °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç
                if (normalZombieAgent[i].remainingDistance <= normalZombieAgent[i].stoppingDistance
                    && normalZombieAnimator[i].GetBool("IsDie") == false)
                {
                    normalZombieAnimator[i].SetBool("IsAttack", true);
                }
            }

            // ÇÃ·¹ÀÌÅ¸ÀÓ Á¶°Ç
            if (playTime > 4 && playTime < 5)
            {
                // ±³½Ç ÇÃ·¹ÀÌ¾î ¼ö ¸¸Å­ ¹İº¹
                for (int i = 0; i < classPlayers.Length; i++)
                {
                    // Man2(»ı°¢Áß) ÇÃ·¹ÀÌ¾î Á¦¿ÜÇÏ°í °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                    if (classPlayersAnimator[i].GetBool("IsAttack") == false && i != 1
                        && normalZombieAnimator[2].GetBool("IsDie") == false)
                    {
                        classPlayersAnimator[i].SetBool("IsAttack", true);
                    }
                }

                // ½´ÆÃ ÆÄÆ¼Å¬ È°¼ºÈ­
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
                // ÀÏ¹İ Á»ºñ1 ¾Ö´Ï¸ŞÀÌ¼Ç Á×À½ Ã³¸®
                if (normalZombieAnimator[0].GetBool("IsDie") == false)
                {
                    normalZombieAnimator[0].SetBool("IsDie", true);
                    normalZombieBloods[0].GetComponent<BloodParticleReactivator>().Stop();
                }

                // Á»ºñ3 Ãâ¹ß
                normalZombieAgent[2].isStopped = false;
                normalZombieBloods[2].SetActive(true);
            }
            else if (playTime >= 16 && playTime < 16.5)
            {
                // Á»ºñ2 ¾Ö´Ï¸ŞÀÌ¼Ç Á×À½ Ã³¸®
                if (normalZombieAnimator[1].GetBool("IsDie") == false)
                {
                    normalZombieAnimator[1].SetBool("IsDie", true); 
                    normalZombieBloods[1].GetComponent<BloodParticleReactivator>().Stop();
                }

                // Á»ºñ4 Ãâ¹ß
                normalZombieAgent[3].isStopped = false;
                normalZombieBloods[3].SetActive(true);
            }
            else if (playTime >= 17 && playTime < 22)
            {
                // Á»ºñ3 ¾Ö´Ï¸ŞÀÌ¼Ç Á×À½ Ã³¸®
                if (normalZombieAnimator[2].GetBool("IsDie") == false)
                {
                    normalZombieAnimator[2].SetBool("IsDie", true);
                    normalZombieBloods[2].GetComponent<BloodParticleReactivator>().Stop();

                    // Á»ºñ3 ÃÄ´Ùº¸µµ·Ï ¼³Á¤
                    classPlayers[0].transform.LookAt(normalZombieAnimator[3].transform);
                    classPlayers[3].transform.LookAt(normalZombieAnimator[3].transform);
                }
            }
            else if (playTime >= 25)
            {
                // Á»ºñ4 ¾Ö´Ï¸ŞÀÌ¼Ç Á×À½ Ã³¸®
                if (normalZombieAnimator[3].GetBool("IsDie") == false)
                {
                    // Man1(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ), Girl2(¸¶ÇÇ¾Æ) °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç ºñÈ°¼ºÈ­
                    classPlayersAnimator[0].SetBool("IsAttack", false);
                    classPlayersAnimator[2].SetBool("IsAttack", false);
                    classPlayersAnimator[3].SetBool("IsAttack", false);

                    // ±³½Ç ÇÃ·¹ÀÌ¾î ½´ÆÃ ÆÄÆ¼Å¬ ÇØÁ¦
                    classPlayerRifles[0].Stop();
                    classPlayerRifles[2].Stop();

                    normalZombieAnimator[3].SetBool("IsDie", true);
                    normalZombieBloods[3].GetComponent<BloodParticleReactivator>().Stop();

                    vaxAutoPlayState = VaxAutoPlayState.NORMAL_ZOMBIES_DEAD;
                    playTime = 0;
                }
            }
        }
        // ÀÏ¹İ Á»ºñ Á×Àº ÈÄ : Girl2(¸¶ÆÄ¾Æ)°¡ Man1(»ıÁ¸ÀÚ) Á×ÀÓ
        else if (vaxAutoPlayState == VaxAutoPlayState.NORMAL_ZOMBIES_DEAD)
        {
            // °ÔÀÓ ÁøÇà ½Ã°£
            playTime += Time.deltaTime;

            // Man1 »ıÁ¸ÀÚ°¡ Á×Áö ¾Ê¾ÒÀ» ¶§ ½ÇÇà
            if (classPlayersAnimator[0].GetBool("IsDie") == false)
            {
                // Grirl2(¸¶ÇÇ¾Æ)°¡ Man1 »ıÁ¸ÀÚ °ø°İ ½ÃÀÛ
                if (playTime > 2 && playTime < 3.5)
                {
                    // Girl2(¸¶ÇÇ¾Æ)°¡ °ø°İ ´ë»ó Man1À» º¸µµ·Ï ¼³Á¤
                    classPlayers[3].transform.LookAt(classPlayers[0].transform);

                    // Girl2(¸¶ÇÇ¾Æ)°¡ °ø°İ ¾Ö´Ï¸ŞÀÌ¼ÇÀÌ ²¨Á®ÀÖÀ¸¸é ÄÑ±â
                    if (classPlayersAnimator[3].GetBool("IsAttack") == false)
                    {
                        classPlayersAnimator[3].SetBool("IsAttack", true);

                        // Girl2(¸¶ÇÇ¾Æ) ½´ÆÃ ÆÄÆ¼Å¬ Àç»ı
                        if (classPlayerRifles[2].isPlaying == false) classPlayerRifles[2].Play();

                        // Man1(»ıÁ¸ÀÚ) ÇÇ ÆÄÆ¼Å¬ È°¼ºÈ­
                        classPlayerBloods[0].SetActive(true);
                    }
                }
                // °ø°İ´çÇÑ Man1ÀÌ Girl2(¸¶ÇÇ¾Æ) °ø°İ
                else if (playTime >= 3.5 && playTime < 6)
                {
                    // Man1(»ıÁ¸ÀÚ)°¡ Girl2(¸¶ÇÇ¾Æ)¸¦ º¸µµ·Ï ¼³Á¤
                    classPlayers[0].transform.LookAt(classPlayers[3].transform);

                    // Man1(»ıÁ¸ÀÚ)°¡ °ø°İ ¾Ö´Ï¸ŞÀÌ¼ÇÀÌ ²¨Á®ÀÖÀ¸¸é ÄÑ±â
                    if (classPlayersAnimator[0].GetBool("IsAttack") == false)
                    {
                        classPlayersAnimator[0].SetBool("IsAttack", true);

                        // Man1(»ıÁ¸ÀÚ) ½´ÆÃ ÆÄÆ¼Å¬ Àç»ı
                        if (classPlayerRifles[0].isPlaying == false) classPlayerRifles[0].Play();
                    }
                }

                // Girl2(¸¶ÇÇ¾Æ)°¡ Man1 »ıÁ¸ÀÚ¸¦ °ø°İÇÏ´Â ¸ğ½ÀÀ» ´Ù¸¥ »ıÁ¸ÀÚ°¡ º¸°í ³î¶÷
                if (playTime >= 5.5f && playTime < 8)
                {
                    // Man2(»ıÁ¸ÀÚ)°¡ Girl2(¸¶ÇÇ¾Æ)¸¦ º¸µµ·Ï ¼³Á¤
                    classPlayers[1].transform.LookAt(classPlayers[3].transform);
                    // Girl1(»ıÁ¸ÀÚ)°¡ Girl2(¸¶ÇÇ¾Æ)¸¦ º¸µµ·Ï ¼³Á¤
                    classPlayers[2].transform.LookAt(classPlayers[3].transform);

                    // Man1(»ıÁ¸ÀÚ) »ı°¢Áß ¾Ö´Ï¸ŞÀÌ¼Ç ÇØÁ¦, ³î¶÷ ¾Ö´Ï¸ŞÀÌ¼Ç Àç»ı
                    classPlayersAnimator[1].SetBool("IsThinking", false);
                    classPlayersAnimator[1].SetBool("IsSurprised", true);

                    // Girl1(»ıÁ¸ÀÚ) ³î¶÷ ¾Ö´Ï¸ŞÀÌ¼Ç Àç»ı
                    classPlayersAnimator[2].SetBool("IsSurprised", true);
                }

                if (playTime >= 10)
                {
                    // Man1 Á×À½À¸·Î º¯°æ
                    classPlayersAnimator[0].SetBool("IsDie", true);

                    // Main1(»ıÁ¸ÀÚ) ÇÇ ÆÄÆ¼Å¬ Á¾·á
                    classPlayerBloods[0].SetActive(false);
                    classPlayerBloods[0].GetComponent<BloodParticleReactivator>().Stop();

                    classPlayerRifles[0].Stop();

                    // Girl2(¸¶ÇÇ¾Æ)°¡ °ø°İ ´ë»ó Girl1 º¸µµ·Ï ¼³Á¤
                    classPlayers[3].transform.LookAt(classPlayers[2].transform);

                    // Girl2(¸¶ÇÇ¾Æ) Àá½Ã °ø°İ ¸ØÃã
                    classPlayersAnimator[3].SetBool("IsAttack", false);

                    // Girl2(¸¶ÇÇ¾Æ) ½´ÆÃ ÆÄÆ¼Å¬ ºñÈ°¼ºÈ­
                    classPlayerRifles[2].Stop();
                }
            }
            // Man1(»ıÁ¸ÀÚ)°¡ Á×¾úÀ» ¶§ ½ÇÇà
            else
            {
                if (playTime >= 11 && playTime < 16)
                {
                    // Man2(»ıÁ¸ÀÚ) ³î¶÷ ¾Ö´Ï¸ŞÀÌ¼Ç ºñÈ°¼ºÈ­, °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                    classPlayersAnimator[1].SetBool("IsSurprised", false);
                    classPlayersAnimator[1].SetBool("IsAttack", true);

                    // Man2(»ıÁ¸ÀÚ) ½´ÆÃ ÆÄÆ¼Å¬ È°¼ºÈ­
                    if (classPlayerRifles[1].isPlaying == false)
                    {
                        classPlayerRifles[1].Play();

                        // Girl2(¸¶ÇÇ¾Æ) ÇÇ ÆÄÆ¼Å¬ È°¼ºÈ­
                        classPlayerBloods[3].SetActive(true);
                    }

                    // Girl1(»ıÁ¸ÀÚ) ³î¶÷ ¾Ö´Ï¸ŞÀÌ¼Ç ºñÈ°¼ºÈ­, °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                    classPlayersAnimator[2].SetBool("IsSurprised", false);
                    classPlayersAnimator[2].SetBool("IsAttack", true);

                    // Girl2(¸¶ÇÇ¾Æ) °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                    classPlayersAnimator[3].SetBool("IsAttack", true);

                    // Girl2(¸¶ÇÇ¾Æ) ½´ÆÃ ÆÄÆ¼Å¬ È°¼ºÈ­
                    if (classPlayerRifles[2].isPlaying == false)
                    {
                        classPlayerRifles[2].Play();

                        // Girl1(»ıÁ¸ÀÚ) ÇÇ ÆÄÆ¼Å¬ È°¼ºÈ­
                        classPlayerBloods[2].SetActive(true);
                    }
                }
                else if (playTime >= 17)
                {
                    // ³×ºê¸Ş½¬ Å¸°Ù Girl2(¸¶ÇÇ¾Æ) ÁöÁ¤
                    classPlayerAgent[2].SetDestination(classPlayers[3].transform.position);
                    // ³×ºê¸Ş½¬ Ãß°İ ½ÃÀÛ
                    classPlayerAgent[2].isStopped = false;

                    // °ÔÀÓÁøÇà »óÅÂ º¯°æ
                    vaxAutoPlayState = VaxAutoPlayState.MAN1_DEAD;

                    // ÇÃ·¹ÀÌ Å¸ÀÓ ÃÊ±âÈ­
                    playTime = 0;
                }
            }
        }
        // Man1(»ıÁ¸ÀÚ) Á×Àº ÈÄ ³²Àº »ıÁ¸ÀÚµéÀÌ Girl2(¸¶ÇÇ¾Æ) Á×ÀÓ
        else if (vaxAutoPlayState == VaxAutoPlayState.MAN1_DEAD)
        {
            // °ÔÀÓ ÁøÇà ½Ã°£
            playTime += Time.deltaTime;

            // Girl2(¸¶ÇÇ¾Æ)°¡ Á×Áö ¾Ê¾ÒÀ» ¶§ ½ÇÇà
            if (classPlayersAnimator[3].GetBool("IsDie") == false)
            {
                if (playTime > 4)
                {
                    // Girl2(¸¶ÇÇ¾Æ) Á×À½ Ã³¸®
                    classPlayersAnimator[3].SetBool("IsDie", true);
                    // ½´ÆÃ ÆÄÆ¼Å¬ ºñÈ°¼ºÈ­
                    classPlayerRifles[2].Stop();

                    // Man2(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ) º¸½º Á»ºñ ¹Ù¶óº¸µµ·Ï ¼³Á¤
                    classPlayers[1].transform.LookAt(bossZombie.transform);
                    classPlayers[2].transform.LookAt(bossZombie.transform);

                    // Man2(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ) °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç ºñÈ°¼ºÈ­
                    classPlayersAnimator[1].SetBool("IsAttack", false);
                    classPlayersAnimator[2].SetBool("IsAttack", false);

                    // ½´ÆÃ ÆÄÆ¼Å¬ ºñÈ°¼ºÈ­
                    classPlayerRifles[1].Stop();

                    // Girl1(»ıÁ¸ÀÚ), Girl2(¸¶ÇÇ¾Æ) ÇÇ ÆÄÆ¼Å¬ Á¾·á
                    classPlayerBloods[2].GetComponent<BloodParticleReactivator>().Stop();
                    classPlayerBloods[2].SetActive(false);
                    classPlayerBloods[3].GetComponent<BloodParticleReactivator>().Stop();
                    classPlayerBloods[3].SetActive(false);
                }
            }
            // Girl2(¸¶ÇÇ¾Æ)°¡ Á×¾úÀ» ¶§ ½ÇÇà
            else
            {
                if (playTime >= 6.5f && playTime < 8f)
                {
                    // Man2(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ) ³î¶÷ ¾Ö´Ï¸ŞÀÌ¼Ç ½ÇÇà
                    classPlayersAnimator[1].SetBool("IsSurprised", true);
                    classPlayersAnimator[2].SetBool("IsSurprised", true);
                }
                else if (playTime >= 8)
                {
                    // Man2(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ) ³î¶÷ ¾Ö´Ï¸ŞÀÌ¼Ç ºñÈ°¼ºÈ­
                    classPlayersAnimator[1].SetBool("IsSurprised", false);
                    classPlayersAnimator[2].SetBool("IsSurprised", false);

                    // °ÔÀÓ ÁøÇà »óÅÂ ¸¶ÇÇ¾Æ Á×Àº ÈÄ·Î º¯°æ
                    vaxAutoPlayState = VaxAutoPlayState.MAFIA_DEAD;

                    // º¸½º Á»ºñ Ãâ¹ß
                    bossZombieAgent.isStopped = false;

                    // ÇÃ·¹ÀÌÅ¸ÀÓ ÃÊ±âÈ­
                    playTime = 0;
                }
            }
        }
        // Girl2(¸¶ÇÇ¾Æ) Á×Àº ÈÄ µîÀåÇÑ º¸½º Á»ºñ Á×ÀÌ±â
        else if (vaxAutoPlayState == VaxAutoPlayState.MAFIA_DEAD)
        {
            //Debug.Log("MAN1_DEAD º¸½º Á»ºñ µîÀå");

            // °ÔÀÓ ÁøÇà ½Ã°£
            playTime += Time.deltaTime;

            // agent.remainingDistance : ÇöÀç agent À§Ä¡¿Í ¿øÇÏ´Â À§Ä¡ÀÇ »çÀÌ°Å¸® °ª
            // agent.stoppingDistance : µµÂøÁöÁ¡ °Å¸®
            // ÁöÁ¤ÇÑ À§Ä¡¿¡ µµÂøÇÏ¸é °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç
            if (bossZombieAgent.remainingDistance <= bossZombieAgent.stoppingDistance
                && isBossZombieDie == false)
            {
                // º¸½º Á»ºñ °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                bossZombieAnimator.SetBool("IsAttack", true);

                // Man2(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ) º¸½º Á»ºñ¿¡°Ô °ø°İ ½ÃÀÛ
                classPlayersAnimator[1].SetBool("IsAttack", true);
                classPlayersAnimator[2].SetBool("IsAttack", true);

                // Man2(»ıÁ¸ÀÚ) ½´ÆÃ ÆÄÆ¼Å¬ È°¼ºÈ­
                if (classPlayerRifles[1].isPlaying == false) classPlayerRifles[1].Play();

                // Man2(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ), º¸½º Á»ºñ ÇÇ ÆÄÆ¼Å¬ È°¼ºÈ­
                classPlayerBloods[1].SetActive(true);
                bossZombieBloods[0].SetActive(true);
                bossZombieBloods[1].SetActive(true);

                // Girl1(»ıÁ¸ÀÚ) º¸½º Á»ºñ¸¦ ³×ºê¸Ş½¬·Î Å¸°ÙÀ¸·Î ¼³Á¤
                classPlayerAgent[2].SetDestination(bossZombie.transform.position);

                // Girl1(»ıÁ¸ÀÚ) Ãß°İ ½ÃÀÛ
                if (classPlayerAgent[2].remainingDistance > 0.2f)
                {
                    classPlayerAgent[2].isStopped = true;
                }
            }

            // º¸½º Á»ºñ°¡ Á×Áö ¾Ê¾ÒÀ» ¶§ ½ÇÇà
            if (isBossZombieDie == false)
            {
                // Man2(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ) º¸½º Á»ºñ ¹Ù¶óº¸µµ·Ï ¼³Á¤
                classPlayers[1].transform.LookAt(bossZombie.transform);
                classPlayers[2].transform.LookAt(bossZombie.transform);

                // Girl1(»ıÁ¸ÀÚ) º¸½º Á»ºñ¸¦ ³×ºê¸Ş½¬·Î Å¸°ÙÀ¸·Î ¼³Á¤
                classPlayerAgent[2].SetDestination(bossZombie.transform.position);

                // Girl1(»ıÁ¸ÀÚ) º¸½º Á»ºñ¿ÍÀÇ °Å¸®°¡ 0.5f ¹Ì¸¸À¸·Î Á¼ÇôÁö¸é ¸ØÃã
                if (classPlayerAgent[2].remainingDistance < 0.5f)
                {
                    classPlayerAgent[2].isStopped = false;
                }

                // ÇÃ·¹ÀÌÅ¸ÀÓ 18ÃÊ ÀÌ»ó µÇ¸é º¸½º Á»ºñ Á×À½
                if (playTime > 18)
                {
                    isBossZombieDie = true;

                    // Man2(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ), º¸½º Á»ºñ ÇÇ ÆÄÆ¼Å¬ ºñÈ°¼ºÈ­
                    classPlayerBloods[1].GetComponent<BloodParticleReactivator>().Stop();
                    classPlayerBloods[1].SetActive(false);
                    bossZombieBloods[0].GetComponent<BloodParticleReactivator>().Stop();
                    bossZombieBloods[0].SetActive(false);
                    bossZombieBloods[1].GetComponent<BloodParticleReactivator>().Stop();
                    bossZombieBloods[1].SetActive(false);
                }
            }
            // º¸½º Á»ºñ°¡ Á×¾úÀ» ¶§ ½ÇÇà
            else if (isBossZombieDie == true)
            {
                // º´¿øÅ° ºñÈ°¼ºÈ­ »óÅÂÀÏ ¶§ ½ÇÇà
                if (hospitalKey.activeSelf == false && playTime < 22)
                {
                    // º¸½º Á»ºñ Á×À½ ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                    bossZombieAnimator.SetBool("IsDie", true);

                    // Man2(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ) °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç ºñÈ°¼ºÈ­
                    classPlayersAnimator[1].SetBool("IsAttack", false);
                    classPlayersAnimator[2].SetBool("IsAttack", false);

                    // Man2(»ıÁ¸ÀÚ) ½´ÆÃ ÆÄÆ¼Å¬ ºñÈ°¼ºÈ­
                    classPlayerRifles[1].Stop();

                    // Ä«¸Ş¶ó º¸°í ¿ôµµ·Ï ¼³Á¤
                    classPlayers[1].transform.LookAt(mainCamera.transform.position);
                    classPlayers[2].transform.LookAt(mainCamera.transform.position);

                    // Man2(»ıÁ¸ÀÚ), Girl1(»ıÁ¸ÀÚ) ±â»İ ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                    classPlayersAnimator[1].SetBool("IsJoy", true);
                    classPlayersAnimator[2].SetBool("IsJoy", true);

                    // º´¿ø Å° È°¼ºÈ­
                    hospitalKey.SetActive(true);
                }
                // º´¿øÅ° È°¼ºÈ­ µÇ¾úÀ» ¶§ ½ÇÇà
                else
                {
                    if (playTime > 25)
                    {
                        // º´¿ø Å° ºñÈ°¼ºÈ­
                        hospitalKey.SetActive(false);

                        // ÇĞ±³ ±³½Ç ÇÃ·¹ÀÌ¾î ¿ÀºêÁ§Æ® »èÁ¦
                        for (int i = 0; i < classPlayers.Length; i++)
                        {
                            Destroy(classPlayers[i].gameObject, 2f);
                        }

                        // ÇĞ±³ ÀÏ¹İ Á»ºñ ¿ÀºêÁ§Æ® »èÁ¦
                        for (int i = 0; i < normalZombies.Length; i++)
                        {
                            Destroy(normalZombies[i].gameObject, 2f);
                        }

                        // º¸½º Á»ºñ »èÁ¦
                        Destroy(bossZombie.gameObject, 2f);

                        // ¸ŞÀÎÄ«¸Ş¶ó º´¿øÀ¸·Î ÀÌµ¿
                        mainCamera.transform.position = mainCameraPos[1].position;
                        mainCamera.transform.rotation = mainCameraPos[1].rotation;

                        // È÷µç º´¿ø Å° º´¿ø À§Ä¡·Î ÀÌµ¿(¹®¿©´Â ¿ëµµ·Î »ç¿ëÇÔ)
                        hospitalKey.transform.position = hospitalKeyPos.position;

                        // º´¿ø ¹® ¿­±â
                        hospitalDoorAnim.SetBool("Door", false);

                        // º´¿ø ÇÃ·¹ÀÌ¾î Å¸°Ù ¼³Á¤ ¹× ÀÌµ¿ ¼³Á¤
                        //hospitalPlayerAgent[0].SetDestination(playersHospitalPos[0].position);
                        //hospitalPlayerAgent[1].SetDestination(playersHospitalPos[1].position);
                        hospitalPlayerAgent[0].isStopped = false;
                        hospitalPlayerAgent[1].isStopped = false;

                        // °ÔÀÓ ÁøÇà »óÅÂ º¯°æ
                        vaxAutoPlayState = VaxAutoPlayState.BOSS_ZOMBIE_DEAD;

                        // °ÔÀÓ ÁøÇà ³Ñ±â±â À§ÇØ ÇÃ·¹ÀÌ Å¸ÀÓ °­Á¦ Á¶Á¤
                        playTime = 0;
                    }
                }
            }
        }
        // º¸½º Á»ºñ Á×¾úÀ¸¸é È÷µç ¿­¼è·Î º´¿ø¹® ¿­°í µé¾î°¡¼­ ¹é½Å Ã£´Â´Ù.
        else if (vaxAutoPlayState == VaxAutoPlayState.BOSS_ZOMBIE_DEAD)
        {
            // °ÔÀÓ ÁøÇà ½Ã°£
            playTime += Time.deltaTime;

            // Girl1(»ıÁ¸ÀÚ)°¡ Å¸°Ù Æ÷ÀÎÆ®¿¡ µµÂø ÇßÀ» ¶§ ½ÇÇà
            if (hospitalPlayerAgent[1].remainingDistance <= hospitalPlayerAgent[1].stoppingDistance
                && vax.activeSelf == false && hospitalPlayersAnimator[0].GetBool("IsLookFor") == false)
            {
                // ÇÃ·¹ÀÌ¾î ¸ØÃß°í °È±â ¾Ö´Ï¸ŞÀÌ¼Ç ºñÈ°¼ºÈ­
                hospitalPlayerAgent[0].isStopped = true;
                hospitalPlayerAgent[1].isStopped = true;
                hospitalPlayersAnimator[0].SetBool("IsMove", false);
                hospitalPlayersAnimator[1].SetBool("IsMove", false);

                // Ã£±â ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                hospitalPlayersAnimator[0].SetBool("IsLookFor", true);
                hospitalPlayersAnimator[1].SetBool("IsLookFor", true);
            }
            else
            {
                if (playTime >= 10 && playTime < 17)
                {
                    vax.SetActive(true);

                    // Ä«¸Ş¶ó º¸°í ¿ôµµ·Ï ¼³Á¤
                    hospitalPlayers[0].transform.LookAt(mainCamera.transform.position);
                    hospitalPlayers[1].transform.LookAt(mainCamera.transform.position);

                    // Ã£±â ¾Ö´Ï¸ŞÀÌ¼Ç ºñÈ°¼ºÈ­µÇ°í ÀÚµ¿À¸·Î ±â»İ ¾Ö´Ï¸ŞÀÌ¼Ç ÁøÇà
                    hospitalPlayersAnimator[0].SetBool("IsLookFor", false);
                    hospitalPlayersAnimator[1].SetBool("IsLookFor", false);
                    hospitalPlayersAnimator[0].SetBool("IsJoy", true);
                    hospitalPlayersAnimator[1].SetBool("IsJoy", true);

                    // Å»Ãâ ÇÃ·¹ÀÌ¾î ±â»İ ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                    /*rescuePlayersAnimator[0].SetBool("IsJoy", true);
                    rescuePlayersAnimator[1].SetBool("IsJoy", true);*/
                }
                else if (playTime > 17)
                {
                    // º´¿ø °ü·Ã ¿ÀºêÁ§Æ® »èÁ¦
                    Destroy(hospitalPlayers[0].gameObject, 2f);
                    Destroy(hospitalPlayers[1].gameObject, 2f);
                    Destroy(hospitalKey.gameObject, 2f);
                    Destroy(vax.gameObject, 2f);

                    // Ä«¸Ş¶ó À§Ä¡ ÇĞ±³-¿îµ¿Àå ÀÌµ¿
                    mainCamera.transform.position = mainCameraPos[2].position;
                    mainCamera.transform.rotation = mainCameraPos[2].rotation;

                    // ½ÅÈ£Åº ¿ÀºêÁ§Æ® È°¼ºÈ­
                    firework.SetActive(true);

                    // Á»ºñ ¿şÀÌºê ½ÃÀÛ
                    for (int i = 0; i < waveZombies.Length; i++)
                    {
                        waveZombieAgent[i].isStopped = false;
                    }

                    // °ÔÀÓ ÁøÇà»óÅÂ º¯°æ
                    vaxAutoPlayState = VaxAutoPlayState.RESCUE_REQEUST;

                    // ÇÃ·¹ÀÌ Å¸ÀÓ ÃÊ±âÈ­
                    playTime = 0;
                }
            }

        }
        // ÇĞ±³-¿îµ¿Àå¿¡¼­ ½ÅÈ£Åº ÅÍÆ®¸° ÈÄ Çï±â°¡ ¿Ã ¶§±îÁö »ì¾Æ´Ô±â
        else if (vaxAutoPlayState == VaxAutoPlayState.RESCUE_REQEUST)
        {
            // °ÔÀÓ ÁøÇà ½Ã°£
            playTime += Time.deltaTime;

            // Å»Ãâ ¼º°øÇßÀ» ¶§
            if(playTime > 5 && isEscape == true)
            {
                Debug.Log("Å»Ãâ ¼º°ø °ÔÀÓ Æ®·¹ÀÏ·¯ Á¾·á");
                isEnd= true;
            }

            // Á»ºñ ¿şÀÌºêÀÇ Á»ºñ ¼ö ¸¸Å­ ¹İº¹
            for (int i = 0; i < waveZombies.Length; i++)
            {
                // ÇØ´ç Á»ºñ°¡ »ì¾ÆÀÖ°í Å¸°Ù Æ÷ÀÎÆ®¿¡ µµÂøÇÏ¸é ½ÇÇà
                if (waveZombieAgent[i].remainingDistance <= waveZombieAgent[i].stoppingDistance
                && waveZombieAnimator[i].GetBool("IsDie") == false)
                {
                    // °ø°İ ¾Ö´Ï¸ŞÀÌ¼ÇÀÌ ¾Æ´Ò ¶§ È°¼ºÈ­·Î º¯°æ
                    if (waveZombieAnimator[i].GetBool("IsAttack") == false)
                    {
                        waveZombieAnimator[i].SetBool("IsAttack", true);
                    }
                    waveZombieAgent[i].isStopped = true;
                }

                // Á»ºñ Á×À½ ¿©ºÎ Ã¼Å©
                if (isWaveZombeDie[i] == true)
                {
                    // Á×À½ ¾Ö´Ï¸ŞÀÌ¼ÇÀÌ ¾Æ´Ò ¶§ È°¼ºÈ­
                    if (waveZombieAnimator[i].GetBool("IsDie") == false)
                    {
                        waveZombieAnimator[i].SetBool("IsDie", true);
                        waveZombieBloods[i].SetActive(false);
                        waveZombieBloods[i].GetComponent<BloodParticleReactivator>().Stop();
                    }
                }

                // Man2(»ıÁ¸ÀÚ)°¡ Á×À¸¸é ÀüÃ¼ Á»ºñ 
                if(rescuePlayersAnimator[0].GetBool("IsDie") == true)
                {
                    // Á»ºñ Á×À½ ¿©ºÎ Ã¼Å©
                    if (isWaveZombeDie[i] == false)
                    {
                        waveZombieBloods[i].SetActive(false);
                        waveZombieBloods[i].GetComponent<BloodParticleReactivator>().Stop();
                    }
                }
            }

            // Man2(»ıÁ¸ÀÚ)°¡ Á×Áö ¾Ê¾ÒÀ» ¶§ ½ÇÇà
            if (rescuePlayersAnimator[0].GetBool("IsDie") == false)
            {
                if (playTime > 3 && playTime < 5)
                {
                    // Ä«¸Ş¶ó º¸°í ¿ôµµ·Ï ¼³Á¤
                    rescuePlayers[0].transform.LookAt(mainCamera.transform.position);
                    rescuePlayers[1].transform.LookAt(mainCamera.transform.position);

                    rescuePlayersAnimator[0].SetBool("IsJoy", true);
                    rescuePlayersAnimator[1].SetBool("IsJoy", true);
                }
                else if (playTime > 5 && playTime < 6)
                {
                    rescueHelicopter.SetActive(true);

                    // ±¸Á¶ Çï¸®ÄßÅÍ ­z¶ó
                    rescueHelicopterAgent.SetDestination(rescueHelicopterPos.position);
                    rescueHelicopterAgent.isStopped = false;

                    // ÇÃ·¹ÀÌ¾î ¿îµ¿Àå ÀÔ±¸ Á»ºñ ¸·À¸·¯ Ãâ¹ß
                    rescuePlayerAgent[0].isStopped = false;
                    rescuePlayerAgent[1].isStopped = false;

                    // ¿ôÀ½ ¾Ö´Ï¸ŞÀÌ¼Ç ºñÈ°¼ºÈ­, ÀÌµ¿ ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                    rescuePlayersAnimator[0].SetBool("IsJoy", false);
                    rescuePlayersAnimator[1].SetBool("IsJoy", false);
                    rescuePlayersAnimator[0].SetBool("IsMove", true);
                    rescuePlayersAnimator[1].SetBool("IsMove", true);

                    // Á»ºñ ½ºÇÇµå ºü¸£°Ô º¯°æ
                    for (int i = 0; i < waveZombies.Length; i++)
                    {
                        waveZombieAgent[i].speed = runSpeed * 2;
                    }
                }
                // Man2(»ıÁ¸ÀÚ)°¡ ¿îµ¿Àå ÀÔ±¸ Á»ºñ ¸·À¸·Î µµÂø ÇÏ¸é °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                if (rescuePlayerAgent[0].remainingDistance <= rescuePlayerAgent[0].stoppingDistance)
                {
                    // °ø°İ ¾Ö´Ï¸ŞÀÌ¼ÇÀÌ È°¼ºÈ­ ¾ÈµÇ¾úÀ» ¶§¸¸ ½ÇÇà
                    if (rescuePlayersAnimator[0].GetBool("IsAttack") == false)
                    {
                        // Man2(»ıÁ¸ÀÚ) ¸ØÃß°í °ø°İ ½ÃÀÛ
                        rescuePlayerAgent[0].isStopped = true;
                        rescuePlayersAnimator[0].SetBool("IsMove", false);
                        rescuePlayersAnimator[0].SetBool("IsAttack", true);
                        rescuePlayerRifles[0].Play();
                        waveZombieBloods[1].SetActive(true);
                        waveZombieBloods[3].SetActive(true);
                        Debug.Log("Man1 °ø°İ ½ÃÀÛ");

                        rescuePlayerBloods[0].SetActive(true);
                    }
                }

                // Girl1(»ıÁ¸ÀÚ)°¡ ¿îµ¿Àå ÀÔ±¸ Á»ºñ ¸·À¸·Î µµÂø ÇÏ¸é °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                if (rescuePlayerAgent[1].remainingDistance <= rescuePlayerAgent[1].stoppingDistance)
                {
                    // °ø°İ ¾Ö´Ï¸ŞÀÌ¼ÇÀÌ È°¼ºÈ­ ¾ÈµÇ¾úÀ» ¶§¸¸ ½ÇÇà
                    if (rescuePlayersAnimator[1].GetBool("IsAttack") == false)
                    {
                        // Girl1(»ıÁ¸ÀÚ) ¸ØÃß°í °ø°İ ½ÃÀÛ
                        //rescuePlayerAgent[1].isStopped = true;
                        rescuePlayersAnimator[1].SetBool("IsMove", false);
                        rescuePlayersAnimator[1].SetBool("IsAttack", true);
                        rescuePlayerBloods[1].SetActive(true);
                        waveZombieBloods[0].SetActive(true);
                        waveZombieBloods[2].SetActive(true);
                        Debug.Log("Girl1 °ø°İ ½ÃÀÛ");
                    }
                    rescuePlayers[1].transform.LookAt(waveZombiesTargetPos[1]);
                }

                // Á»ºñ 1 Á×À½
                if (playTime >= 15 && playTime < 16)
                {
                    isWaveZombeDie[0] = true;
                    Debug.Log("Á»ºñ 1 Á×À½");
                    waveZombieBloods[4].SetActive(true);
                }
                // Á»ºñ 2 Á×À½
                else if (playTime >= 18 && playTime < 19)
                {
                    isWaveZombeDie[1] = true;
                    Debug.Log("Á»ºñ 2 Á×À½");
                    waveZombieBloods[5].SetActive(true);
                }
                // Á»ºñ 3 Á×À½
                else if (playTime >= 21 && playTime < 22)
                {
                    isWaveZombeDie[2] = true;
                    Debug.Log("Á»ºñ3 :Á×À½");
                    waveZombieBloods[6].SetActive(true);
                }
                // Á»ºñ 3 Á×À½
                else if (playTime >= 25 && playTime < 26)
                {
                    isWaveZombeDie[3] = true;
                    Debug.Log("Á»ºñ4 Á×À½");
                    waveZombieBloods[7].SetActive(true);
                }
                // Á»ºñ 5 Á×À½
                else if (playTime >= 28 && playTime < 29)
                {
                    isWaveZombeDie[4] = true;
                    Debug.Log("Á»ºñ5 Á×À½");
                }
                // Man1 Á×À½
                else if (playTime >= 30)
                {
                    rescuePlayersAnimator[0].SetBool("IsDie", true);
                    rescuePlayerRifles[0].Stop();
                    rescuePlayerBloods[0].GetComponent<BloodParticleReactivator>().Stop();
                }
            }
            else
            {
                // »ì¾ÆÀÖ´Â Á»ºñ Å¸°Ù Girl1·Î ¼³Á¤
                for (int i = 0; i < isWaveZombeDie.Length; i++)
                {
                    if (isWaveZombeDie[i] == false && playTime > 34)
                    {
                        waveZombieAgent[i].SetDestination(playersRescuePos[2].position);
                        waveZombieAgent[i].isStopped = false;
                    }
                }

                // ±¸Á¶ Çï¸® ÄßÅÍ°¡ µµÂøÇßÀ» ¶§ ½ÇÇà
                if (rescueHelicopterAgent.remainingDistance <= rescueHelicopterAgent.stoppingDistance
                    && isHellcopterArrive == false)
                {
                    // Girl1(»ıÁ¸ÀÚ) ÀÌµ¿ ¸ñÀûÁö Å»Ãâ Æ÷ÀÎÆ®·Î ÁöÁ¤
                    rescuePlayerAgent[1].SetDestination(playersRescuePos[2].position);
                    rescuePlayers[1].transform.LookAt(playersRescuePos[2]);
                    // ÀÌµ¿ ½ÃÀÛ
                    rescuePlayerAgent[1].isStopped = false;
                    // ÀÌµ¿ ¾Ö´Ï¸ŞÀÌ¼Ç È°¼ºÈ­
                    if (rescuePlayersAnimator[1].GetBool("IsMove") == false)
                    {
                        // Girl1(»ıÁ¸ÀÚ) ÇÇ ÆÄÆ¼Å¬ ºñÈ°¼ºÈ­
                        rescuePlayerBloods[1].GetComponent<BloodParticleReactivator>().Stop();

                        rescuePlayersAnimator[1].SetBool("IsAttack", false);
                        rescuePlayersAnimator[1].SetBool("IsMove", true);
                    }

                    // Çï¸® ÄßÅÍ Âø·ú ½ÃÀÛ
                    if(rescueHelicopterObj.transform.position.y > 0)
                    {
                        rescueHelicopterObj.transform.position = new Vector3(rescueHelicopterObj.transform.position.x, rescueHelicopterObj.transform.position.y -0.1f, rescueHelicopterObj.transform.position.z);
                    }
                    // Çï¸® ÄßÅÍ Âø·ú ¿Ï·á
                    else
                    {
                        Debug.Log("Çï¸®ÄßÅÍ µµÂø");
                        isHellcopterArrive = true;
                    }    
                }

                // Girl1(»ıÁ¸ÀÚ)°¡ Å»Ãâ Æ÷ÀÎÆ®¿¡ µµÂø ÇßÀ» ¶§ ½ÇÇà
                if (isHellcopterArrive == true && isEscape == false
                    && rescuePlayerAgent[1].remainingDistance <= rescuePlayerAgent[1].stoppingDistance)
                {
                    Debug.Log("Girl1 Çï±â µµÂø");

                    rescuePlayerAgent[1].isStopped = true;
                    rescuePlayersAnimator[1].SetBool("IsMove", false);
                    rescuePlayersAnimator[1].SetBool("IsJoy", true);

                    // Ä«¸Ş¶ó º¸°í ¿ôµµ·Ï ¼³Á¤
                    rescuePlayers[1].transform.LookAt(mainCamera.transform.position);
                    isEscape = true;
                    playTime = 0;
                }
            }
        }
    }

    #endregion Method
}