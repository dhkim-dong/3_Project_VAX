using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using static PhotonNetworkManager;
using static GameManager;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// ���Ӿ� ���� ���� UI ���� �Ŵ���
/// </summary>
public class GameNetworkUIManager : MonoBehaviourPunCallbacks
{
    #region Variable
    
    // �̱����� �Ҵ�� static ����
    private static GameNetworkUIManager instance;

    // �ܺο��� �̱��� ������Ʈ�� �����ö� ����� ������Ƽ
    public static GameNetworkUIManager GNUM
    {
        get
        {
            // ���� �̱��� ������ ���� ������Ʈ�� �Ҵ���� �ʾҴٸ�
            if (instance == null)
            {
                // ������ GameNetworkUIManager ������Ʈ�� ã�� �Ҵ�
                instance = FindObjectOfType<GameNetworkUIManager>();
            }

            // �̱��� ������Ʈ�� ��ȯ
            return instance;
        }
    }

    public PhotonView PV;               // ���� ��������
    public bool isUIActive;             // ���Ӿ� �� UI�� �ϳ��� Ȱ��ȭ�Ǿ� �ִ��� ����

    [Header("PopUpIconPanel")]
    public GameObject infoPopUpPanel;
    public Image infoPopUpIcon;         // �÷��̾� ���� ���� �˸� ������
    public GameObject chatPopUpPanel;
    public Image chatPopUpIcon;         // ä�� �˸� ������
    private Color yellowColor = new Color(255, 230, 0, 255);    // �����

    [Header("PlayerRolePanel")]
    public GameObject playerRolePanel;  // ������/���Ǿ� ���� �˷��ִ� UI
    public Text roleContentText;        // ������/���Ǿ� ���� �˷��ִ� �ؽ�Ʈ

    // ä��UI
    [Header("ChattingPanel")]
    public GameObject chattingPanel;    // ä�� UI
    public Text[] chatText;             // ä�� �ؽ�Ʈ�ʵ� ����Ʈ
    public InputField chatInput;        // ä�� �Է°�

    // �÷��̾� ���� ���� UI
    [Header("PlayerStatePanel")]
    public GameObject playerStatePanel; // �÷��̾� ���� ���� UI
    public Text[] nicknameText;         // ���� �г��� �ؽ�Ʈ�ʵ� ����Ʈ
    public Text[] zombieKillText;       // ���� ų �� �ؽ�Ʈ�ʵ� ����Ʈ
    public Text[] playerKillText;       // ���� ų �� �ؽ�Ʈ�ʵ� ����Ʈ
    public Text[] scoreText;            // ���� �ؽ�Ʈ�ʵ� ����Ʈ
    public Button[] voteButtons;        // �Ű� ��ư

    // �÷��̾� ���� ���� UI
    [Header("VotePanel")]
    public Text voteTimeText;           // ��ǥ �ð� �ؽ�Ʈ
    public GameObject voteCheckPanel;   // ��ǥ Ȯ�� UI
    public Text voteNickNameText;       // ��ǥ�� ���� �г��� �ؽ�Ʈ
    public GameObject voteErrorPanel;   // ��ǥ ���� UI 
    public Text voteErrorText;          // ��ǥ�� ���� �г��� �ؽ�Ʈ
    public int selectVoteIdx;           // ������ ��ǥ �ѹ�
    public float voteWatingTime = 180;  // ��ǥ ��� �ð�
    public float voteTime;              // �Ű�� 3�д� 1�� ���� �� �ִ�.
    public bool isVoteTime;             // ��ǥ �ð� üũ ����(3�д� ��ǥ �ð� ���ƿ�)
    public bool isVoted;                // �÷��̾ ��ǥ�ߴ��� ����
    public bool isMasterVoteMsgSend;    // �����Ͱ� ��ǥ ��� �����ߴ��� ����

