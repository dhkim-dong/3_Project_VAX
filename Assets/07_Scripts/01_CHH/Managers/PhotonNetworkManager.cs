using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using static FirebaseAuthManager;

public class PhotonNetworkManager : MonoBehaviourPun
{ 
    #region Variable

    // 싱글톤이 할당될 static 변수
    private static PhotonNetworkManager instance;

    // 외부에서 싱글톤 오브젝트를 가져올때 사용할 프로퍼티
    public static PhotonNetworkManager PNM
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (instance == null)
            {
                // 씬에서 NetworkManager 오브젝트를 찾아 할당
                instance = FindObjectOfType<PhotonNetworkManager>();
            }

            // 싱글톤 오브젝트를 반환
            return instance;
        }
    }

    public PhotonView PV;         // 포톤 연결정보

    public int MAX_PLAYER = 4;  // 방 최대 입장 인원 = 4
    public string userNickName;     // 플레이어 닉네임
    public int MaxUser;             // 최대 플레이어 수

    #endregion Variable

    #region Unity Method

    // Start 메서드 실행 전 컴포넌트 초기화
    private void Awake()
    {
    }

    // 컴포넌트 초기화
    private void Start()
    {
        // 싱글톤 객체 1개만 존재하도록 설정
        if (null == instance)
        {
            instance = this;

            // 씬을 이동하더라도 사라지지 않도록 설정
            DontDestroyOnLoad(this);
        }
        // 싱글톤 객체가 1개 이상 존재할 경우 이후에 생성된 객체 삭제
        else
        {
            Destroy(this);
        }

        // 포톤 네트워크 연결
        PhotonNetworkConnect();

        // 포톤 초기 설정
        Setting();
    }

    // 매 프레임 마다 반복
    private void Update()
    {

    }

    #endregion Unity Method

    #region Method

    // 현재 유저가 마스터인지 확인
    // true : 마스터 / false : 클라이언트
    public bool IsMaster()
    {
        return PhotonNetwork.LocalPlayer.IsMasterClient;
    }

    /// <summary>
    /// 지정한 플레이어의 고유 식별번호 가져오기
    /// </summary>
    /// <param name="player">포톤에서 제공하는 플레이어 컴포넌트</param>
    /// <returns></returns>
    public int GetActorNumber(Player player = null)
    {
        // 플레이어를 지정하지 않았으면 자신을 지정
        if (player == null)
        {
            player = PhotonNetwork.LocalPlayer;
        }

        // Debug.Log($"{player.NickName} 플레이어의 식별자 : " + player.ActorNumber);

        // 지정된 플레이어의 고유 식별자 반환
        return player.ActorNumber;
    }

    /// <summary>
    /// 포톤 네트워크에서 지정한 오브젝트 객체 리스트 삭제
    /// </summary>
    /// <param name="_gameObjects"> 삭제할 게임 오브젝트 리스트</param>
    public void DestroyGameObjectList(List<GameObject> _gameObjects)
    {
        // 리스트에 담긴 모든 객체 삭제
        for (int i = 0; i < _gameObjects.Count; i++)
        {
            // 포톤에서 지정된 객체 삭제
            PhotonNetwork.Destroy(_gameObjects[i]);
        }
    }

    /// <summary>
    /// 지정한 트랜스폼 위치 재설정
    /// </summary>
    /// <param name="transform">위치를 변경할 트랜스폼</param>
    /// <param name="target">이동할 위치</param>
    public void SetPos(Transform transform, Vector3 target)
    {
        transform.position = target;
    }

    /// <summary>
    /// 포톤에서 제공하는 플레이어에 키와 태그값을 부여
    /// 씬이름과 그곳에 존재하는 여부를 또는 마피아 여부나 퀘스트 완료 여부를 확인 하도록 함.
    /// </summary>
    /// <param name="key">플레이어가 체크해야할 사항</param>
    /// <param name="value">체크 사항 여부. true/false 또는 특정한 값을 입력</param>
    /// <param name="player">키, 태그값을 부여할 포톤에서 제공하는 플레이어 컴포넌트</param>
    public void SetTag(string key, string value, Player player = null)
    {
        // 플레이어를 지정하지 않았으면 자신을 지정
        if (player == null)
        {
            player = PhotonNetwork.LocalPlayer;
        }

        // 지정된 플레이어에 키, 태그값 부여
        player.SetCustomProperties(new Hashtable { { key, value } });
    }

    /// <summary>
    /// 지정한 유저의 키에 맞는 태그 값을 가져온다.
    /// </summary>
    /// <param name="player">키값을 조회할 포톤에서 제공하는 플레이어 컴포넌트</param>
    /// <param name="key">조회할 키</param>
    /// <returns></returns>
    public object GetTag(Player player, string key)
    {
        // 지정한 플레이어가 해당 키를 가지고 있지 않다면 null 반환
        if (player.CustomProperties[key] == null)
        {
            return null;
        }

        // 키를 가지고 있으면 태그값 반환
        return player.CustomProperties[key].ToString();
    }

    /// <summary>
    /// 전체 유저가 지정한 키값이 저장되어 있는지 체크한다.
    /// 전체 유저가 같은 씬으로 이동 되었을 경우 true 반환
    /// </summary>
    /// <param name="key">조회할 키</param>
    /// <returns></returns>
    public bool AllhasTag(string key, string value)
    {
        int getTagPlayerCount = 0;

        // 전체 유저 목록을 확인 할수 있도록 함.
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];

            Debug.Log(player.NickName + "/ " + player.ActorNumber + " / "+ key + "/ " + player.CustomProperties[key]);

            // 유저가 해당 키값이 없는지 확인
            if ((string)player.CustomProperties[key] == "True")
            {
                Debug.Log($"{player.NickName} / {key} / {player.CustomProperties[key]}");
                getTagPlayerCount++;
            }
        }

        // 전체 유저가 키값이 존재하면 true를 반환
        if (getTagPlayerCount == PhotonNetwork.PlayerList.Length)
        {
            Debug.Log($"{key} AllhasTag 결과값 : True");
            return true;
        }
        else
        {
            // 1명이라도 키값이 없으면 false 반환
            Debug.Log($"{key} AllhasTag 결과값 : False");
            return false;
        }
    }
    #endregion Method

    #region Photon Connect

    /// <summary>
    /// 포톤 초기 설정
    /// </summary>
    private void Setting()
    {
        // 해상도 설정
        //Screen.SetResolution(960, 540, false);

        // 포톤 게임 버전 설정
        PhotonNetwork.GameVersion = "1";

        // 클라이언트가 얼마나 자주 패키지를 전송 할지 설정 / 포톤으로 주고 받는 통신간격
        // 포톤으로 주고 받는 통신간격
        PhotonNetwork.SendRate = 40;

        // 데이터를 받아쓰는 빈도
        PhotonNetwork.SerializationRate = 20;

        // 마스터 이동시 나머지 클라이언트도 같이 이동
        PhotonNetwork.AutomaticallySyncScene = true;

        Debug.Log("PhotonNetworkManager / 포톤 세팅 완료");
    }

    /// <summary>
    /// 포톤 네트워크 연결
    /// </summary>
    public void PhotonNetworkConnect()
    {
        // 포톤 연결이 되어 있지 않다면 포톤 연결
        if (!PhotonNetwork.IsConnected)
        {
            // 파이어베이스 유저 닉네임 불러오기
            userNickName = FAM.user.DisplayName;

            // 포톤 닉네임 설정
            PhotonNetwork.NickName = userNickName;

            // 해당 게임버전으로 photon 클라우드로 연결되는 시작점
            // ConnectUsingSettings( 게임버전 생략가능 )
            PhotonNetwork.ConnectUsingSettings();

            Debug.Log("포톤 서버 연결 시작");
        }
    }

    #endregion Photon Connect
}
