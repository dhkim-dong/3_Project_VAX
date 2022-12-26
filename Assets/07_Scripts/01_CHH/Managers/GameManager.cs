using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PhotonNetworkManager;
using static GameNetworkUIManager;
using static GameManager;

// 게임 진행을 관리하는 게임 매니저
public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Variable

    // 싱글톤이 할당될 static 변수
    private static GameManager instance;

    // 외부에서 싱글톤 오브젝트를 가져올때 사용할 프로퍼티
    public static GameManager GM
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (instance == null)
            {
                // 씬에서 GameManager 오브젝트를 찾아 할당
                instance = FindObjectOfType<GameManager>();
            }

            // 싱글톤 오브젝트를 반환
            return instance;
        }
    }

    // 플레이어 게임 상태
    public enum PlayerGameState 
    { 
        NONE,       // 플레이어 생성 안됨
        INIT,       // 플레이어 생성됨
        ALIVE,      // 플레이어 살아있음
        REMOVE,     // 플레이어 포톤 연결상태 해제로 삭제
        DAMAGE,     // 플레이어 데미지 입음
        DIE         // 플레이어 죽음
    }

    // 좀비 웨이브 단계
    public enum ZombieWaveStep
    {
        FirstDay,   // 첫쨋날
        SECOND_DAY, // 둘쨋날
        THIRD_DAY,  // 셋쨋날
        END_DAY     // 마지막날
    }

    //private GameNetworkUIManager gameNetworkUIManager;

    public PhotonView PV;                   // 포톤 연결정보
    public GameObject[] playerCharacters;   // 랜덤 플레이어로 사용할 캐릭터 오브젝트 리스트
    public Transform[] playerSpwnPos;      // 플레이어가 생성될 위치 리스트

    public List<PlayerInfo> playerInfos;    // 플레이어 정보를 저장하는 클래스
    public List<GameObject> playerPrefabs = new List<GameObject>();

    public PlayerGameState playerGameState; // 플레이어 게임 상태
    public ZombieWaveStep zombieWaveStep;   // 좀비 웨이브 단계
    public int mafiaNum;                    // 마피아로 지정된 플레이어 고유번호
    public bool isDie;                      // 플레이어 죽음 여부
    public bool isMafiaWin;                 // 마피아 승리 여부
    public bool isEscape;                   // 탈출 여부
    public bool isGameveEnd;                // 게임이 종료되었는지 여부

    // 테스트 확인용
    public string jsonPlayerInfo;     

    #endregion Variable

    #region Unity Method

    // 초기화
    private void Awake()
    {
        // 싱글톤 객체 1개만 존재하도록 설정
        if (null == instance)
        {
            instance = this;
            // 씬을 이동하더라도 사라지지 않도록 설정
            DontDestroyOnLoad(this);

            Debug.Log("DontDestroyOnLoad");
        }
        else Destroy(this);
    }

    // 게임 초기화
    private IEnumerator Start()
    {
        // 현재 플레이어가 마스터인지 체크
        if (PNM.IsMaster())
        {
            // 플레이어 정보 초기화
            MasterInitPlayerInfo();
        }

        // false라면  반복체크하면서 모든 플레이어가 씬 이동이 될 때까지 기다림.
        while (PNM.AllhasTag("ReceivePlayerInfo", "True") == false)
        {
            yield return null;
        }

        // 게임씬 시작시 플레이어 생성 및 초기화 작업
        yield return Loading();

        // 플레이어 게임 진행상태 생존으로 변경
        playerGameState = PlayerGameState.ALIVE;

        // 좀비 웨이브 단계 = 첫번째날
        zombieWaveStep = ZombieWaveStep.FirstDay;

        // 마피아 승리여부 초기화
        isMafiaWin = false;

        // 탈출여부 초기화
        isEscape = false;

        // 죽음여부 초기화
        isDie = false;

        // 게임 종료 여부 초기화
        isGameveEnd = false;
    }

    // 매 프레임마다 반복
    private void Update()
    {
        // 플레이어가 정상적으로 생성된 후 부터 게임 종료전까지 게임종료여부 체크
        if(playerGameState == PlayerGameState.ALIVE && isGameveEnd == false)
        {
            // 게임 종료 여부 체크
            GameEndCheck();

            // 게임종료 되었으면 실행
            GameEnd();
        }
    }

    #endregion Unity Method

    #region Method

    /// <summary>
    /// 게임시작 시 플레이어 생성 및 초기화 기능
    /// </summary>
    /// <returns></returns>
    private IEnumerator Loading()
    {
        Debug.Log("Loading");

        // 게임씬이 로드 되었음을 저장
        PNM.SetTag("LoadGameScene", "True");

        // AllhasTag : 방 전체 인원이 같은 곳으로 이동 되었을 경우 true 반환
        // false라면  반복체크하면서 모든 플레이어가 씬 이동이 될 때까지 기다림.
        while (PNM.AllhasTag("LoadGameScene", "True") == false)
        {
            yield return null;
        }

        Debug.Log("전체 게임 씬 이동 완료.");

        // 플레이어 생성
        PlayerSpawn();

        // 모든 플레이어가 스폰이 되었을 때까지 기다림
        while (PNM.AllhasTag("LoadPlayer", "True") == false)
        {
            yield return null;
        }
    }

    /// <summary>
    /// 플레이어 오브젝트 생성
    /// </summary>
    private void PlayerSpawn()
    {
        // Resources 폴더의 하위 폴더안에 생성할 오브젝트가 존재하므로 경로설정
        string path = "Player/";

        // 현재 플레이어의 고유넘버를 가져온다.
        int playerIdx = PNM.GetActorNumber() - 1;

        // 현재 플레이어의 랜덤 캐릭터 번호를 가져온다.
        int myRandCharacterNum = playerInfos[playerIdx].randCharacterNum;
        path += playerCharacters[myRandCharacterNum].name;

        // 플레이어 오브젝트 리스트에 생성한 플레이어 추가
        playerPrefabs.Add(PhotonNetwork.Instantiate(path, playerSpwnPos[playerIdx].transform.position, Quaternion.identity));

        // SetTag : 키와 태그값 저장
        // 플레이어가 스폰이 완료됨을 의미
        PNM.SetTag("LoadPlayer", "True");
    }

    /// <summary>
    /// 마스터만 호출가능한 메서드
    /// 플레이어 정보 초기화
    /// </summary>
    private void MasterInitPlayerInfo()
    {
        Debug.Log("MasterInitPlayerInfo / 마스터만 가능");

        // 마피아 랜덤 지정
        mafiaNum = Random.Range(0, PhotonNetwork.PlayerList.Length);

        // 게임시작시 초기화
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            // 플레이어 정보 저장
            Player player = PhotonNetwork.PlayerList[i];

            // 랜덤 캐릭터 지정
            int randCharacterNum = Random.Range(0, playerCharacters.Length-1);

            // 협력자/마피아 여부
            bool isMafia = false;

            // 마피아로 지정되 었다면 true로 바꿔준다.
            if(i== mafiaNum)
            {
                isMafia = true;
            }

            // PlayerInfo : 플레이어 정보 초기화
            // playerInfos.Add : 초기화된 플레이어 정보를 리스트에 추가한다.
            playerInfos.Add(new PlayerInfo(player.NickName, player.ActorNumber, randCharacterNum, isMafia));
        }

        playerGameState = PlayerGameState.INIT;

        // PlayerInfo 리스트 Json화 시켜서 보내기
        MasterSendPlayerInfo();
    }

    /// <summary>
    /// OnPlayerLeftRoom으로 방을 나갈경우 플레이어 제거
    /// </summary>
    /// <param name="actorNum">플레이어 고유 식별자</param>
    private void MasterRemovePlayerInfo(int actorNum)
    {
        // 지정된 플레이어와 같은 식별자 번호를 가진 플레이어 정보를 찾는다.
        PlayerInfo playerInfo = playerInfos.Find(player => player.actorNum == actorNum);

        // 플레이어 정보 리스트에서 해당 정보 삭제
        playerInfos.Remove(playerInfo);

        playerGameState = PlayerGameState.REMOVE;

        MasterSendPlayerInfo();
    }

    /// <summary>
    /// 마스터는 PlayerInfo 정렬 후 클라이언트에게 정보를 전달한다.
    /// </summary>
    /// <param name="playergameState"></param>
    public void MasterSendPlayerInfo()
    {
        if (PNM.IsMaster() == false) return;

        // 플레이어의 식별자 번호 순 정렬
        playerInfos.Sort((p1, p2) => p1.actorNum.CompareTo(p2.actorNum));

        // 플레이어 정보 리스트를 JSON 포맷으로 직렬화 한다.
        string jdata = JsonUtility.ToJson(new Serialization<PlayerInfo>(playerInfos));

        Debug.Log("Json Data : " + jdata);

        // 클라이언트들에게 플레이어 정보 리스트 데이터를 전송한다.
        PV.RPC("OtherReceivePlayerInfoRPC", RpcTarget.All, jdata);
    }

    /// <summary>
    /// 포톤 서버를 통해 플레이어의 정보 리스트를 받는다.
    /// </summary>
    /// <param name="playerGameState">플레이어 게임 상태</param>
    /// <param name="jdata">JSON 포맷으로 직렬화한 플레이어의 정보 리스트</param>
    [PunRPC]
    private void OtherReceivePlayerInfoRPC(string jdata)
    {
        // 클라이언트가 마스터에게서 플레이어 정보가 담겨있는 리스트 playerInfos를 받아왔음을 의미.
        PNM.SetTag("ReceivePlayerInfo", "True");

        // 마스터에서 받아온 playerInfos 데이터 정보가 없을 경우
        if (jdata == null || jdata == "")
        {
            Debug.Log("OtherReceivePlayerInfoRPC / playerInfos Json 파일 못받아왔음.");
            return;
        }

        jsonPlayerInfo = jdata;

        // 다른 사람은 PlayerInfo 받기
        playerInfos = JsonUtility.FromJson<Serialization<PlayerInfo>>(jdata).playerInfos;

        // 플레이어 정보 데이터 확인
        for (int i = 0; i < playerInfos.Count; i++)
        {
            Debug.Log($"NickName : {playerInfos[i].nickName}");
            Debug.Log($"ActorNum : {playerInfos[i].actorNum}");
            Debug.Log($"RandCharacterNum : {playerInfos[i].randCharacterNum}");
            Debug.Log($"IsMafia : {playerInfos[i].isMafia}");
            Debug.Log("GNUM.PlayerRoleSend / 생존자/마피아 역할 알려주는 메서드");

            // 게임 시작할 때 역할 메세지 초기화
            // playerIndex가 현재 플레이어 고유 넘버와 같아야 실행
            GNUM.PlayerRoleSend();
        }

        // 플레이어 상태 UI 갱신
        GNUM.PlayerStateRenewal();
    }

    /// <summary>
    /// 게임 종료 체크 메서드
    /// </summary>
    public void GameEndCheck()
    {
        isMafiaWin = false;      // 마피아 승리 여부

        // 생존자 숫자 : 전체 인원 - 마피아 1명
        int survivorCount = PhotonNetwork.PlayerList.Length  -1;     

        // 플레이어 수만큼 반복
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            // 협력자 역할을 맡은 플레이어가 죽었는제 체크
            if (playerInfos[i].isDie == true && playerInfos[i].isMafia == false)
            {
                survivorCount--;
            }
        }

        // 마피아가 살아 있으며 생존자가 모두 죽었으면 마피아 승리
        if (playerInfos[mafiaNum].isDie == false && survivorCount == 0)
        {
            // 마피아 승리
            isMafiaWin = true;

            // 게임 종료됨
            isGameveEnd = true;
        }
        // 생존자가 탈출에 성공했다면 생존자 승리
        else if(isEscape == true)
        {
            // 게임 종료됨
            isGameveEnd = true;

            // 생존자 숫자 : 전체 인원 - 마피아 1명
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
        // 좀비 웨이브가 마지막일 때 무조건 게임 종료
        // 마피아, 생존자 모두 게임 패배
        else if(zombieWaveStep == ZombieWaveStep.END_DAY)
        {
            // 게임 종료됨
            isGameveEnd = true;
        }
    }

    /// <summary>
    /// 게임 종료시 게임종료 UI 켜기
    /// </summary>
    public void GameEnd()
    {
        // 게임이 종료되었다면 실행
        if(isGameveEnd == true)
        {
            // 마피아 이겼다면
            if (isMafiaWin == true)
            {
                // GameNetworkUIManger의 승자를 마피아로 설정
                for(int i=0;i< PhotonNetwork.PlayerList.Length; i++)
                {
                    if(playerInfos[i].isMafia == true)
                    {
                        Debug.Log($"{playerInfos[i].nickName} / 마피아 승리!");
                        break;
                    }
                }
            }
            // 생존자가 이겼다면
            else if (isEscape == true)
            {
                // 생존자 이름 저장
                string playerNames = "";
                
                // 생존자 검색
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    // 생존자 역할이면서 탈출에 성공한 플레이어 찾기
                    if (playerInfos[i].isMafia == false && playerInfos[i].isEcapeSuccess == true)
                    {
                        // 탈출에 성공한 플레이어명 저장
                        playerNames += playerInfos[i].nickName + " / ";
                    }
                }

                Debug.Log($"{playerNames}생존자 승리!");
            }
            // 생존자/마피아 모두 게임 패배
            else if (isMafiaWin == false && isEscape == false)
            {
                Debug.Log($"생존자/마피아 모두 게임에 패배하였습니다.");
            }
        }
    }

    #endregion Method

    #region Photon Method

    // 주기적으로 자동 실행되는, 동기화 메서드
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 로컬 오브젝트라면 쓰기 부분이 실행됨
        if (stream.IsWriting)
        {
            // 네트워크를 통해 score 값을 보내기
            stream.SendNext(isGameveEnd);
        }
        // 리모트 오브젝트라면 읽기 부분이 실행됨  
        else
        {
            // 네트워크를 통해 게임종료여부 값 받기
            isGameveEnd = (bool)stream.ReceiveNext();
        }
    }

    /// <summary>
    /// 플레이어가 방을 나갔다면
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // 마스터가 나가면 바뀐 마스터가 호출되서 성공
        if (PNM.IsMaster())
        {
            MasterRemovePlayerInfo(otherPlayer.ActorNumber);
        }
    }

    #endregion Photon Method
}
