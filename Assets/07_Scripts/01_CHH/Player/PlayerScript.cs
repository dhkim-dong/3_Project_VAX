using Photon.Pun;
using UnityEngine;
using static PhotonNetworkManager;

public class PlayerScript : MonoBehaviourPun, IPunObservable
{
    #region Variable

    public PhotonView PV;               // 포톤 연결정보
    private GameManager gameManager;    // 게임씬 멀티서버 관리자
    private Vector3 currentPos;         // 플레이어 현재 위치
    private bool isDie;                 // 플레이어 죽음 여부

    // 플레이어 오브젝트를 담는 리스트
    //List<GameObject> players = new List<GameObject>(); 


    #endregion Variable

    #region Unity Method

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void Awake()
    {
        // 씬에서 GameManager 오브젝트를 찾아 할당
        gameManager = FindObjectOfType<GameManager>();

        // SetTag : 키와 태그값 저장
        // 플레이어가 스폰이 완료됨을 의미
        PNM.SetTag("LoadPlayer", "True");

        // PhotonView에 대한 캐시 참조
        PV = photonView;
    }

    private void Start()
    {
        // photonView.isMine이 false면 즉 내가 아니면 애니메이터를 실행하지 않겠다는 것.
        // false이면 PhotonView 컴포넌트에 의해서만 transform과 애니메이터가 동기화되서 움직일것.
        if (PV.IsMine == false) return;


        //PlayerSpawn();

    }

    private void Update()
    {

    }

    #endregion Unity Method

    #region Photon Method

    /// <summary>
    /// IPunObservable 상속 시 꼭 구현해야 하는 것
    /// 데이터를 네트워크 사용자 간에 보내고 받고 하게 하는 콜백 함수
    /// 포톤 서버의 주기적으로 자동 실행되는, 동기화 메서드
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 로컬 오브젝트라면 쓰기 부분이 실행됨
        if (stream.IsWriting)
        {
            // 네트워크를 통해 score 값을 보내기
            stream.SendNext(transform.position);
        }
        // 리모트 오브젝트라면 읽기 부분이 실행됨  
        else
        {
            // 네트워크를 통해 score 값 받기
            currentPos = (Vector3)stream.ReceiveNext();
        }
    }

    #endregion Photon Method

    #region Method

    /// <summary>
    /// 플레이어 오브젝트 생성
    /// </summary>
    private void PlayerSpawn()
    {
        // Resources 폴더의 하위 폴더안에 생성할 오브젝트가 존재하므로 경로설정
        string path = "Player/";

        // 현재 플레이어의 고유넘버를 가져온다.
        int myActorNum = PNM.GetActorNumber();

        // 현재 플레이어의 랜덤 캐릭터 번호를 가져온다.
        int myRandCharacterNum = gameManager.playerInfos[myActorNum].randCharacterNum;

        // 플레이어 오브젝트 리스트에 생성한 플레이어 추가
        //players.Add(PhotonNetwork.Instantiate(path + gameManager.playerCharacters[myRandCharacterNum].name, gameManager.playerSpwnPos[myActorNum].transform.position, Quaternion.identity));
    }

    #endregion Method
}
