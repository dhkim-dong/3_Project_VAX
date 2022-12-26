using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static PhotonNetworkManager;
using static System.Net.Mime.MediaTypeNames;
using Text = UnityEngine.UI.Text;

/// <summary>
/// 포톤 네트워크 접속 관련 기능을 다루는 스크립트
/// </summary>

public class MainMenuConnectManager : MonoBehaviourPunCallbacks
{
    #region Variable

    public PhotonView PV;           // 포톤 연결정보

    // 로그인 후 로비 대기실
    [Header("LobbyPanel")]
    public GameObject lobbyPanel;       // 로비 UI
    public Text nickNameText;           // 닉네임 텍스트
    public Text lobbyInfoText;          // 로비 정보 (0로비 / 0접속)
    public InputField roomInput;        // 방이름
    public Button[] cellBtn;            // 방버튼 리스트 
    public Button backBtn;              // 방 리스트 Back 버튼
    public Button nextBtn;              // 방 리스트 Next 버튼

    // 방 입장
    [Header("RoomPanel")]
    public GameObject roomPanel;        // 방 UI
    public GameObject roomFrame;        // 방 프레임 빨강
    public Text roomName;               // 방에 입장한 유저 닉네임 순서대로 배열
    public Text roomInfoText;           // 방 정보 (방이름 / 0명 / 0최대)
    public Text[] nicknameText;         // 방에 모인 회원 닉네임
    public GameObject[] readyToggle;    // 게임 준비 여부 체크 이미지
    public Text gameStartText;		    // 게임 시작 버튼 텍스트 (방장 : 게임시작, 그 외 : 게임 준비)
    public Text[] chatText;             // 채팅 리스트
    public InputField chatInput;        // 채팅 입력값

    // 설정창
    [Header("SettingPanel")]
    public GameObject settingPanel;     // 설정창 UI

    // 포톤 작동 상태
    [Header("StatusPanel")]
    public Text statusText;             // 포톤 작동 상태 출력되는 텍스트

    // 방 리스트 저장
    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;

    #endregion Variable

    #region Unity Method

    /// <summary>
    /// 게임 실행시 포톤 기본 설정
    /// </summary>
    private void Awake()
    {
        // true : 마스터가 씬이동시 기존 유저(클라이언트)들도 같이 이동됨.
        // flase : 마스터만 이동
        PhotonNetwork.AutomaticallySyncScene = true;

        // 클라이언트가 얼마나 자주 패키지를 전송 할지 설정 / 포톤으로 주고 받는 통신간격
        PhotonNetwork.SendRate = 40;

        // 데이터를 받아쓰는 빈도
        PhotonNetwork.SerializationRate = 20;
    }

    // 컴포넌트 초기화
    private void Start()
    {
        // 포톤 연결이 되어있을 때 = 게임씬에서 메인메뉴씬으로 이동되었을 때
        if (PhotonNetwork.IsConnected == true)
        {
            roomPanel.SetActive(false);
            roomFrame.SetActive(false);
            settingPanel.SetActive(false);
        }
        else
        {
            // 환영문구 초기화
            nickNameText.text = "";
        }
        // 방이름 입력란 초기화
        roomInput.text = "";
    }

    /// <summary>
    /// 매 프레임마다 반복 실행
    /// </summary>
    private void Update()
    {
        // 포톤 작동 상태 출력(정상 연결, 대기실 이동, 방참가 등)
        statusText.text = PhotonNetwork.NetworkClientState.ToString();

        // 로비 정보 갱신
        lobbyInfoText.text = "LOBBY : " + (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + " / CONNECT : " + PhotonNetwork.CountOfPlayers;

        // 방에 접속 했다면
        if (PhotonNetwork.InRoom)
        {
            // 새로 들어온 유저는 이전 유저의 데이터가 없기 때문에
            // 이전의 유저가 게임 준비를 눌렀더라도 화면에 표시되지 않는다.
            if (PNM.IsMaster())
            {
                // 전체 유저 정보를 가지고 있는 방장=마스터가 다른 클라이언트의 게임 준비 상태 UI를 업데이트 시켜준다. 
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    PV.RPC("GameStateInfoUpdateRPC", RpcTarget.All, i, readyToggle[i].activeSelf);
                }
            }

            // 엔터키를 눌렀을 때 채팅 보내기
            if (Input.GetKey(KeyCode.Return))
            {
                // 채팅 입력란이 공백이 아닐경우 엔터를 누르면 채팅을 보낸다
                if (chatInput.text != "")
                {
                    Send();
                }
            }

            // 방 정보 갱신
            RoomRenewal();

            // 방 오른쪽 패널 유저 리스트와 게임준비 토글키 갱신
            PV.RPC("RoomRenewalRPC", RpcTarget.All);
        }


        // Esc 버튼 누를시 설정창 활성화/비활성화
        if (Input.GetButtonDown("SettingPanel"))
        {
            // 설정창이 비활성화 상태라면 활성화로 변경한다
            if (settingPanel.activeSelf == false)
            {
                settingPanel.SetActive(true);
            }
            // 설정창이 활성화 상태라면 비활성화로 변경한다
            else
            {
                settingPanel.SetActive(false);
            }
        }
    }