    // �÷��̾� ���� �̹����� ������ ��������Ʈ �̹����� ������ ����Ʈ
    // 0: Empty, 1 : ����, 2: ��ǥ��� ���Ǿ� ����, 3 : Ż�� ����, 4 : ����
    public Sprite[] playerStateList;
    public Image[] playerStateImg;       // �÷��̾���� ���� �̹���

    [Header("SettingPanel")]
    public GameObject settingPanel;// ���� ���� UI

    private bool isInit;            // �ʱ�ȭ ����

    #endregion Variable

    #region Unity Method

    private void Awake() 
    {
        //gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        isInit = false;

        Invoke("Init", 1f);
    }

    private void Update()
    {
        if (isInit == false) return;

        // ä��UI�� ��Ȱ��ȭ�� ���
        if (chattingPanel.activeSelf == false)
        {
            // ����Ű�� ������ �� 
            if (Input.GetKey(KeyCode.Return))
            {
                // ä��UI�� Ȱ��ȭ ��Ų��.
                chattingPanel.SetActive(true);

                chatPopUpIcon.color = Color.white;
            }
        }
        // ä��UI�� Ȱ��ȭ �Ǿ��� ���
        else
        {   
            // ����Ű�� ������ �� 
            if (Input.GetKey(KeyCode.Return))
            {
                // ä�� �Է�â�� ������ �ƴ϶�� ������ ���
                if (chatInput.text != "")
                {
                    Send();
                }
            }
        }

        // EscŰ�� ������ �� 
        if (Input.GetButtonDown("SettingPanel"))
        {
            // ä��UI�� Ȱ��ȭ�� �� ���콺�� Ŭ���ϸ�
            if (chattingPanel.activeSelf)
            {
                // ä��UI�� ��Ȱ��ȭ �Ѵ�.
                chattingPanel.SetActive(false);
            }
            // ����â�� ��Ȱ��ȭ ���¶�� Ȱ��ȭ�� �����Ѵ�
            else if(settingPanel.activeSelf == false)
            {
                settingPanel.SetActive(true);
            }
            // ����â�� Ȱ��ȭ ���¶�� ��Ȱ��ȭ�� �����Ѵ�
            else
            {
                settingPanel.SetActive(false);
            }
        }

        // ä��UI�� ��Ȱ��ȭ�� ���
        if (playerStatePanel.activeSelf == false)
        {
            // ��Ű�� ������ ��
            if (Input.GetButtonDown("PlayerStatePanel"))
            {
                // �÷��̾� ���� ����â ����
                PlayerStateRenewal();

                // ä��UI�� Ȱ��ȭ ��Ų��.
                playerStatePanel.SetActive(true);

                infoPopUpIcon.color = Color.white;
            }
        }
        else
        {
            // ��Ű�� ������ ��
            if (Input.GetButtonDown("PlayerStatePanel"))
            {
                // ä��UI�� ��Ȱ��ȭ ��Ų��.
                playerStatePanel.SetActive(false);
            }
        }


        // ��ü UIâ�� ��� ��Ȱ��ȭ �Ǿ��־�� �Ͻ����� Ǯ��
        if (!chattingPanel.activeSelf && !playerStatePanel.activeSelf && !settingPanel.activeSelf 
            && !playerRolePanel.activeSelf)
        {
            isUIActive = false;
            //Time.timeScale = 1f;
            Cursor.visible = false;
        }
        // UIâ�� �ϳ��� Ȱ��ȭ �Ǿ� ������ �Ͻ�����
        else
        {
            isUIActive = true;
            //Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // ��ǥ �ð� ��嵵�� ����
        voteTime += Time.deltaTime;

        if(voteTime < voteWatingTime)
        {
            int min = (int)(voteWatingTime - voteTime) / 60;
        ;   int sec = (int)(voteWatingTime - voteTime) % 60;
            voteTimeText.text = string.Format("{0:D2}:{1:D2}", min, sec);
            voteTimeText.color = Color.white;
        }
        else if(voteTime > voteWatingTime && voteTime < voteWatingTime + 1)
        {
            if(PNM.IsMaster() == true)
            {
                VoteInit();
            }
        }
        // ��ǥ �ð� ������ �ʱ�ȭ
        else if(voteTime >= voteWatingTime +1 && voteTime < voteWatingTime + (voteWatingTime / 3))
        {
            isVoteTime = true;
            isVoted = false;
            voteTimeText.color = yellowColor;

            int min = (int)(voteWatingTime + (voteWatingTime/3) - voteTime) / 60;
            int sec = (int)(voteWatingTime + (voteWatingTime / 3) - voteTime) % 60;
            voteTimeText.text = string.Format("{0:D2}:{1:D2}", min, sec);
        }
        else if(voteTime > voteWatingTime + (voteWatingTime / 3) && voteTime < voteWatingTime + (voteWatingTime / 3) + 5)
        {
            if(isMasterVoteMsgSend == false && PNM.IsMaster() == true)
            {
                Debug.Log("������ ��� ��ǥ");
                VoteMafiaCheck();
                isMasterVoteMsgSend = true;
            }
        }
        else if(voteTime > voteWatingTime + (voteWatingTime / 3) + 5)
        {
            voteTimeText.text = "00:00";
            voteErrorText.text = "��ǥ �ð��� �ƴմϴ�.";

            isMasterVoteMsgSend = false;
            voteTime = 0;
            isVoteTime = false;
        }
    }

    #endregion Unity Method

    #region Init Method

    /// <summary>
    /// �ʱ�ȭ
    /// </summary>
    public void Init()
    {
        // ���Ӿ� ���� �� ä�� ��ϰ� �Է�â �ʱ�ȭ
        for (int i = 0; i < chatText.Length; i++)
        {
            chatText[i].text = "";
        }
        chatInput.text = "";

        // �� ������ �г��� ����Ʈ, ���� �غ� ��� �ʱ�ȭ
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            nicknameText[i].text = "";
            zombieKillText[i].text = "";
            playerKillText[i].text = "";
            scoreText[i].text = "";
            playerStateImg[i].sprite = playerStateList[0];
        }

        // ��ü ä������ ���Ӿ��� �������� �����鿡�� �˸�
        PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + PNM.userNickName + "���� �����ϼ̽��ϴ�</color>");

