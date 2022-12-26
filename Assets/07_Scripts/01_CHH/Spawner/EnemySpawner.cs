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

    // �⺻ ���͸� �����ϴ� ����Ʈ
    private readonly List<BossEnemy> enemies = new List<BossEnemy>();

    public Transform[] spawnPoints;     // �� AI�� ��ȯ�� ��ġ��

    public BossEnemy[] enemyPrefab; // ���� ������
    public int maxHP;                   // ���� �ִ� ü��
    public int atk;                     // ���� ���ݷ�
    public int def;                     // ���� ����
    public float moveSpeed;             // ���� �̵� �ӵ�

    private int enemyCount;             // ���� ���� ��
    private int wave;                   // ���� ���̺�

    #endregion Variable

    #region Photon Method

    // �ֱ������� �ڵ� ����Ǵ�, ����ȭ �޼���
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // ���� ������Ʈ��� ���� �κ��� �����
        if (stream.IsWriting)
        {
            // ���� ���� ���� ��Ʈ��ũ�� ���� ������
            stream.SendNext(enemies.Count);
            // ���� ���̺긦 ��Ʈ��ũ�� ���� ������
            stream.SendNext(wave);
        }
        else
        {
            // ����Ʈ ������Ʈ��� �б� �κ��� �����
            // ���� ���� ���� ��Ʈ��ũ�� ���� �ޱ�
            enemyCount = (int)stream.ReceiveNext();
            // ���� ���̺긦 ��Ʈ��ũ�� ���� �ޱ� 
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
        // ȣ��Ʈ�� ���� ���� ������ �� ����
        // �ٸ� Ŭ���̾�Ʈ���� ȣ��Ʈ�� ������ ���� ����ȭ�� ���� �޾ƿ�
        if (PhotonNetwork.IsMasterClient)
        {
            // ���� ���� �����϶��� �������� ����
            if (gameManger.isGameveEnd == true || gameManger.isDie) return;

            // ���� ��� ����ģ ��� ���� ���� ����
            if (enemies.Count <= 0) SpawnWave();
        }
        // UI ����
        UpdateUI();
    }

    #endregion Unity Method

    // ���̺� ������ UI�� ǥ��
    private void UpdateUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // ȣ��Ʈ�� ���� ������ �� ����Ʈ�� ���� ���� ���� ���� ǥ����
            //UIManager.Instance.UpdateWaveText(wave, enemies.Count);
        }
        else
        {
            // Ŭ���̾�Ʈ�� �� ����Ʈ�� ������ �� �����Ƿ�, ȣ��Ʈ�� ������ enemyCount�� ���� ���� ���� ǥ����
            //UIManager.Instance.UpdateWaveText(wave, enemyCount);
        }
    }

    // ���� ���̺꿡 ���� ���� ����
    private void SpawnWave()
    {
        // ���̺� 1 ����
        wave++;

        // ���� ���̺� * 1.5�� �ݿø� �� ���� ��ŭ ���� ����
        var spawnCount = Mathf.RoundToInt(wave * 5f);

        // spawnCount ��ŭ ���� ����
        for (var i = 0; i < spawnCount; i++)
        {
            // ���� ���⸦ 0%���� 100% ���̿��� ���� ����
            var enemyIntensity = Random.Range(0f, 1f);
            // �� ���� ó�� ����
            CreateEnemy(enemyIntensity);
        }
    }

    // ���� �����ϰ� ������ ������ ������ ����� �Ҵ�
    private void CreateEnemy(float intensity)
    {
        // ������ ��ġ�� �������� ����
        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // ���ʹ� ������ ���
        string path = "Enemy/";

        int randEnemy = Random.Range(0, enemyPrefab.Length -1);

        // �� ���������κ��� �� ����
        var CreateEnemy = Instantiate(enemyPrefab[randEnemy], spawnPoint.position, spawnPoint.rotation);

        BossEnemy enemy = CreateEnemy.GetComponent<BossEnemy>();

        // ������ ���� �ɷ�ġ�� ���� ��� ����
        //enemy.photonView.RPC("SetUp", RpcTarget.All, maxHP, atk, def, moveSpeed, (float)moveSpeed * 0.3);

        // ������ ���� ����Ʈ�� �߰�
        enemies.Add(enemy);

        // ���� onDeath �̺�Ʈ�� �͸� �޼��� ���
        // ����� ���� ����Ʈ���� ����
        //enemy.OnDeath += () => enemies.Remove(enemy);

        // ����� ���� 10 �� �ڿ� �ı�
        //enemy.OnDeath += () => StartCoroutine(DestroyAfter(enemy.gameObject, 10f));

        // �� ����� ���� ���
        //enemy.OnDeath += () => GameManager.Instance.AddScore(100);
    }

    // ������ Network.Destory()�� �ڿ� �ı��� �������� �����Ƿ� ���� �ı��� ���� ����
    IEnumerator DestroyAfter(GameObject target, float delay)
    {
        // delay ��ŭ ����
        yield return new WaitForSeconds(delay);

        // target�� ���� �ı����� �ʾҴٸ�
        if (target != null)
        {
            // target�� ��� ��Ʈ��ũ �󿡼� �ı�
            PhotonNetwork.Destroy(target);
        }
    }
}
