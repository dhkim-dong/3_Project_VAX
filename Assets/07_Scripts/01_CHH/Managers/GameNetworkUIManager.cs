using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using static PhotonNetworkManager;
using static GameManager;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// 게임씬 포톤 서버 UI 연동 매니저
/// </summary>
public class GameNetworkUIManager : MonoBehaviourPunCallbacks
{
    #region Variable
    
    // 싱글톤이 할당될 static 변수
    private static GameNetworkUIManager instance;

    // 외부에서 싱글톤 오브젝트를 가져올때 사용할 프로퍼티
    public static GameNetworkUIManager GNUM
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (instance == null)
            {
                // 씬에서 GameNetworkUIManager 오브젝트를 찾아 할당
                instance = FindObjectOfType<GameNetworkUIManager>();
            }

            // 싱글톤 오브젝트를 반환
            return instance;
        }
    }

    public PhotonView PV;               // 포톤 연결정보
    public bool isUIActive;             // 게임씬 내 UI가 하나라도 활성화되어 있는지 여부

    [Header("PopUpIconPanel")]
    public GameObject infoPopUpPanel;
    public Image infoPopUpIcon;         // 플레이어 상태 정보 알림 아이콘
    public GameObject chatPopUpPanel;
    public Image chatPopUpIcon;         // 채팅 알림 아이콘
    private Color yellowColor = new Color(255, 230, 0, 255);    // 노란색

    [Header("PlayerRolePanel")]
    public GameObject playerRolePanel;  // 생존자/마피아 역할 알려주는 UI
    public Text roleContentText;        // 생존자/마피아 여부 알려주는 텍스트

    // 채팅UI
    [Header("ChattingPanel")]
    public GameObject chattingPanel;    // 채팅 UI
    public Text[] chatText;             // 채팅 텍스트필드 리스트
    public InputField chatInput;        // 채팅 입력값

    // 플레이어 상태 정보 UI
    [Header("PlayerStatePanel")]
    public GameObject playerStatePanel; // 플레이어 상태 정보 UI
    public Text[] nicknameText;         // 유저 닉네임 텍스트필드 리스트
    public Text[] zombieKillText;       // 좀비 킬 수 텍스트필드 리스트
    public Text[] playerKillText;       // 유저 킬 수 텍스트필드 리스트
    public Text[] scoreText;            // 점수 텍스트필드 리스트
    public Button[] voteButtons;        // 신고 버튼

    // 플레이어 상태 정보 UI
    [Header("VotePanel")]
    public Text voteTimeText;           // 투표 시간 텍스트
    public GameObject voteCheckPanel;   // 투표 확인 UI
    public Text voteNickNameText;       // 투표할 유저 닉네임 텍스트
    public GameObject voteErrorPanel;   // 투표 에러 UI 
    public Text voteErrorText;          // 투표할 유저 닉네임 텍스트
    public int selectVoteIdx;           // 선택한 투표 넘버
    public float voteWatingTime = 180;  // 투표 대기 시간
    public float voteTime;              // 신고는 3분당 1번 누를 수 있다.
    public bool isVoteTime;             // 투표 시간 체크 여부(3분당 투표 시간 돌아옴)
    public bool isVoted;                // 플레이어가 투표했는지 여부
    public bool isMasterVoteMsgSend;    // 마스터가 투표 결과 전달했는지 여부

    // 플레이어 상태 이미지에 적용할 스프라이트 이미지를 집합한 리스트
    // 0: Empty, 1 : 유저, 2: 투표결과 마피아 지목, 3 : 탈출 성공, 4 : 죽음
    public Sprite[] playerStateList;
    public Image[] playerStateImg;       // 플레이어들의 상태 이미지

    [Header("SettingPanel")]
    public GameObject settingPanel;// 게임 설정 UI

    private bool isInit;            // 초기화 여부

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

        // 채팅UI가 비활성화일 경우
        if (chattingPanel.activeSelf == false)
        {
            // 엔터키를 눌렀을 때 
            if (Input.GetKey(KeyCode.Return))
            {
                // 채팅UI를 활성화 시킨다.
                chattingPanel.SetActive(true);

                chatPopUpIcon.color = Color.white;
            }
        }
        // 채팅UI가 활성화 되었을 경우
        else
        {   
            // 엔터키를 눌렀을 때 
            if (Input.GetKey(KeyCode.Return))
            {
                // 채팅 입력창이 공백이 아니라면 보내기 기능
                if (chatInput.text != "")
                {
                    Send();
                }
            }
        }

        // Esc키를 눌렀을 때 
        if (Input.GetButtonDown("SettingPanel"))
        {
            // 채팅UI가 활성화일 때 마우스를 클릭하면
            if (chattingPanel.activeSelf)
            {
                // 채팅UI를 비활성화 한다.
                chattingPanel.SetActive(false);
            }
            // 설정창이 비활성화 상태라면 활성화로 변경한다
            else if(settingPanel.activeSelf == false)
            {
                settingPanel.SetActive(true);
            }
            // 설정창이 활성화 상태라면 비활성화로 변경한다
            else
            {
                settingPanel.SetActive(false);
            }
        }

        // 채팅UI가 비활성화일 경우
        if (playerStatePanel.activeSelf == false)
        {
            // 탭키를 눌렀을 때
            if (Input.GetButtonDown("PlayerStatePanel"))
            {
                // 플레이어 상태 정보창 갱신
                PlayerStateRenewal();

                // 채팅UI를 활성화 시킨다.
                playerStatePanel.SetActive(true);

                infoPopUpIcon.color = Color.white;
            }
        }
        else
        {
            // 탭키를 눌렀을 때
            if (Input.GetButtonDown("PlayerStatePanel"))
            {
                // 채팅UI를 비활성화 시킨다.
                playerStatePanel.SetActive(false);
            }
        }


        // 전체 UI창이 모두 비활성화 되어있어야 일시정지 풀림
        if (!chattingPanel.activeSelf && !playerStatePanel.activeSelf && !settingPanel.activeSelf 
            && !playerRolePanel.activeSelf)
        {
            isUIActive = false;
            //Time.timeScale = 1f;
            Cursor.visible = false;
        }
        // UI창이 하나라도 활성화 되어 있으면 일시정지
        else
        {
            isUIActive = true;
            //Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // 투표 시간 흐드도록 설정
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
        // 투표 시간 지나면 초기화
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
                Debug.Log("마스터 결과 발표");
                VoteMafiaCheck();
                isMasterVoteMsgSend = true;
            }
        }
        else if(voteTime > voteWatingTime + (voteWatingTime / 3) + 5)
        {
            voteTimeText.text = "00:00";
            voteErrorText.text = "투표 시간이 아닙니다.";

            isMasterVoteMsgSend = false;
            voteTime = 0;
            isVoteTime = false;
        }
    }

    #endregion Unity Method

    #region Init Method

    /// <summary>
    /// 초기화
    /// </summary>
    public void Init()
    {
        // 게임씬 입장 시 채팅 목록과 입력창 초기화
        for (int i = 0; i < chatText.Length; i++)
        {
            chatText[i].text = "";
        }
        chatInput.text = "";

        // 방 참가자 닉네임 리스트, 게임 준비 토글 초기화
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            nicknameText[i].text = "";
            zombieKillText[i].text = "";
            playerKillText[i].text = "";
            scoreText[i].text = "";
            playerStateImg[i].sprite = playerStateList[0];
        }

        // 전체 채팅으로 게임씬에 참가함을 유저들에게 알림
        PV.RPC("ChatRPC", RpcTarget.All, "<color=yellow>" + PNM.userNickName + "님이 참가하셨습니다</color>");

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
    /// 전체 채팅 보내기
    /// </summary>
    public void Send()
    {
        Debug.Log("채팅보내기");
        // 채팅 InputFiled에 입력한 텍스트를 전체 유저에게 보낸다.
        PV.RPC("ChatRPC", RpcTarget.All, PNM.userNickName + " : " + chatInput.text);

        // 유저에게 텍스트 전송 후 InputField를 초기화하여 새로운 메세지를 적을 수 있도록 함.
        chatInput.text = "";

        // 채팅을 보낸 이후 채팅 입력란에 포커스 유지
        chatInput.Select();
    }

    /// <summary>
    /// 생존자/마피아 역할 알려주는 메서드
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
            msg += "님은 좀비 바이러스에 감염되어 15분 후 반드시 죽게 됩니다.";
            msg += "\n생존자에게 들키지 않게 조심하세요!";
            msg += "\n그들은 위험분자를 살려두지 않아요.";
            msg += "\n가는길 쓸쓸하지 않게 동반자들을 만드세요.";
        }
        else
        {
            msg += "님은 생존자 입니다.";
            msg += "\n백신을 찾아서 탈출에 성공하시길 바랍니다.";
            msg += "\n생존에 위험한건 좀비만 있는건 아닙니다.";
        }

        roleContentText.text = msg;

        Debug.Log(msg);
    }

    /// <summary>
    /// 포톤 서버를 통한 전체 채팅 기능
    /// </summary>
    /// <param name="msg">채팅 문구</param>
    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    private void ChatRPC(string msg)
    {
        // 채팅 데이터 입력 여부
        bool isInput = false;

        // 채팅창의 전체 채팅 데이터 저장하는 텍스트 객체들 중 빈곳이 있는지 확인후 
        for (int i = 0; i < chatText.Length; i++)
        {
            // 빈 텍스트 객체에 채팅 데이터를 저장한다.
            if (chatText[i].text == "")
            {
                isInput = true;         // 채팅 데이터를 저장완료 표시로 변경
                chatText[i].text = msg; // 채팅 데이터를 저장
                break;                  // 반복문을 나가야 중복입력되지 않는다.
            }
        }

        // 채팅창의 채팅 데이터 리스트에 빈칸이 없다면 한칸씩 위로 올림
        if (!isInput)
        {
            // 현제 텍스트 내용을 1칸씩 위로 올림ㄴ
            for (int i = 1; i < chatText.Length; i++)
            {
                chatText[i - 1].text = chatText[i].text;
            }

            //  채팅창 맨 하단에 입력한 채팅 정보 입력
            chatText[chatText.Length - 1].text = msg;
        }

        chatPopUpIcon.color = yellowColor;
    }

    #endregion Chatting Method

    #region Player State Method

    /// <summary>
    /// 플레이어 상태 정보 갱신
    /// </summary>