        isVoteTime = false;
        isVoted = false;
        isMasterVoteMsgSend = false;
        voteTime = 0;

        infoPopUpPanel.SetActive(true);
        chatPopUpPanel.SetActive(true);

        isInit = true;
    }

    #endregion Init Method

    #region Chatting Method

    /// <summary>
    /// ��ü ä�� ������
    /// </summary>
    public void Send()
    {
        Debug.Log("ä�ú�����");
        // ä�� InputFiled�� �Է��� �ؽ�Ʈ�� ��ü �������� ������.
        PV.RPC("ChatRPC", RpcTarget.All, PNM.userNickName + " : " + chatInput.text);

        // �������� �ؽ�Ʈ ���� �� InputField�� �ʱ�ȭ�Ͽ� ���ο� �޼����� ���� �� �ֵ��� ��.
        chatInput.text = "";

        // ä���� ���� ���� ä�� �Է¶��� ��Ŀ�� ����
        chatInput.Select();
    }

    /// <summary>
    /// ������/���Ǿ� ���� �˷��ִ� �޼���
    /// </summary>
    public void PlayerRoleSend()
    {
        int playerIndex = -1;

        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (GM.playerInfos[i].actorNum == PNM.GetActorNumber())
            {
                playerIndex = i;
                break;
            }
        }

        string msg = PhotonNetwork.PlayerList[playerIndex].NickName;

        if(GM.playerInfos[playerIndex].isMafia == true)
        {
            msg += "���� ���� ���̷����� �����Ǿ� 15�� �� �ݵ�� �װ� �˴ϴ�.";
            msg += "\n�����ڿ��� ��Ű�� �ʰ� �����ϼ���!";
            msg += "\n�׵��� ������ڸ� ������� �ʾƿ�.";
            msg += "\n���±� �������� �ʰ� �����ڵ��� ���弼��.";
        }
        else
        {
            msg += "���� ������ �Դϴ�.";
            msg += "\n����� ã�Ƽ� Ż�⿡ �����Ͻñ� �ٶ��ϴ�.";
            msg += "\n������ �����Ѱ� ���� �ִ°� �ƴմϴ�.";
        }

        roleContentText.text = msg;

        Debug.Log(msg);
    }

    /// <summary>
    /// ���� ������ ���� ��ü ä�� ���
    /// </summary>
    /// <param name="msg">ä�� ����</param>
    [PunRPC] // RPC�� �÷��̾ �����ִ� �� ��� �ο����� �����Ѵ�
    private void ChatRPC(string msg)
    {
        // ä�� ������ �Է� ����
        bool isInput = false;

        // ä��â�� ��ü ä�� ������ �����ϴ� �ؽ�Ʈ ��ü�� �� ����� �ִ��� Ȯ���� 
        for (int i = 0; i < chatText.Length; i++)
        {
            // �� �ؽ�Ʈ ��ü�� ä�� �����͸� �����Ѵ�.
            if (chatText[i].text == "")
            {
                isInput = true;         // ä�� �����͸� ����Ϸ� ǥ�÷� ����
                chatText[i].text = msg; // ä�� �����͸� ����
                break;                  // �ݺ����� ������ �ߺ��Էµ��� �ʴ´�.
            }
        }

        // ä��â�� ä�� ������ ����Ʈ�� ��ĭ�� ���ٸ� ��ĭ�� ���� �ø�
        if (!isInput)
        {
            // ���� �ؽ�Ʈ ������ 1ĭ�� ���� �ø���
            for (int i = 1; i < chatText.Length; i++)
            {
                chatText[i - 1].text = chatText[i].text;
            }

            //  ä��â �� �ϴܿ� �Է��� ä�� ���� �Է�
            chatText[chatText.Length - 1].text = msg;
        }

        chatPopUpIcon.color = yellowColor;
    }

    #endregion Chatting Method

    #region Player State Method

    /// <summary>
    /// �÷��̾� ���� ���� ����
    /// </summary>
