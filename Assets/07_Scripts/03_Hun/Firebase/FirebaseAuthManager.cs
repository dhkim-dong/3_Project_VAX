#define Case1 // ���� ���� ���� �˸� �޼���


using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FirebaseAuthManager : MonoBehaviour
{
    #region Variable

    // �̱����� �Ҵ�� static ����
    private static FirebaseAuthManager instance;

    // �ܺο��� �̱��� ������Ʈ�� ������ �� ����� ������Ƽ
    public static FirebaseAuthManager FAM
    {
        get
        {
            // ���� �̱��� ������ ���� ������Ʈ�� �Ҵ���� �ʾҴٸ�
            if (instance == null)
            {
                // ������ FirebaseManager ������Ʈ�� ã�� �Ҵ�
                instance = FindObjectOfType<FirebaseAuthManager>();
            }

            // �̱��� ������Ʈ�� ��ȯ
            return instance;
        }
    }

    // Firebase ����
    private FirebaseAuth auth; // ����
    private FirebaseDatabase db; // �����ͺ��̽�
    public FirebaseUser user; // classUser

    private bool isError; // ���� �߻� ���� Ȯ�ο� ���� ����
    private string errorText; // ���� �޼��� ���� ����
    private bool isNickNameNull = true; // �г��� �ȳ־����� Ȯ��


    [Header("�˾� â")]
    public GameObject popUpUI; // �˾� â UI
    public Text titleText; // �˾� â Ÿ��Ʋ �ؽ�Ʈ
    public Text contentText; // �˾� â �ؽ�Ʈ
    #endregion Variable

    private void Awake()
    {
        // �̱��� ��ü 1���� �����ϵ��� ����
        if (instance == null)
        {
            instance = this;

            // ���� �̵��ϴ��� ������� �ʵ��� ����
            DontDestroyOnLoad(this);
        }
        // �̱��� ��ü�� 1�� �̻� ������ ��� ���Ŀ� ������ ��ü ����
        else
        {
            // �κ������ ���� �α׾ƿ� �� �ٽ� ���θ޴������� ���ư� ���� �̹� ������ ��ü�� ����
            if(SceneManager.GetActiveScene().name == "01_MainMenuManager")
            {
                Destroy(instance.gameObject);
            }
            // �̹� ��ü�� ������ ��� �������� ������ ��ü�� ���� 
            else
            {
                Destroy(this);
            }
        }

        Init();
    }

    private void Start()
    {
        Init();
    }

    /// <summary>
    /// ���̾�̽� �ʱ� ����
    /// </summary>
    private void Init()
    {
        auth = FirebaseAuth.DefaultInstance;

        /*popUpUI = GameObject.Find("MainMenu_Canvas").transform.GetChild(5).gameObject;
        titleText = popUpUI.transform.GetChild(2).transform.GetChild(0).GetComponent<Text>();
        contentText = popUpUI.transform.GetChild(3).GetComponent<Text>();*/
        //popUpUI.SetActive(false);


        // db = FirebaseDatabase.DefaultInstance;
        user = null;

        isError = false;
        errorText = "";
    }

    /// <summary>
    /// Firebase Auth������ ���� ����
    /// </summary>
    /// <param nickName="email">�̸���</param>
    /// <param nickName="password">��й�ȣ</param>
    /// <param nickName="nickName">�г���</param>
    public void Create(string email, string password, string nickName) // ���� ����
    {
        // ����, informText ���� �ʱ�ȭ
        isError = false;
        errorText = null;
        isNickNameNull = false;

        // �̸��� Ȥ�� ��й�ȣ �� �� Ȯ��
        if (email == "" || password == "")
        {
            errorText = "�̸����̳� ��й�ȣ��\n�Է����� �ʾҽ��ϴ�.";
            isError = true;
        }
        // �̸��� @�տ� a~Z, 0~9���ڿ��� 2~16�ڸ�
        if (!ChkEmail(email))
        {

            contentText.text = "�̸��� ������ �߸��Ǿ����ϴ�.";
            popUpUI.SetActive(true);
            return;
        }

        // ���� ���� �޼ҵ�
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            // ���� ���α׷����� �����带 ��� ���� ���� ����� ��ҵǴ� ���
            if (task.IsCanceled)
            {
                // Debug.Log("ȸ������ ���");

                // �˾� â�� ���� ���� ���� ����
                errorText = "REGISTER CANCEL";
            }

            // Firebase���� ���� ������ ���ϰ� ���� ���
            else if (task.IsFaulted)
            {
                // ���� �ڵ带 ���� ������ �޾ƿ´�
                int errorCode = GetFirebaseErrorCode(task.Exception);

                // ���� ��Ȳ ǥ��
                isError = true;

                // ���� �ڵ忡 �´� �˸� ���� ����
                switch (errorCode)
                {
                    // �̸����� �̹� ����
                    case 8:
                        // �˾� â�� ���� ���� ���� ����
                        errorText = "�̸����� �̹� �����մϴ�.";
                        break;

                    // �̸��� ���� ���� 
                    case 11:
                        // �˾� â�� ���� ���� ���� ����
                        errorText = "�̸��� ������ �߸��Ǿ����ϴ�.";
                        break;

                    // ��й�ȣ 6�ڸ� �̸�
                    case 23:
                        // �˾� â�� ���� ���� ���� ����
                        errorText = "��� ��ȣ�� 6 �ڸ� �̻����� ������ �ּ���.";
                        break;

                    // ���ͳ� ��û ����
                    case 19:
                        // �˾� â�� ���� ���� ���� ����
                        errorText = "���ͳ� ��û ����";
                        break;

                    default:
                        break;
                }
            }

            // ȸ�� ������ ���������� �Ǵ� ���
            else if (task.IsCompleted)
            {
                // ���� �����̶� user�� task ����� ���� �ʴ´�
                FirebaseUser newUser = task.Result;
                
                if (nickName == "")
                {
                    isNickNameNull = true;
                }

                if (isNickNameNull)
                {
                    nickName = "User" + UnityEngine.Random.Range(10000, 99999);
                    // PNM.userNickName = newNickName;
                    isNickNameNull = false;
                }

                // newUser�� DisplayName �Ӽ��� nickName���� ����
                UserProfile profile = new UserProfile { DisplayName = nickName };
                newUser.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task =>
                {
                    /*
                    if (task.IsCompleted)
                    {
                        // Debug.Log(newUser.DisplayName); 
                        // Debug.Log(auth.CurrentUser.DisplayName);
                    }*/
                });

                // ������ ���� �����͸� Firebase database�� ���
                // SaveUserData(nickName);

#if Case1
                errorText = $"{nickName}\nREGISTER SUCCESS";
                contentText.text = errorText;
                titleText.text = "REGISTER";
                popUpUI.SetActive(true);
#endif
                // �г����� �������� �ʾ����� ���� �г��� �ο�
                // Debug.Log("ȸ������ �Ϸ�"); 
            }

            // ������ �ִ� ���
            if (isError)
            {
                Debug.Log(errorText); 

                // �˾�â ���� ���� �� Ȱ��ȭ
                contentText.text = errorText;
                popUpUI.SetActive(true);
            }

            return; // task ����
        });
    }

    /// <summary>
    /// �α��� ��ư ���� �� �߻��ϴ� �̺�Ʈ
    /// </summary>
    /// <param nickName="email">Firebase�� ��ϵ� �̸���</param>
    /// <param nickName="password">�̸��ϰ� �´� ��й�ȣ</param>
    /// <param nickName="classUser">Firebase DB�� ��ϵ� ������ �޾ƿ� ��ü</param>
    public void Login(string email, string password)
    {
        Debug.Log("���̾�̽� �α���");

        // ���� ����, �˸� ���� �ʱ�ȭ
        isError = false;
        errorText = null;

        // �α��� 
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            // ���� ���α׷����� �����带 ��� ���� ���� ����� ��ҵǴ� ���
            if (task.IsCanceled)
            {
                // Debug.LogError("�α��� ���");

                // �˾� â�� ���� ���� ���� ����
                errorText = "LOGIN CANCEL";
            }

            // Firebase���� ���� �α����� ���ϰ� ���� ���
            else if (task.IsFaulted)
            {
                // ���� �ڵ带 ���� ������ �޾ƿ´�
                var errorCode = GetFirebaseErrorCode(task.Exception);

                // ���� ��Ȳ ǥ��
                isError = true;

                // ���� �ڵ忡 �´� �˸� ���� ����
                switch (errorCode)
                {
                    // �̸��� ���� ����
                    case 11:
                        // �˾� â�� ���� ���� ���� ����
                        errorText = "�̸��� ������ �߸��Ǿ����ϴ�.";
                        break;

                    // ��й�ȣ ����
                    case 12:
                        // �˾� â�� ���� ���� ���� ����
                        errorText = "�������� �ʴ� �̸��� �̰ų�\n��й�ȣ�� �߸� �Է��Ͽ����ϴ�.";
                        break;

                    // �������� �ʴ� �̸���
                    case 14:
                        // �˾� â�� ���� ���� ���� ����
                        errorText = "�������� �ʴ� �̸��� �̰ų�\n��й�ȣ�� �߸� �Է��Ͽ����ϴ�.";
                        break;

                    // ���ͳ� ��û ����
                    case 19:
                        // �˾� â�� ���� ���� ���� ����
                        errorText = "���ͳ� ��û �����߽��ϴ�.";
                        break;

                    default:
                        break;
                }
            }

            // �α����� ���������� �Ǵ� ���
            else 
            {
                Debug.Log("LOGIN SUCCESS");

                // user�� ����� ��´�
                user = task.Result;

                Debug.Log("user DisplayName ; " + user.DisplayName);

                // �α��� ������ �κ�� �̵�
                SceneManager.LoadScene("02_LobbyScene");
            }

            // ������ �ִ� ���
            if (isError)
            {
                // Debug.Log(informText);

                // �˾�â ���� ���� �� Ȱ��ȭ
                contentText.text = errorText;
                popUpUI.SetActive(true);
            }

            return; // task ����
        });
    }

    /// <summary>
    /// �α׾ƿ� ���� �ʱ�ȭ
    /// </summary>
    /// <param nickName="classUser">�г���, ��, ��, ���ھ� ������ ��� �ִ� Ŭ����</param>
    public void Logout(User classUser = null)
    {
        // ���� �ʱ�ȭ
        auth.SignOut();

        // user ������ �ʱ�ȭ
        this.user = null;


        // classUser ������ �ʱ�ȭ
        if (classUser != null)
        {
            classUser = null;
        }

        // Debug.Log("�α׾ƿ� �Ϸ�");
        return;
    }

    // ���� FirebaseDatabaseManager�� �ְ� ������ �ʿ�!
    #region �����ͺ��̽� 
    /*
    /// <summary>
    /// ������ ������ ������ ���̽��� ���
    /// ��, ��, ����, �г����� ��´�
    /// </summary>
    /// <param nickName="nickName">�ε��� �̸�</param>
    public void SaveUserData(string nickName)
    {
        // User �׸� ����Ѵ�
        DatabaseReference database = db.GetReference("User"); // Ű ���� ������ ��� ����
                                                              // ���� Ű ���� nickName ���Ŀ� emailInput �տ� ���� �Ϸ��� ���� �ʼ�!
                                                              // User > key �׸��� ��������

        // ���Ŀ� User�� nickName �׸� �ִ��� �̹� �����ϴ��� Ȯ���ϴ� �ڵ� ����
        // �̹� �����ϸ� ����? �߻�����!

        // Ŭ����(��ü)�� json���Ϸ� ��ȯ
        var data = new User(nickName);
        string jsonData = JsonUtility.ToJson(data); // ��ü�� json���Ϸ� ��ȯ


        // jsonData�� ��ȯ�� ���� nickName�� �ε����� Firebase database�� ���
        database.Child(nickName).SetRawJsonValueAsync(jsonData); // database�� nickName ĭ ����� ������ ���
                                                                 // ������ ���� ������
                                                                 // Ű ���� ������ ������ ���ĵȴ�

        return;
    }

    /// <summary>
    /// Firebase�� database�� ������ �ҷ�����
    /// </summary>
    /// <param name="key">User�� �ִ� ������ �ε��� �̸�</param>
    /// <param name="classUser">key�� �ش��ϴ� ������ ���� ��ü</param>
    public void LoadUserData(string key)
    {
        // User�׸��� �������� ����
        DatabaseReference database = db.GetReference("User"); // User �׸��� ������ ���̽� 
                                                              // User ������ ���� ����
                                                              // Ű�� �ִ��� üũ �ϱ� ���� ���� ���� User�� �ߴ�

        // ���� ��������
        database.Child(key).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            // ���еǴ� ���
            if (task.IsCanceled)
            {
                // Debug.Log("�б� ���");
            }

            // ������ ������ �Ǽ��� ������ ��� ���� ���� ���غ��� ��?��...
            else if (task.IsFaulted)
            {
                // Debug.Log("�б� ����");
            }

            //������ ���
            else if (task.IsCompleted)
            {
                // IDictionary Ű �ڷ����� string, �� �ڷ����� object�̴�
                var snapshot = (IDictionary)(task.Result.Value);


                // ���� snapshot�� �Ʒ� ���� ������ �Ʒ� ���� �۵� ���Ѵ�
                // int���� string�� �޾ƿ��� ���
                // name = (string)snapshot["nickName"]; // ������ ���°� ���ڿ��̹Ƿ� ���ڿ��� ��ȯ�Ȱ�
                // win = (Convert.ToInt32(snapshot["win"])); // int.Parse((string)snapshot["win"]); �����ȵ�
                // lose = (Convert.ToInt32(snapshot["lose"]));
                // score = (Convert.ToInt32(snapshot["score"]));

                // Debug.Log("���� ���� �� �޾ƿԽ��ϴ�.");
            }
        });
    }
    */
    #endregion �����ͺ��̽�

    /// <summary>
    /// �̸��Ͽ��� @ �տ� ���ڿ��� �޾ƿ��� �޼ҵ�
    /// </summary>
    /// <param name="str">�̸���</param>
    /// <returns></returns>
    [Obsolete("����� �Ⱦ��� �޼ҵ� ���Ŀ� ���� �� ��Ʈ����Ʈ ���� �ʼ�!")]
    private string GetKeyCode(string str) // �����ͺ��̽��� ����� Ű�� �������� �޼ҵ�
    {
        return str.Split('@')[0]; // �̸��� @ �տ� ���ڿ��� Ű�� ����
    }

    /// <summary>
    /// ���̾� ���̽� ���� �ڵ带 ���ڷ� ��ȯ
    /// </summary>
    /// <param name="exception">����? ���� Ŭ���� �ֱ�</param>
    /// <returns></returns>
    private int GetFirebaseErrorCode(AggregateException exception)
    {
        // �ʱ�ȭ
        FirebaseException firebaseException = null;

        // ���̾�̽��� �ش�Ǵ� exception���� Ȯ�� 
        foreach (Exception e in exception.Flatten().InnerExceptions)
        {
            // �� ��ȯ �ȵǸ� null
            firebaseException = e as FirebaseException;

            if (firebaseException != null) // �����ϸ� �ٷ� ��ȯ
            {
                break;
            }
        }

        return firebaseException?.ErrorCode ?? 0; // ?? �� null �� ��� 0���� ��ȯ
    }

    public bool ChkEmail(string Email)
    {
        string chkStr = @"^[A-z0-9]{2,16}[@][A-z0-9]{2,8}[.][A-z0-9]{2,4}$";

        Regex reg = new Regex(chkStr);
        if (reg.IsMatch(Email))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