/*    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    private void GameStateInfoUpdateRPC()
    {
        // 전체 유저 
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            // 유저 리스트 이름 갱신
            nicknameText[i].text = PhotonNetwork.PlayerList[i].NickName;
        }
    }*/

    #endregion Player State Method

    #region Method

    /// <summary>
    /// 방 떠나기
    /// </summary>
    public void LeaveRoom()
    {
        // 마스터 이동시 나머지 클라이언트도 같이 이동
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
        //PhotonNetwork.LoadLevel("02_LobbyScene");
        PhotonNetwork.LoadLevel("02_LobbyScene");
        Destroy(GameObject.Find("GameManager"));
        Destroy(GameObject.Find("GameNetworkUIManager"));
    }

    /// <summary>
    /// 플레이어 상태 정보창 갱신
    /// </summary>
    public void PlayerStateRenewal()
    {
        // 플레이어 상태 정보 갱신
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            PlayerInfo player = GM.playerInfos[i];

            Debug.Log($"PlayerStateRenewal nickname : {player.nickName}");
            Debug.Log($"PlayerStateRenewal nickname : {player.actorNum}");

            nicknameText[i].text = player.nickName;                     // 닉네임
            zombieKillText[i].text = player.zombieKillCount.ToString(); // 좀비킬
            playerKillText[i].text = player.playerKillCount.ToString(); // 플레이어킬
            scoreText[i].text = player.score.ToString();                // 점수

            // 플레이어가 죽었을 때 죽음 이미지
            if(player.isDie == true)
            {
                playerStateImg[i].sprite = playerStateList[4];
            }
            // 플레이어가 탈출에 성공했을 때 이미지
            else if (player.isEcapeSuccess == true)
            {
                playerStateImg[i].sprite = playerStateList[3];
            }
            // 플레이어가 투표에서 마피아로 지정되었을 때 이미지
            else if(player.isVoteMafiaSelect == true)
            {
                playerStateImg[i].sprite = playerStateList[2];
            }
            // 플레이어 기본 이미지
            else
            {
                playerStateImg[i].sprite = playerStateList[1];
            }
        }

        // 비어있는 플레이어 수
        int maxPlayer = 4;
        int emptyPlayer = maxPlayer - PhotonNetwork.PlayerList.Length;

        // 플레이어 상태 정보 빈칸 초기화
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
    /// 마피아 투표 버튼 눌렀을 때
    /// </summary>
    public void BtnVoteMafia(int playerIndex)
    {
        // 이미 투표했거나 투표시간이 아니면 메서드 종료
        if (isVoted == false && isVoteTime == true)
        {
            selectVoteIdx = playerIndex;
            voteNickNameText.text = PhotonNetwork.PlayerList[playerIndex].NickName;
            voteCheckPanel.SetActive(true);
        }
       else if(isVoteTime == false)
        {
            voteErrorText.text = "투표 시간이 아닙니다.";
            voteErrorPanel.SetActive(true);
        }
        else if (isVoted == true)
        {
            voteErrorText.text = "이미 투표하셨습니다.";
            voteErrorPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 투표 확인 버튼
    /// </summary>
    public void BtnVoteCheck()
    {
        // 투표함 투표 시간 지나고 나서 재투표 가능
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
    /// 투표 초기화
    /// </summary>
    private void VoteInit()
    {
        // 투표 전 이전 투표 기록 초기화
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
        Debug.Log("마스터 VoteMafiaCheck 실행");

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
                // 마피아 투표를 받았으면
                if (GM.playerInfos[i].voteMafiaCount > 0)
                {
                    // mafiaNum이 지정되지 않은 상태라면 현재 플레이어 번호로 지정
                    if (mafiaNum == -1)
                    {
                        mafiaNum = i;
                    }
                    // mafiaNum으로 지정된 플레이어와 현재 플레이어의 투표수가 같은 경우
                    else if (GM.playerInfos[mafiaNum].voteMafiaCount == GM.playerInfos[i].voteMafiaCount)
                    {
                        mafiaNum = -2;
                    }
                    // mafiaNum으로 지정된 플레이어보다 현재 플레이어의 투표수가 많으면 mafiaNum을 현재 플레이어로 지정
                    else if (GM.playerInfos[mafiaNum].voteMafiaCount < GM.playerInfos[i].voteMafiaCount)
                    {
                        mafiaNum = i;
                    }
                }
            }
        }

        Debug.Log("마피아 번호 : "+mafiaNum);

        // 투표 결과 동점으로 무효거나 아무도 투표하지 않았을 때
        if (mafiaNum == -1)
        {
            PV.RPC("ChatRPC", RpcTarget.All, "아무도 투표되지 않았습니다.");
            return;
        }
        else if (mafiaNum == -2)
        {
            PV.RPC("ChatRPC", RpcTarget.All, "투표 수가 동일하여 마피아 지정되지 않았습니다.");
            return;
        }

        // 해당 플레이어를 마피아로 지정함
        GM.playerInfos[mafiaNum].isVoteMafiaSelect = true;

        // 플레이어 상태 정보 
        GM.MasterSendPlayerInfo();
        PV.RPC("SetMafiaVoteRPC", RpcTarget.All, mafiaNum);
        PV.RPC("ChatRPC", RpcTarget.All, "<color=red>" + GM.playerInfos[mafiaNum].nickName + " 플레이어가 마피아로 지정되었습니다.</color>");
        PV.RPC("ChatRPC", RpcTarget.All, "생존자들은 " + GM.playerInfos[mafiaNum].nickName + "를 공격할 수 있습니다.");
    }

    /// <summary>
    /// 플레이어 상태 정보에서 지목당한 유저의 상태를 마피아로 변경
    /// 마피아로 지목당하면 다른 플레이어가 공격가능
    /// </summary>
    /// <param name="playerIdx">플레이어 번호</param>
    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    private void SetMafiaVoteRPC(int playerIdx)
    {
        Debug.Log("지정된 마피아 플레이어 상태 정보창 스프라이트 변경");

        
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
    /// 세팅창 닫기
    /// </summary>
    public void BtnSettingPanelClose()
    {
        settingPanel.SetActive(false);
    }

    #endregion Method
}