/*    [PunRPC] // RPC�� �÷��̾ �����ִ� �� ��� �ο����� �����Ѵ�
    private void GameStateInfoUpdateRPC()
    {
        // ��ü ���� 
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            // ���� ����Ʈ �̸� ����
            nicknameText[i].text = PhotonNetwork.PlayerList[i].NickName;
        }
    }*/

    #endregion Player State Method

    #region Method

    /// <summary>
    /// �� ������
    /// </summary>
    public void LeaveRoom()
    {
        // ������ �̵��� ������ Ŭ���̾�Ʈ�� ���� �̵�
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
        //PhotonNetwork.LoadLevel("02_LobbyScene");
        PhotonNetwork.LoadLevel("02_LobbyScene");
        Destroy(GameObject.Find("GameManager"));
        Destroy(GameObject.Find("GameNetworkUIManager"));
    }

    /// <summary>
    /// �÷��̾� ���� ����â ����
    /// </summary>
    public void PlayerStateRenewal()
    {
        // �÷��̾� ���� ���� ����
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            PlayerInfo player = GM.playerInfos[i];

            Debug.Log($"PlayerStateRenewal nickname : {player.nickName}");
            Debug.Log($"PlayerStateRenewal nickname : {player.actorNum}");

            nicknameText[i].text = player.nickName;                     // �г���
            zombieKillText[i].text = player.zombieKillCount.ToString(); // ����ų
            playerKillText[i].text = player.playerKillCount.ToString(); // �÷��̾�ų
            scoreText[i].text = player.score.ToString();                // ����

            // �÷��̾ �׾��� �� ���� �̹���
            if(player.isDie == true)
            {
                playerStateImg[i].sprite = playerStateList[4];
            }
            // �÷��̾ Ż�⿡ �������� �� �̹���
            else if (player.isEcapeSuccess == true)
            {
                playerStateImg[i].sprite = playerStateList[3];
            }
            // �÷��̾ ��ǥ���� ���ǾƷ� �����Ǿ��� �� �̹���
            else if(player.isVoteMafiaSelect == true)
            {
                playerStateImg[i].sprite = playerStateList[2];
            }
            // �÷��̾� �⺻ �̹���
            else
            {
                playerStateImg[i].sprite = playerStateList[1];
            }
        }

        // ����ִ� �÷��̾� ��
        int maxPlayer = 4;
        int emptyPlayer = maxPlayer - PhotonNetwork.PlayerList.Length;

        // �÷��̾� ���� ���� ��ĭ �ʱ�ȭ
        if (emptyPlayer > 0)
        {
            for(int i = maxPlayer -1; i >= PhotonNetwork.PlayerList.Length; i--)
            {
                nicknameText[i].text = "";
                zombieKillText[i].text = "";
                playerKillText[i].text = "";
                scoreText[i].text = "";
                playerStateImg[i].sprite = playerStateList[0];
                voteButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void BtnClosePanel(GameObject obj)
    {
        if(obj.name == "VoteCancelBtn")
        {
            voteCheckPanel.SetActive(false);
        }
        else if (obj.name == "ErrorCloseBtn")
        {
            voteErrorPanel.SetActive(false);
        }
        else if(obj.name == "RoleCloseBtn")
        {
            playerRolePanel.SetActive(false);
        }
    }

    /// <summary>
    /// ���Ǿ� ��ǥ ��ư ������ ��
    /// </summary>
    public void BtnVoteMafia(int playerIndex)
    {
        // �̹� ��ǥ�߰ų� ��ǥ�ð��� �ƴϸ� �޼��� ����
        if (isVoted == false && isVoteTime == true)
        {
            selectVoteIdx = playerIndex;
            voteNickNameText.text = PhotonNetwork.PlayerList[playerIndex].NickName;
            voteCheckPanel.SetActive(true);
        }
       else if(isVoteTime == false)
        {
            voteErrorText.text = "��ǥ �ð��� �ƴմϴ�.";
            voteErrorPanel.SetActive(true);
        }
        else if (isVoted == true)
        {
            voteErrorText.text = "�̹� ��ǥ�ϼ̽��ϴ�.";
            voteErrorPanel.SetActive(true);
        }
    }

    /// <summary>
    /// ��ǥ Ȯ�� ��ư
    /// </summary>
    public void BtnVoteCheck()
    {
        // ��ǥ�� ��ǥ �ð� ������ ���� ����ǥ ����
        isVoted = true;
        voteCheckPanel.SetActive(false);
        PV.RPC("VoteMafiaRPC", RpcTarget.MasterClient, selectVoteIdx);
    }

    [PunRPC]
    public void VoteMafiaRPC(int playerIndex)
    {
        GM.playerInfos[playerIndex].voteMafiaCount++;
    }

    /// <summary>
    /// ��ǥ �ʱ�ȭ
    /// </summary>
    private void VoteInit()
    {
        // ��ǥ �� ���� ��ǥ ��� �ʱ�ȭ
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            GM.playerInfos[i].voteMafiaCount = 0;

            if (GM.playerInfos[i].isVoteMafiaSelect == true)
            {
                GM.playerInfos[i].isVoteMafiaSelect = false;
            }
        }

        GM.MasterSendPlayerInfo();
    }

    private void VoteMafiaCheck()
    {
        Debug.Log("������ VoteMafiaCheck ����");

        int mafiaNum = -1;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Debug.Log($"{GM.playerInfos[i]} : {GM.playerInfos[i].voteMafiaCount}");

            if (i == 0)
            {
                if (GM.playerInfos[i].voteMafiaCount > 0)
                {
                    mafiaNum = i;
                }
            }
            else
            {
                // ���Ǿ� ��ǥ�� �޾�����
                if (GM.playerInfos[i].voteMafiaCount > 0)
                {
                    // mafiaNum�� �������� ���� ���¶�� ���� �÷��̾� ��ȣ�� ����
                    if (mafiaNum == -1)
                    {
                        mafiaNum = i;
                    }
                    // mafiaNum���� ������ �÷��̾�� ���� �÷��̾��� ��ǥ���� ���� ���
                    else if (GM.playerInfos[mafiaNum].voteMafiaCount == GM.playerInfos[i].voteMafiaCount)
                    {
                        mafiaNum = -2;
                    }
                    // mafiaNum���� ������ �÷��̾�� ���� �÷��̾��� ��ǥ���� ������ mafiaNum�� ���� �÷��̾�� ����
                    else if (GM.playerInfos[mafiaNum].voteMafiaCount < GM.playerInfos[i].voteMafiaCount)
                    {
                        mafiaNum = i;
                    }
                }
            }
        }

        Debug.Log("���Ǿ� ��ȣ : "+mafiaNum);

        // ��ǥ ��� �������� ��ȿ�ų� �ƹ��� ��ǥ���� �ʾ��� ��
        if (mafiaNum == -1)
        {
            PV.RPC("ChatRPC", RpcTarget.All, "�ƹ��� ��ǥ���� �ʾҽ��ϴ�.");
            return;
        }
        else if (mafiaNum == -2)
        {
            PV.RPC("ChatRPC", RpcTarget.All, "��ǥ ���� �����Ͽ� ���Ǿ� �������� �ʾҽ��ϴ�.");
            return;
        }

        // �ش� �÷��̾ ���ǾƷ� ������
        GM.playerInfos[mafiaNum].isVoteMafiaSelect = true;

        // �÷��̾� ���� ���� 
        GM.MasterSendPlayerInfo();
        PV.RPC("SetMafiaVoteRPC", RpcTarget.All, mafiaNum);
        PV.RPC("ChatRPC", RpcTarget.All, "<color=red>" + GM.playerInfos[mafiaNum].nickName + " �÷��̾ ���ǾƷ� �����Ǿ����ϴ�.</color>");
        PV.RPC("ChatRPC", RpcTarget.All, "�����ڵ��� " + GM.playerInfos[mafiaNum].nickName + "�� ������ �� �ֽ��ϴ�.");
    }

    /// <summary>
    /// �÷��̾� ���� �������� ������� ������ ���¸� ���ǾƷ� ����
    /// ���ǾƷ� ������ϸ� �ٸ� �÷��̾ ���ݰ���
    /// </summary>
    /// <param name="playerIdx">�÷��̾� ��ȣ</param>
    [PunRPC] // RPC�� �÷��̾ �����ִ� �� ��� �ο����� �����Ѵ�
    private void SetMafiaVoteRPC(int playerIdx)
    {
        Debug.Log("������ ���Ǿ� �÷��̾� ���� ����â ��������Ʈ ����");

        
        for(int i=0;i< PhotonNetwork.PlayerList.Length; i++)
        {
            playerStateImg[i].sprite = playerStateList[0];
        }

        playerStateImg[playerIdx].sprite = playerStateList[2];

        if(playerStatePanel.activeSelf == false)
        {
            infoPopUpIcon.color= yellowColor;
        }
    }

    /// <summary>
    /// ����â �ݱ�
    /// </summary>
    public void BtnSettingPanelClose()
    {
        settingPanel.SetActive(false);
    }

    #endregion Method
}