    #endregion Unity Method

    #region Server Connect

    /// <summary>
    /// PUN 이 준비되었을 때 호출 되어 마스터서버 접속하여 방이 있다면 방에 참가
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    /// <summary>
    /// 로비에 참가
    /// </summary>
    public override void OnJoinedLobby()
    {
        // 로비 UI창 활성화
        lobbyPanel.SetActive(true);

        // 방 UI창 비활성화
        roomPanel.SetActive(false);
        roomFrame.SetActive(false);

        // 포톤 현재 유저 닉네임 설정
        PhotonNetwork.LocalPlayer.NickName = PNM.userNickName;

        // 환영문구 설정
        nickNameText.text = "-< " + PhotonNetwork.LocalPlayer.NickName + " >-";

        // 방 리스트 초기화
        myList.Clear();
    }

    /// <summary>
    /// 포톤 연결 해제
    /// </summary>
    public void Disconnect()
    {
        Debug.Log("로그아웃");
        PhotonNetwork.Disconnect();
        Destroy(GameObject.Find("PhotonNetworkManager").gameObject);
        SceneManager.LoadScene("01_MainMenuScene");
    }

    /// <summary>
    /// 로비 UI와 룸 UI 모두 비활성화
    /// </summary>
    /// <param name="cause"></param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        // 로비 UI창 비활성화
        lobbyPanel.SetActive(false);

