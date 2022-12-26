using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using static PhotonNetworkManager;

public class EnemySpawner : MonoBehaviourPun, IPunObservable
{
    #region Variable

    private GameManager gameManger;

    // 기본 몬스터를 저장하는 리스트
    private readonly List<BossEnemy> enemies = new List<BossEnemy>();

    public Transform[] spawnPoints;     // 적 AI를 소환할 위치들

    public BossEnemy[] enemyPrefab; // 몬스터 프리팹
    public int maxHP;                   // 몬스터 최대 체력
    public int atk;                     // 몬스터 공격력
    public int def;                     // 몬스터 방어력
    public float moveSpeed;             // 몬스터 이동 속도

    private int enemyCount;             // 남은 적의 수
    private int wave;                   // 현재 웨이브

    #endregion Variable

    #region Photon Method

    // 주기적으로 자동 실행되는, 동기화 메서드
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 로컬 오브젝트라면 쓰기 부분이 실행됨
        if (stream.IsWriting)
        {
            // 적의 남은 수를 네트워크를 통해 보내기
            stream.SendNext(enemies.Count);
            // 현재 웨이브를 네트워크를 통해 보내기
            stream.SendNext(wave);
        }
        else
        {
            // 리모트 오브젝트라면 읽기 부분이 실행됨
            // 적의 남은 수를 네트워크를 통해 받기
            enemyCount = (int)stream.ReceiveNext();
            // 현재 웨이브를 네트워크를 통해 받기 
            wave = (int)stream.ReceiveNext();
        }
    }

    #endregion Photon Method

    #region Unity Method

    [System.Obsolete]
    private void Awake()
    {
        //PhotonPeer.RegisterType(typeof(Color), 128, ColorSerialization.SerializeColor, ColorSerialization.DeserializeColor);
    }

    private void Start()
    {
        gameManger = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        // 호스트만 적을 직접 생성할 수 있음
        // 다른 클라이언트들은 호스트가 생성한 적을 동기화를 통해 받아옴
        if (PhotonNetwork.IsMasterClient)
        {
            // 게임 오버 상태일때는 생성하지 않음
            if (gameManger.isGameveEnd == true || gameManger.isDie) return;

            // 적을 모두 물리친 경우 다음 스폰 실행
            if (enemies.Count <= 0) SpawnWave();
        }
        // UI 갱신
        UpdateUI();
    }

    #endregion Unity Method

    // 웨이브 정보를 UI로 표시
    private void UpdateUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 호스트는 직접 갱신한 적 리스트를 통해 남은 적의 수를 표시함
            //UIManager.Instance.UpdateWaveText(wave, enemies.Count);
        }
        else
        {
            // 클라이언트는 적 리스트를 갱신할 수 없으므로, 호스트가 보내는 enemyCount를 통해 적의 수를 표시함
            //UIManager.Instance.UpdateWaveText(wave, enemyCount);
        }
    }

    // 현재 웨이브에 맞춰 적을 생성
    private void SpawnWave()
    {
        // 웨이브 1 증가
        wave++;

        // 현재 웨이브 * 1.5에 반올림 한 개수 만큼 적을 생성
        var spawnCount = Mathf.RoundToInt(wave * 5f);

        // spawnCount 만큼 적을 생성
        for (var i = 0; i < spawnCount; i++)
        {
            // 적의 세기를 0%에서 100% 사이에서 랜덤 결정
            var enemyIntensity = Random.Range(0f, 1f);
            // 적 생성 처리 실행
            CreateEnemy(enemyIntensity);
        }
    }

    // 적을 생성하고 생성한 적에게 추적할 대상을 할당
    private void CreateEnemy(float intensity)
    {
        // 생성할 위치를 랜덤으로 결정
        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 에너미 프리팹 경로
        string path = "Enemy/";

        int randEnemy = Random.Range(0, enemyPrefab.Length -1);

        // 적 프리팹으로부터 적 생성
        var CreateEnemy = Instantiate(enemyPrefab[randEnemy], spawnPoint.position, spawnPoint.rotation);

        BossEnemy enemy = CreateEnemy.GetComponent<BossEnemy>();

        // 생성한 적의 능력치와 추적 대상 설정
        //enemy.photonView.RPC("SetUp", RpcTarget.All, maxHP, atk, def, moveSpeed, (float)moveSpeed * 0.3);

        // 생성된 적을 리스트에 추가
        enemies.Add(enemy);

        // 적의 onDeath 이벤트에 익명 메서드 등록
        // 사망한 적을 리스트에서 제거
        //enemy.OnDeath += () => enemies.Remove(enemy);

        // 사망한 적을 10 초 뒤에 파괴
        //enemy.OnDeath += () => StartCoroutine(DestroyAfter(enemy.gameObject, 10f));

        // 적 사망시 점수 상승
        //enemy.OnDeath += () => GameManager.Instance.AddScore(100);
    }

    // 포톤의 Network.Destory()는 자연 파괴를 지원하지 않으므로 지연 파괴를 직접 구현
    IEnumerator DestroyAfter(GameObject target, float delay)
    {
        // delay 만큼 쉬고
        yield return new WaitForSeconds(delay);

        // target이 아직 파괴되지 않았다면
        if (target != null)
        {
            // target을 모든 네트워크 상에서 파괴
            PhotonNetwork.Destroy(target);
        }
    }
}
