using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using static FirebaseAuthManager;

public class PhotonNetworkManager : MonoBehaviourPun
{ 
    #region Variable

    // �̱����� �Ҵ�� static ����
    private static PhotonNetworkManager instance;

    // �ܺο��� �̱��� ������Ʈ�� �����ö� ����� ������Ƽ
    public static PhotonNetworkManager PNM
    {
        get
        {
            // ���� �̱��� ������ ���� ������Ʈ�� �Ҵ���� �ʾҴٸ�
            if (instance == null)
            {
                // ������ NetworkManager ������Ʈ�� ã�� �Ҵ�
                instance = FindObjectOfType<PhotonNetworkManager>();
            }

            // �̱��� ������Ʈ�� ��ȯ
            return instance;
        }
    }

    public PhotonView PV;         // ���� ��������

    public int MAX_PLAYER = 4;  // �� �ִ� ���� �ο� = 4
    public string userNickName;     // �÷��̾� �г���
    public int MaxUser;             // �ִ� �÷��̾� ��

    #endregion Variable

    #region Unity Method

    // Start �޼��� ���� �� ������Ʈ �ʱ�ȭ
    private void Awake()
    {
    }

    // ������Ʈ �ʱ�ȭ
    private void Start()
    {
        // �̱��� ��ü 1���� �����ϵ��� ����
        if (null == instance)
        {
            instance = this;

            // ���� �̵��ϴ��� ������� �ʵ��� ����
            DontDestroyOnLoad(this);
        }
        // �̱��� ��ü�� 1�� �̻� ������ ��� ���Ŀ� ������ ��ü ����
        else
        {
            Destroy(this);
        }

        // ���� ��Ʈ��ũ ����
        PhotonNetworkConnect();

        // ���� �ʱ� ����
        Setting();
    }

    // �� ������ ���� �ݺ�
    private void Update()
    {

    }

    #endregion Unity Method

    #region Method

    // ���� ������ ���������� Ȯ��
    // true : ������ / false : Ŭ���̾�Ʈ
    public bool IsMaster()
    {
        return PhotonNetwork.LocalPlayer.IsMasterClient;
    }

    /// <summary>
    /// ������ �÷��̾��� ���� �ĺ���ȣ ��������
    /// </summary>
    /// <param name="player">���濡�� �����ϴ� �÷��̾� ������Ʈ</param>
    /// <returns></returns>
    public int GetActorNumber(Player player = null)
    {
        // �÷��̾ �������� �ʾ����� �ڽ��� ����
        if (player == null)
        {
            player = PhotonNetwork.LocalPlayer;
        }

        // Debug.Log($"{player.NickName} �÷��̾��� �ĺ��� : " + player.ActorNumber);

        // ������ �÷��̾��� ���� �ĺ��� ��ȯ
        return player.ActorNumber;
    }

    /// <summary>
    /// ���� ��Ʈ��ũ���� ������ ������Ʈ ��ü ����Ʈ ����
    /// </summary>
    /// <param name="_gameObjects"> ������ ���� ������Ʈ ����Ʈ</param>
    public void DestroyGameObjectList(List<GameObject> _gameObjects)
    {
        // ����Ʈ�� ��� ��� ��ü ����
        for (int i = 0; i < _gameObjects.Count; i++)
        {
            // ���濡�� ������ ��ü ����
            PhotonNetwork.Destroy(_gameObjects[i]);
        }
    }

    /// <summary>
    /// ������ Ʈ������ ��ġ �缳��
    /// </summary>
    /// <param name="transform">��ġ�� ������ Ʈ������</param>
    /// <param name="target">�̵��� ��ġ</param>
    public void SetPos(Transform transform, Vector3 target)
    {
        transform.position = target;
    }

