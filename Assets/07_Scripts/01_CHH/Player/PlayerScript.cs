using Photon.Pun;
using UnityEngine;
using static PhotonNetworkManager;

public class PlayerScript : MonoBehaviourPun, IPunObservable
{
    #region Variable

    public PhotonView PV;               // ���� ��������
    private GameManager gameManager;    // ���Ӿ� ��Ƽ���� ������
    private Vector3 currentPos;         // �÷��̾� ���� ��ġ
    private bool isDie;                 // �÷��̾� ���� ����

    // �÷��̾� ������Ʈ�� ��� ����Ʈ
    //List<GameObject> players = new List<GameObject>(); 


    #endregion Variable

    #region Unity Method

    /// <summary>
    /// ������Ʈ �ʱ�ȭ
    /// </summary>
    private void Awake()
    {
        // ������ GameManager ������Ʈ�� ã�� �Ҵ�
        gameManager = FindObjectOfType<GameManager>();

        // SetTag : Ű�� �±װ� ����
        // �÷��̾ ������ �Ϸ���� �ǹ�
        PNM.SetTag("LoadPlayer", "True");

        // PhotonView�� ���� ĳ�� ����
        PV = photonView;
    }

    private void Start()
    {
        // photonView.isMine�� false�� �� ���� �ƴϸ� �ִϸ����͸� �������� �ʰڴٴ� ��.
        // false�̸� PhotonView ������Ʈ�� ���ؼ��� transform�� �ִϸ����Ͱ� ����ȭ�Ǽ� �����ϰ�.
        if (PV.IsMine == false) return;


        //PlayerSpawn();

    }

    private void Update()
    {

    }

    #endregion Unity Method

    #region Photon Method

    /// <summary>
    /// IPunObservable ��� �� �� �����ؾ� �ϴ� ��
    /// �����͸� ��Ʈ��ũ ����� ���� ������ �ް� �ϰ� �ϴ� �ݹ� �Լ�
    /// ���� ������ �ֱ������� �ڵ� ����Ǵ�, ����ȭ �޼���
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // ���� ������Ʈ��� ���� �κ��� �����
        if (stream.IsWriting)
        {
            // ��Ʈ��ũ�� ���� score ���� ������
            stream.SendNext(transform.position);
        }
        // ����Ʈ ������Ʈ��� �б� �κ��� �����  
        else
        {
            // ��Ʈ��ũ�� ���� score �� �ޱ�
            currentPos = (Vector3)stream.ReceiveNext();
        }
    }

    #endregion Photon Method

    #region Method

    /// <summary>
    /// �÷��̾� ������Ʈ ����
    /// </summary>
    private void PlayerSpawn()
    {
        // Resources ������ ���� �����ȿ� ������ ������Ʈ�� �����ϹǷ� ��μ���
        string path = "Player/";

        // ���� �÷��̾��� �����ѹ��� �����´�.
        int myActorNum = PNM.GetActorNumber();

        // ���� �÷��̾��� ���� ĳ���� ��ȣ�� �����´�.
        int myRandCharacterNum = gameManager.playerInfos[myActorNum].randCharacterNum;

        // �÷��̾� ������Ʈ ����Ʈ�� ������ �÷��̾� �߰�
        //players.Add(PhotonNetwork.Instantiate(path + gameManager.playerCharacters[myRandCharacterNum].name, gameManager.playerSpwnPos[myActorNum].transform.position, Quaternion.identity));
    }

    #endregion Method
}
