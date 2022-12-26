using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PhotonNetworkManager;
using static GameNetworkUIManager;
using static GameManager;

// ���� ������ �����ϴ� ���� �Ŵ���
public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Variable

    // �̱����� �Ҵ�� static ����
    private static GameManager instance;

    // �ܺο��� �̱��� ������Ʈ�� �����ö� ����� ������Ƽ
    public static GameManager GM
    {
        get
        {
            // ���� �̱��� ������ ���� ������Ʈ�� �Ҵ���� �ʾҴٸ�
            if (instance == null)
            {
                // ������ GameManager ������Ʈ�� ã�� �Ҵ�
                instance = FindObjectOfType<GameManager>();
            }

            // �̱��� ������Ʈ�� ��ȯ
            return instance;
        }
    }

    // �÷��̾� ���� ����
    public enum PlayerGameState 
    { 
        NONE,       // �÷��̾� ���� �ȵ�
        INIT,       // �÷��̾� ������
        ALIVE,      // �÷��̾� �������
        REMOVE,     // �÷��̾� ���� ������� ������ ����
        DAMAGE,     // �÷��̾� ������ ����
        DIE         // �÷��̾� ����
    }

    // ���� ���̺� �ܰ�
    public enum ZombieWaveStep
    {
        FirstDay,   // ù¶��
        SECOND_DAY, // ��¶��
        THIRD_DAY,  // ��¶��
        END_DAY     // ��������
    }

    //private GameNetworkUIManager gameNetworkUIManager;

    public PhotonView PV;                   // ���� ��������
    public GameObject[] playerCharacters;   // ���� �÷��̾�� ����� ĳ���� ������Ʈ ����Ʈ
    public Transform[] playerSpwnPos;      // �÷��̾ ������ ��ġ ����Ʈ

    public List<PlayerInfo> playerInfos;    // �÷��̾� ������ �����ϴ� Ŭ����
    public List<GameObject> playerPrefabs = new List<GameObject>();

    public PlayerGameState playerGameState; // �÷��̾� ���� ����
    public ZombieWaveStep zombieWaveStep;   // ���� ���̺� �ܰ�
    public int mafiaNum;                    // ���ǾƷ� ������ �÷��̾� ������ȣ
    public bool isDie;                      // �÷��̾� ���� ����
    public bool isMafiaWin;                 // ���Ǿ� �¸� ����
    public bool isEscape;                   // Ż�� ����
    public bool isGameveEnd;                // ������ ����Ǿ����� ����

    // �׽�Ʈ Ȯ�ο�
    public string jsonPlayerInfo;     

    #endregion Variable

    #region Unity Method

    // �ʱ�ȭ
    private void Awake()
    {
        // �̱��� ��ü 1���� �����ϵ��� ����
        if (null == instance)
        {
            instance = this;
            // ���� �̵��ϴ��� ������� �ʵ��� ����
            DontDestroyOnLoad(this);

            Debug.Log("DontDestroyOnLoad");
        }
        else Destroy(this);
    }

    // ���� �ʱ�ȭ
    private IEnumerator Start()
    {
        // ���� �÷��̾ ���������� üũ
        if (PNM.IsMaster())
        {
            // �÷��̾� ���� �ʱ�ȭ
            MasterInitPlayerInfo();
        }

        // false���  �ݺ�üũ�ϸ鼭 ��� �÷��̾ �� �̵��� �� ������ ��ٸ�.
        while (PNM.AllhasTag("ReceivePlayerInfo", "True") == false)
        {
            yield return null;
        }

        // ���Ӿ� ���۽� �÷��̾� ���� �� �ʱ�ȭ �۾�
        yield return Loading();

        // �÷��̾� ���� ������� �������� ����
        playerGameState = PlayerGameState.ALIVE;

        // ���� ���̺� �ܰ� = ù��°��
        zombieWaveStep = ZombieWaveStep.FirstDay;

        // ���Ǿ� �¸����� �ʱ�ȭ
        isMafiaWin = false;

        // Ż�⿩�� �ʱ�ȭ
        isEscape = false;

        // �������� �ʱ�ȭ
        isDie = false;

        // ���� ���� ���� �ʱ�ȭ
        isGameveEnd = false;
    }

    // �� �����Ӹ��� �ݺ�
    private void Update()
    {
        // �÷��̾ ���������� ������ �� ���� ���� ���������� �������Ῡ�� üũ
        if(playerGameState == PlayerGameState.ALIVE && isGameveEnd == false)
        {
            // ���� ���� ���� üũ
            GameEndCheck();

            // �������� �Ǿ����� ����
            GameEnd();
        }
    }

    #endregion Unity Method

    #region Method

    /// <summary>
    /// ���ӽ��� �� �÷��̾� ���� �� �ʱ�ȭ ���
    /// </summary>
    /// <returns></returns>
    private IEnumerator Loading()
    {
        Debug.Log("Loading");

        // ���Ӿ��� �ε� �Ǿ����� ����
        PNM.SetTag("LoadGameScene", "True");

        // AllhasTag : �� ��ü �ο��� ���� ������ �̵� �Ǿ��� ��� true ��ȯ
        // false���  �ݺ�üũ�ϸ鼭 ��� �÷��̾ �� �̵��� �� ������ ��ٸ�.
        while (PNM.AllhasTag("LoadGameScene", "True") == false)
        {
            yield return null;
        }

        Debug.Log("��ü ���� �� �̵� �Ϸ�.");

        // �÷��̾� ����
        PlayerSpawn();

        // ��� �÷��̾ ������ �Ǿ��� ������ ��ٸ�
        while (PNM.AllhasTag("LoadPlayer", "True") == false)
        {
            yield return null;
        }
    }

    /// <summary>
    /// �÷��̾� ������Ʈ ����
    /// </summary>
    private void PlayerSpawn()
    {
        // Resources ������ ���� �����ȿ� ������ ������Ʈ�� �����ϹǷ� ��μ���
        string path = "Player/";

        // ���� �÷��̾��� �����ѹ��� �����´�.
        int playerIdx = PNM.GetActorNumber() - 1;

        // ���� �÷��̾��� ���� ĳ���� ��ȣ�� �����´�.
        int myRandCharacterNum = playerInfos[playerIdx].randCharacterNum;
        path += playerCharacters[myRandCharacterNum].name;

        // �÷��̾� ������Ʈ ����Ʈ�� ������ �÷��̾� �߰�
        playerPrefabs.Add(PhotonNetwork.Instantiate(path, playerSpwnPos[playerIdx].transform.position, Quaternion.identity));

        // SetTag : Ű�� �±װ� ����
        // �÷��̾ ������ �Ϸ���� �ǹ�
        PNM.SetTag("LoadPlayer", "True");
    }

    /// <summary>
    /// �����͸� ȣ�Ⱑ���� �޼���
    /// �÷��̾� ���� �ʱ�ȭ
    /// </summary>
    private void MasterInitPlayerInfo()
    {
        Debug.Log("MasterInitPlayerInfo / �����͸� ����");

        // ���Ǿ� ���� ����
        mafiaNum = Random.Range(0, PhotonNetwork.PlayerList.Length);

        // ���ӽ��۽� �ʱ�ȭ
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            // �÷��̾� ���� ����
            Player player = PhotonNetwork.PlayerList[i];

            // ���� ĳ���� ����
            int randCharacterNum = Random.Range(0, playerCharacters.Length-1);

            // ������/���Ǿ� ����
            bool isMafia = false;

            // ���ǾƷ� ������ ���ٸ� true�� �ٲ��ش�.
            if(i== mafiaNum)
            {
                isMafia = true;
            }

            // PlayerInfo : �÷��̾� ���� �ʱ�ȭ
            // playerInfos.Add : �ʱ�ȭ�� �÷��̾� ������ ����Ʈ�� �߰��Ѵ�.
            playerInfos.Add(new PlayerInfo(player.NickName, player.ActorNumber, randCharacterNum, isMafia));
        }

        playerGameState = PlayerGameState.INIT;

        // PlayerInfo ����Ʈ Jsonȭ ���Ѽ� ������
        MasterSendPlayerInfo();
    }

    /// <summary>
    /// OnPlayerLeftRoom���� ���� ������� �÷��̾� ����
    /// </summary>
    /// <param name="actorNum">�÷��̾� ���� �ĺ���</param>
    private void MasterRemovePlayerInfo(int actorNum)
    {
        // ������ �÷��̾�� ���� �ĺ��� ��ȣ�� ���� �÷��̾� ������ ã�´�.
        PlayerInfo playerInfo = playerInfos.Find(player => player.actorNum == actorNum);

        // �÷��̾� ���� ����Ʈ���� �ش� ���� ����
        playerInfos.Remove(playerInfo);

        playerGameState = PlayerGameState.REMOVE;

        MasterSendPlayerInfo();
    }

    /// <summary>
    /// �����ʹ� PlayerInfo ���� �� Ŭ���̾�Ʈ���� ������ �����Ѵ�.
    /// </summary>
    /// <param name="playergameState"></param>
    public void MasterSendPlayerInfo()
    {
        if (PNM.IsMaster() == false) return;

        // �÷��̾��� �ĺ��� ��ȣ �� ����
        playerInfos.Sort((p1, p2) => p1.actorNum.CompareTo(p2.actorNum));

        // �÷��̾� ���� ����Ʈ�� JSON �������� ����ȭ �Ѵ�.
        string jdata = JsonUtility.ToJson(new Serialization<PlayerInfo>(playerInfos));

        Debug.Log("Json Data : " + jdata);

        // Ŭ���̾�Ʈ�鿡�� �÷��̾� ���� ����Ʈ �����͸� �����Ѵ�.
        PV.RPC("OtherReceivePlayerInfoRPC", RpcTarget.All, jdata);
    }

    /// <summary>
    /// ���� ������ ���� �÷��̾��� ���� ����Ʈ�� �޴´�.
    /// </summary>
    /// <param name="playerGameState">�÷��̾� ���� ����</param>
    /// <param name="jdata">JSON �������� ����ȭ�� �÷��̾��� ���� ����Ʈ</param>
    [PunRPC]
    private void OtherReceivePlayerInfoRPC(string jdata)
    {
        // Ŭ���̾�Ʈ�� �����Ϳ��Լ� �÷��̾� ������ ����ִ� ����Ʈ playerInfos�� �޾ƿ����� �ǹ�.
        PNM.SetTag("ReceivePlayerInfo", "True");

        // �����Ϳ��� �޾ƿ� playerInfos ������ ������ ���� ���
        if (jdata == null || jdata == "")
        {
            Debug.Log("OtherReceivePlayerInfoRPC / playerInfos Json ���� ���޾ƿ���.");
            return;
        }

        jsonPlayerInfo = jdata;

        // �ٸ� ����� PlayerInfo �ޱ�
        playerInfos = JsonUtility.FromJson<Serialization<PlayerInfo>>(jdata).playerInfos;

        // �÷��̾� ���� ������ Ȯ��
        for (int i = 0; i < playerInfos.Count; i++)
        {
            Debug.Log($"NickName : {playerInfos[i].nickName}");
            Debug.Log($"ActorNum : {playerInfos[i].actorNum}");
            Debug.Log($"RandCharacterNum : {playerInfos[i].randCharacterNum}");
            Debug.Log($"IsMafia : {playerInfos[i].isMafia}");
            Debug.Log("GNUM.PlayerRoleSend / ������/���Ǿ� ���� �˷��ִ� �޼���");

            // ���� ������ �� ���� �޼��� �ʱ�ȭ
            // playerIndex�� ���� �÷��̾� ���� �ѹ��� ���ƾ� ����
            GNUM.PlayerRoleSend();
        }

        // �÷��̾� ���� UI ����
        GNUM.PlayerStateRenewal();
    }

    /// <summary>
    /// ���� ���� üũ �޼���
    /// </summary>
    public void GameEndCheck()
    {
        isMafiaWin = false;      // ���Ǿ� �¸� ����

        // ������ ���� : ��ü �ο� - ���Ǿ� 1��
        int survivorCount = PhotonNetwork.PlayerList.Length  -1;     

        // �÷��̾� ����ŭ �ݺ�
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            // ������ ������ ���� �÷��̾ �׾����� üũ
            if (playerInfos[i].isDie == true && playerInfos[i].isMafia == false)
            {
                survivorCount--;
            }
        }

        // ���Ǿư� ��� ������ �����ڰ� ��� �׾����� ���Ǿ� �¸�
        if (playerInfos[mafiaNum].isDie == false && survivorCount == 0)
        {
            // ���Ǿ� �¸�
            isMafiaWin = true;

            // ���� �����
            isGameveEnd = true;
        }
        // �����ڰ� Ż�⿡ �����ߴٸ� ������ �¸�
        else if(isEscape == true)
        {
            // ���� �����
            isGameveEnd = true;

            // ������ ���� : ��ü �ο� - ���Ǿ� 1��
            survivorCount = PhotonNetwork.PlayerList.Length - 1;

            // EndDAy
            for (int i = 0; i < survivorCount; i++)
            {
                if (playerInfos[i].isDie == false  && playerInfos[i].isEcapeSuccess == true)
                {
                    isGameveEnd = true;
                    break;
                }
            }
        }
        // ���� ���̺갡 �������� �� ������ ���� ����
        // ���Ǿ�, ������ ��� ���� �й�
        else if(zombieWaveStep == ZombieWaveStep.END_DAY)
        {
            // ���� �����
            isGameveEnd = true;
        }
    }

    /// <summary>
    /// ���� ����� �������� UI �ѱ�
    /// </summary>
    public void GameEnd()
    {
        // ������ ����Ǿ��ٸ� ����
        if(isGameveEnd == true)
        {
            // ���Ǿ� �̰�ٸ�
            if (isMafiaWin == true)
            {
                // GameNetworkUIManger�� ���ڸ� ���ǾƷ� ����
                for(int i=0;i< PhotonNetwork.PlayerList.Length; i++)
                {
                    if(playerInfos[i].isMafia == true)
                    {
                        Debug.Log($"{playerInfos[i].nickName} / ���Ǿ� �¸�!");
                        break;
                    }
                }
            }
            // �����ڰ� �̰�ٸ�
            else if (isEscape == true)
            {
                // ������ �̸� ����
                string playerNames = "";
                
                // ������ �˻�
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    // ������ �����̸鼭 Ż�⿡ ������ �÷��̾� ã��
                    if (playerInfos[i].isMafia == false && playerInfos[i].isEcapeSuccess == true)
                    {
                        // Ż�⿡ ������ �÷��̾�� ����
                        playerNames += playerInfos[i].nickName + " / ";
                    }
                }

                Debug.Log($"{playerNames}������ �¸�!");
            }
            // ������/���Ǿ� ��� ���� �й�
            else if (isMafiaWin == false && isEscape == false)
            {
                Debug.Log($"������/���Ǿ� ��� ���ӿ� �й��Ͽ����ϴ�.");
            }
        }
    }

    #endregion Method

    #region Photon Method

    // �ֱ������� �ڵ� ����Ǵ�, ����ȭ �޼���
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // ���� ������Ʈ��� ���� �κ��� �����
        if (stream.IsWriting)
        {
            // ��Ʈ��ũ�� ���� score ���� ������
            stream.SendNext(isGameveEnd);
        }
        // ����Ʈ ������Ʈ��� �б� �κ��� �����  
        else
        {
            // ��Ʈ��ũ�� ���� �������Ῡ�� �� �ޱ�
            isGameveEnd = (bool)stream.ReceiveNext();
        }
    }

    /// <summary>
    /// �÷��̾ ���� �����ٸ�
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // �����Ͱ� ������ �ٲ� �����Ͱ� ȣ��Ǽ� ����
        if (PNM.IsMaster())
        {
            MasterRemovePlayerInfo(otherPlayer.ActorNumber);
        }
    }

    #endregion Photon Method
}