    /// <summary>
    /// ���濡�� �����ϴ� �÷��̾ Ű�� �±װ��� �ο�
    /// ���̸��� �װ��� �����ϴ� ���θ� �Ǵ� ���Ǿ� ���γ� ����Ʈ �Ϸ� ���θ� Ȯ�� �ϵ��� ��.
    /// </summary>
    /// <param name="key">�÷��̾ üũ�ؾ��� ����</param>
    /// <param name="value">üũ ���� ����. true/false �Ǵ� Ư���� ���� �Է�</param>
    /// <param name="player">Ű, �±װ��� �ο��� ���濡�� �����ϴ� �÷��̾� ������Ʈ</param>
    public void SetTag(string key, string value, Player player = null)
    {
        // �÷��̾ �������� �ʾ����� �ڽ��� ����
        if (player == null)
        {
            player = PhotonNetwork.LocalPlayer;
        }

        // ������ �÷��̾ Ű, �±װ� �ο�
        player.SetCustomProperties(new Hashtable { { key, value } });
    }

    /// <summary>
    /// ������ ������ Ű�� �´� �±� ���� �����´�.
    /// </summary>
    /// <param name="player">Ű���� ��ȸ�� ���濡�� �����ϴ� �÷��̾� ������Ʈ</param>
    /// <param name="key">��ȸ�� Ű</param>
    /// <returns></returns>
    public object GetTag(Player player, string key)
    {
        // ������ �÷��̾ �ش� Ű�� ������ ���� �ʴٸ� null ��ȯ
        if (player.CustomProperties[key] == null)
        {
            return null;
        }

        // Ű�� ������ ������ �±װ� ��ȯ
        return player.CustomProperties[key].ToString();
    }

    /// <summary>
    /// ��ü ������ ������ Ű���� ����Ǿ� �ִ��� üũ�Ѵ�.
    /// ��ü ������ ���� ������ �̵� �Ǿ��� ��� true ��ȯ
    /// </summary>
    /// <param name="key">��ȸ�� Ű</param>
    /// <returns></returns>
    public bool AllhasTag(string key, string value)
    {
        int getTagPlayerCount = 0;

        // ��ü ���� ����� Ȯ�� �Ҽ� �ֵ��� ��.
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];

            Debug.Log(player.NickName + "/ " + player.ActorNumber + " / "+ key + "/ " + player.CustomProperties[key]);

            // ������ �ش� Ű���� ������ Ȯ��
            if ((string)player.CustomProperties[key] == "True")
            {
                Debug.Log($"{player.NickName} / {key} / {player.CustomProperties[key]}");
                getTagPlayerCount++;
            }
        }

        // ��ü ������ Ű���� �����ϸ� true�� ��ȯ
        if (getTagPlayerCount == PhotonNetwork.PlayerList.Length)
        {
            Debug.Log($"{key} AllhasTag ����� : True");
            return true;
        }
        else
        {
            // 1���̶� Ű���� ������ false ��ȯ
            Debug.Log($"{key} AllhasTag ����� : False");
            return false;
        }
    }
    #endregion Method

    #region Photon Connect

    /// <summary>
    /// ���� �ʱ� ����
    /// </summary>
    private void Setting()
    {
        // �ػ� ����
        //Screen.SetResolution(960, 540, false);

        // ���� ���� ���� ����
        PhotonNetwork.GameVersion = "1";

        // Ŭ���̾�Ʈ�� �󸶳� ���� ��Ű���� ���� ���� ���� / �������� �ְ� �޴� ��Ű���
        // �������� �ְ� �޴� ��Ű���
        PhotonNetwork.SendRate = 40;

        // �����͸� �޾ƾ��� ��
        PhotonNetwork.SerializationRate = 20;

        // ������ �̵��� ������ Ŭ���̾�Ʈ�� ���� �̵�
        PhotonNetwork.AutomaticallySyncScene = true;

        Debug.Log("PhotonNetworkManager / ���� ���� �Ϸ�");
    }

    /// <summary>
    /// ���� ��Ʈ��ũ ����
    /// </summary>
    public void PhotonNetworkConnect()
    {
        // ���� ������ �Ǿ� ���� �ʴٸ� ���� ����
        if (!PhotonNetwork.IsConnected)
        {
            // ���̾�̽� ���� �г��� �ҷ�����
            userNickName = FAM.user.DisplayName;

            // ���� �г��� ����
            PhotonNetwork.NickName = userNickName;

            // �ش� ���ӹ������� photon Ŭ����� ����Ǵ� ������
            // ConnectUsingSettings( ���ӹ��� �������� )
            PhotonNetwork.ConnectUsingSettings();

            Debug.Log("���� ���� ���� ����");
        }
    }

    #endregion Photon Connect
}