        // 방 UI창 비활성화
        roomPanel.SetActive(false);
        roomFrame.SetActive(false);
    }

    #endregion Server Connect

    #region Room Renewal

    /// <summary>
    /// ◀버튼 -2 , ▶버튼 -1 , 셀 숫자
    /// </summary>
    /// <param name="num"></param>
    public void MyListClick(int num)
    {
        // 방리스트 이전 버튼
        if (num == -2) --currentPage;
        // 방 리스트 ㅏ음 버튼
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name);

        // 이전, 다음 버튼으로 대기실 방 리스트 UI 갱신
        MyListRenewal();
    }

    /// <summary>
    /// 이전, 다음 버튼으로 대기실 방 리스트 UI 갱신
    /// 페이지 나눔하여 보여지는 방 리스트를 갱신한다.
    /// </summary>
    void MyListRenewal()
    {
        // 최대페이지
        maxPage = (myList.Count % cellBtn.Length == 0) ? myList.Count / cellBtn.Length : myList.Count / cellBtn.Length + 1;

        // 이전, 다음버튼
        backBtn.interactable = (currentPage <= 1) ? false : true;
        nextBtn.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * cellBtn.Length;
        for (int i = 0; i < cellBtn.Length; i++)
        {
            cellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
            cellBtn[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            cellBtn[i].transform.GetChild(2).GetComponent<Text>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
    }

    /// <summary>
    /// 대기실 전체 방 리스트 갱신
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 전체 방 개수
        int roomCount = roomList.Count;

        // 전체 방 리스트에서 삭제되지 않은 방 확인
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }

        // 페이지 나눔하여 보여지는 방 리스트를 갱신한다.
        MyListRenewal();
    }
    #endregion Room Renewal

    #region Room Connect

    /// <summary>
    /// 방 생성
    /// </summary>
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(roomInput.text == "" ? "Room" + Random.Range(0, 100) : roomInput.text, new RoomOptions { MaxPlayers = (byte)PNM.MAX_PLAYER });
    }

    /// <summary>
    /// 랜덤방 참가
    /// </summary>
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// 방 떠나기
    /// </summary>
    public void LeaveRoom()
    {
        Debug.Log("방 떠나기");
        roomFrame.SetActive(false);
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// 대기실 방리스트에서 방을 클릭했을 때 룸UI 활성화
    /// 기존 채팅 정보 초기화
    /// </summary>
    public override void OnJoinedRoom()
    {
        // 방에 입장되면 로비/방이름 입력칸 초기화
        roomInput.text = "";

        // 방 UI 활성화
        roomPanel.SetActive(true);
        roomFrame.SetActive(true);

        /// 방 정보 갱신
        RoomRenewal();

        // 방 참가시 채팅 목록과 입력창 초기화
        for (int i = 0; i < chatText.Length; i++)
        {
            chatText[i].text = "";
        }
        chatInput.text = "";

        // 방 참가시 버튼명 게임 준비로 초기화
        gameStartText.text = "READY";

        // 방 참가자 닉네임 리스트, 게임 준비 토글 초기화
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            nicknameText[i].text = "";
            readyToggle[i].SetActive(false);
        }
    }

    /// <summary>
    /// 방 생성 실패시 재시도
    /// </summary>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        roomInput.text = ""; 
        CreateRoom();
    }

    /// <summary>
    /// 방 참가 실패시 새로운 방 만들기
    /// </summary>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        roomInput.text = ""; 
        CreateRoom();
    }

    /// <summary>
    /// 방에 새로 입장시 방 정보 갱신과 함께 입장 메세지 보내기
    /// </summary>
    /// <param name="newPlayer">신규 플레이어</param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // 방 정보 갱신
        RoomRenewal();

        // 방에 참가한 전체 플레이어에게 채팅
        ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    /// <summary>
    /// 방에서 퇴장시 방 정보 갱신과 함께 퇴장 메세지 보내기
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // 방 정보 갱신
        RoomRenewal();

        // 방에 참가한 전체 플레이어에게 채팅
        ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    /// <summary>
    /// 방 정보 갱신
    /// </summary>
    private void RoomRenewal()
    {
        // 방에 입장한 유저 닉네임 순서대로 배열할 텍스트 객체
        roomName.text = PhotonNetwork.CurrentRoom.Name;

        // 전체 유저 
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            // 방의 오른쪽 패널 상단에 위치한 전체 유저들의 이름 갱신
            //roomName.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");


            // 방의 오른쪽 패널에 중간에 위치한 유저 리스트 이름 갱신
            nicknameText[i].text = PhotonNetwork.PlayerList[i].NickName;
        }

        // 최소인원/최대인원 확인용 텍스트 설정
        roomInfoText.text = "CURRENT : " + PhotonNetwork.CurrentRoom.PlayerCount + " / MAX : " + PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    /// <summary>
    /// 방을 퇴장한 인원이 있으면 방의 오른쪽패널에 위치한 유저 리스트 갱신
    /// </summary>
    [PunRPC]
    private void RoomRenewalRPC()
    {
        // 방 전체 인원이 MAX_PLAYER(4명)이 아니라면
        if (PhotonNetwork.PlayerList.Length < PNM.MAX_PLAYER)
        {
            // 빈 칸 초기화
            for (int i = PNM.MAX_PLAYER - 1; i >= PhotonNetwork.PlayerList.Length; i--)
            {
                nicknameText[i].text = "";
                readyToggle[i].SetActive(false);
            }
        }
    }

    #endregion Room Connect

    #region Chatting Room

    /// <summary>
    /// 전체 채팅 보내기
    /// </summary>
    public void Send()
    {
        // 채팅 InputFiled에 입력한 텍스트를 전체 유저에게 보낸다.
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + chatInput.text);
        // 유저에게 텍스트 전송 후 InputField를 초기화하여 새로운 메세지를 적을 수 있도록 함.
        chatInput.text = "";
        // 채팅을 보낸 이후 채팅 입력란에 포커스 유지
        chatInput.Select();
    }

    /// <summary>
    /// 게임준비, 게임시작 버튼 누를 시 채팅 전달
    /// </summary>
    public void BtnGameReady()
    {

        // 마스터 이동시 나머지 클라이언트도 같이 이동
        PhotonNetwork.AutomaticallySyncScene = true;

        // 게임준비 버튼일 때
        if (gameStartText.text == "READY")
        {
            // 게임 준비 토글키 활성화
            PV.RPC("GameStateRPC", RpcTarget.All, PNM.userNickName, true);


            // 현재 유저가 방장 = 마스터라면
            if (PNM.IsMaster())
            {
                gameStartText.text = "START";
            }
            // 방에 추가된 일반 유저라면
            else
            {
                gameStartText.text = "WATING";
            }

            // 전체 채팅으로 해당 유저가 게임준비가 되었다는 것을 알린다.
            PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + "READY");
        }
        // 게임대기 버튼일 때 = 일반유저
        else if (gameStartText.text == "WATING")
        {
            // 버튼명 변경
            gameStartText.text = "READY";
            // 게임준비 토글키 비활성화
            PV.RPC("GameStateRPC", RpcTarget.All, PNM.userNickName, false);
            // 전체 채팅으로 해당 유저가 게임대기 상태가 되었다는 것을 알린다.
            PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + "WATING");
        }
        // 게임 시작 버튼일 때 = 방장 = 마스터
        else
        {
            // 테스트용 4인 미만 게임 씬으로 이동
            PhotonNetwork.LoadLevel("03_GameScene");

            // 전체 인원 게임 준비 눌렀을 때 유저 게임씬 이동
            //PV.RPC("LoadGameScene", RpcTarget.AllViaServer);
        }
    }

    /// <summary>
    /// 방장=마스터만 호출 가능한 메서드.
    /// 방장이 가지고 있는 유저 인덱스 값과 해당 유저의 게임 상태를 전달.
    /// 새로 입장한 플레이어는 기존의 유저 게임 준비 상태를 UI 업데이트로 확인할 수 있다.
    /// </summary>
    /// <param name="index">게임준비상태 토글키 인덱스</param>
    /// <param name="gameReadyState">게임 준비 상태 여부</param>
    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    private void GameStateInfoUpdateRPC(int index, bool gameReadyState)
    {
        readyToggle[index].SetActive(gameReadyState);
    }

    /// <summary>
    /// 포톤 서버를 통한 전체 유저  게임 준비 상태 토글키 활성화
    /// </summary>
    /// <param name="msg">채팅 문구</param>
    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    private void GameStateRPC(string readyUser, bool isActive)
    {
        // 현재 플레이어에 맞는 게임 준비 토글키 활성화
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            // 게임 준비를 누른 유저 여부 확인
            if (readyUser == nicknameText[i].text)
            {
                readyToggle[i].SetActive(isActive);
            }
        }
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
            // 현제 텍스트 내용을 1칸씩 위로 올림
            for (int i = 1; i < chatText.Length; i++)
            {
                chatText[i - 1].text = chatText[i].text;
            }

            // 채팅창 맨 하단에 입력한 채팅 정보 입력
            chatText[chatText.Length - 1].text = msg;
        }
    }

    /// <summary>
    /// 게임 시작 버튼 눌렀을 때 4명이 모였을 때만 게임씬으로 이동
    /// </summary>
    [PunRPC]
    public void LoadGameScene()
    {
        // 게임 시작 여부
        bool isReady = false;

        // 방 전체 인원이 게임 준비가 되었는지 체크
        for (int i = 0; i < readyToggle.Length; i++)
        {
            if (readyToggle[i].activeSelf == true)
            {
                isReady = true;
            }
            else
            {
                isReady = false;
                break;
            }
        }

        // 전체가 준비 되지 않은 상태라면 게임 시작 불가
        if (!isReady)
        {
            statusText.text = "[게임시작불가] 전체 플레이어가 게임준비 되지 않았습니다.";
            return;
        }

        // 전체 유저가 게임 준비상태이고 방에 있으며 마스터인 사람이 게임시작을 누르면
        if (isReady == true && PhotonNetwork.InRoom && PNM.IsMaster())
        {
            // 전체 유저 게임 씬으로 이동
            PhotonNetwork.LoadLevel("03_GameScene");
        }
    }

    #endregion Chatting Room

    #region Method

    /// <summary>
    /// 설정창 닫기
    /// </summary>
    public void BtnSettingClose()
    {
        settingPanel.SetActive(false);
    }

    #endregion
}
