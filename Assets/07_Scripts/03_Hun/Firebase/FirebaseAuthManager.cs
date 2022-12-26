#define Case1 // 계정 가입 성공 알림 메세지


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

    // 싱글톤이 할당될 static 변수
    private static FirebaseAuthManager instance;

    // 외부에서 싱글톤 오브젝트를 가져올 때 사용할 프로퍼티
    public static FirebaseAuthManager FAM
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (instance == null)
            {
                // 씬에서 FirebaseManager 오브젝트를 찾아 할당
                instance = FindObjectOfType<FirebaseAuthManager>();
            }

            // 싱글톤 오브젝트를 반환
            return instance;
        }
    }

    // Firebase 변수
    private FirebaseAuth auth; // 계정
    private FirebaseDatabase db; // 데이터베이스
    public FirebaseUser user; // classUser

    private bool isError; // 에러 발생 여부 확인용 상태 변수
    private string errorText; // 에러 메세지 담을 변수
    private bool isNickNameNull = true; // 닉네임 안넣었는지 확인


    [Header("팝업 창")]
    public GameObject popUpUI; // 팝업 창 UI
    public Text titleText; // 팝업 창 타이틀 텍스트
    public Text contentText; // 팝업 창 텍스트
    #endregion Variable

    private void Awake()
    {
        // 싱글톤 객체 1개만 존재하도록 설정
        if (instance == null)
        {
            instance = this;

            // 씬을 이동하더라도 사라지지 않도록 설정
            DontDestroyOnLoad(this);
        }
        // 싱글톤 객체가 1개 이상 존재할 경우 이후에 생성된 객체 삭제
        else
        {
            // 로비씬에서 포톤 로그아웃 후 다시 메인메뉴씬으로 돌아갈 때는 이미 생성된 객체를 삭제
            if(SceneManager.GetActiveScene().name == "01_MainMenuManager")
            {
                Destroy(instance.gameObject);
            }
            // 이미 객체가 존재할 경우 다음으로 생성된 객체를 삭제 
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
    /// 파이어베이스 초기 설정
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
    /// Firebase Auth서버에 계정 생성
    /// </summary>
    /// <param nickName="email">이메일</param>
    /// <param nickName="password">비밀번호</param>
    /// <param nickName="nickName">닉네임</param>
    public void Create(string email, string password, string nickName) // 계정 생성
    {
        // 에러, informText 상태 초기화
        isError = false;
        errorText = null;
        isNickNameNull = false;

        // 이메일 혹은 비밀번호 빈 거 확인
        if (email == "" || password == "")
        {
            errorText = "이메일이나 비밀번호를\n입력하지 않았습니다.";
            isError = true;
        }
        // 이메일 @앞에 a~Z, 0~9문자열이 2~16자리
        if (!ChkEmail(email))
        {

            contentText.text = "이메일 형식이 잘못되었습니다.";
            popUpUI.SetActive(true);
            return;
        }

        // 계정 생성 메소드
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            // 내부 프로그램에서 스레드를 취소 시켜 계정 등록이 취소되는 경우
            if (task.IsCanceled)
            {
                // Debug.Log("회원가입 취소");

                // 팝업 창에 담을 에러 문자 설정
                errorText = "REGISTER CANCEL";
            }

            // Firebase에서 계정 생성을 못하게 막는 경우
            else if (task.IsFaulted)
            {
                // 에러 코드를 정수 값으로 받아온다
                int errorCode = GetFirebaseErrorCode(task.Exception);

                // 에러 상황 표시
                isError = true;

                // 에러 코드에 맞는 알림 구문 선택
                switch (errorCode)
                {
                    // 이메일이 이미 존재
                    case 8:
                        // 팝업 창에 담을 에러 문자 설정
                        errorText = "이메일이 이미 존재합니다.";
                        break;

                    // 이메일 형식 에러 
                    case 11:
                        // 팝업 창에 담을 에러 문자 설정
                        errorText = "이메일 형식이 잘못되었습니다.";
                        break;

                    // 비밀번호 6자리 미만
                    case 23:
                        // 팝업 창에 담을 에러 문자 설정
                        errorText = "비밀 번호를 6 자리 이상으로 설정해 주세요.";
                        break;

                    // 인터넷 요청 실패
                    case 19:
                        // 팝업 창에 담을 에러 문자 설정
                        errorText = "인터넷 요청 실패";
                        break;

                    default:
                        break;
                }
            }

            // 회원 가입이 성공적으로 되는 경우
            else if (task.IsCompleted)
            {
                // 생성 구문이라 user에 task 결과를 담지 않는다
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

                // newUser의 DisplayName 속성을 nickName으로 변경
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

                // 생성된 유저 데이터를 Firebase database에 등록
                // SaveUserData(nickName);

#if Case1
                errorText = $"{nickName}\nREGISTER SUCCESS";
                contentText.text = errorText;
                titleText.text = "REGISTER";
                popUpUI.SetActive(true);
#endif
                // 닉네임이 설정되지 않았으면 랜덤 닉네임 부여
                // Debug.Log("회원가입 완료"); 
            }

            // 에러가 있는 경우
            if (isError)
            {
                Debug.Log(errorText); 

                // 팝업창 문자 설정 및 활성화
                contentText.text = errorText;
                popUpUI.SetActive(true);
            }

            return; // task 종료
        });
    }

    /// <summary>
    /// 로그인 버튼 누를 때 발생하는 이벤트
    /// </summary>
    /// <param nickName="email">Firebase에 등록된 이메일</param>
    /// <param nickName="password">이메일과 맞는 비밀번호</param>
    /// <param nickName="classUser">Firebase DB에 등록된 정보를 받아올 객체</param>
    public void Login(string email, string password)
    {
        Debug.Log("파이어베이스 로그인");

        // 에러 상태, 알림 구문 초기화
        isError = false;
        errorText = null;

        // 로그인 
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            // 내부 프로그램에서 스레드를 취소 시켜 계정 등록이 취소되는 경우
            if (task.IsCanceled)
            {
                // Debug.LogError("로그인 취소");

                // 팝업 창에 담을 에러 문자 설정
                errorText = "LOGIN CANCEL";
            }

            // Firebase에서 계정 로그인을 못하게 막는 경우
            else if (task.IsFaulted)
            {
                // 에러 코드를 정수 값으로 받아온다
                var errorCode = GetFirebaseErrorCode(task.Exception);

                // 에러 상황 표시
                isError = true;

                // 에러 코드에 맞는 알림 구문 선택
                switch (errorCode)
                {
                    // 이메일 형식 에러
                    case 11:
                        // 팝업 창에 담을 에러 문자 설정
                        errorText = "이메일 형식이 잘못되었습니다.";
                        break;

                    // 비밀번호 에러
                    case 12:
                        // 팝업 창에 담을 에러 문자 설정
                        errorText = "존재하지 않는 이메일 이거나\n비밀번호를 잘못 입력하였습니다.";
                        break;

                    // 존재하지 않는 이메일
                    case 14:
                        // 팝업 창에 담을 에러 문자 설정
                        errorText = "존재하지 않는 이메일 이거나\n비밀번호를 잘못 입력하였습니다.";
                        break;

                    // 인터넷 요청 실패
                    case 19:
                        // 팝업 창에 담을 에러 문자 설정
                        errorText = "인터넷 요청 실패했습니다.";
                        break;

                    default:
                        break;
                }
            }

            // 로그인이 성공적으로 되는 경우
            else 
            {
                Debug.Log("LOGIN SUCCESS");

                // user에 결과를 담는다
                user = task.Result;

                Debug.Log("user DisplayName ; " + user.DisplayName);

                // 로그인 성공시 로비씬 이동
                SceneManager.LoadScene("02_LobbyScene");
            }

            // 에러가 있는 경우
            if (isError)
            {
                // Debug.Log(informText);

                // 팝업창 문자 설정 및 활성화
                contentText.text = errorText;
                popUpUI.SetActive(true);
            }

            return; // task 종료
        });
    }

    /// <summary>
    /// 로그아웃 정보 초기화
    /// </summary>
    /// <param nickName="classUser">닉네임, 승, 패, 스코어 정보를 담고 있는 클래스</param>
    public void Logout(User classUser = null)
    {
        // 계정 초기화
        auth.SignOut();

        // user 정보도 초기화
        this.user = null;


        // classUser 정보도 초기화
        if (classUser != null)
        {
            classUser = null;
        }

        // Debug.Log("로그아웃 완료");
        return;
    }

    // 추후 FirebaseDatabaseManager에 넣고 수정이 필요!
    #region 데이터베이스 
    /*
    /// <summary>
    /// 유저의 정보를 데이터 베이스에 등록
    /// 승, 패, 점수, 닉네임을 담는다
    /// </summary>
    /// <param nickName="nickName">인덱스 이름</param>
    public void SaveUserData(string nickName)
    {
        // User 항목에 등록한다
        DatabaseReference database = db.GetReference("User"); // 키 값이 없으면 경로 생성
                                                              // 현재 키 값은 nickName 추후에 emailInput 앞에 껄로 하려면 수정 필수!
                                                              // User > key 항목이 시작지점

        // 추후에 User에 nickName 항목 있는지 이미 존재하는지 확인하는 코드 생성
        // 이미 존재하면 에러? 발생으로!

        // 클래스(객체)를 json파일로 변환
        var data = new User(nickName);
        string jsonData = JsonUtility.ToJson(data); // 객체를 json파일로 변환


        // jsonData로 변환된 것을 nickName을 인덱스로 Firebase database에 등록
        database.Child(nickName).SetRawJsonValueAsync(jsonData); // database에 nickName 칸 만들고 데이터 등록
                                                                 // 데이터 정렬 순서는
                                                                 // 키 값의 사전식 순서로 정렬된다

        return;
    }

    /// <summary>
    /// Firebase의 database에 정보를 불러오기
    /// </summary>
    /// <param name="key">User에 있는 가져올 인덱스 이름</param>
    /// <param name="classUser">key에 해당하는 정보를 담을 객체</param>
    public void LoadUserData(string key)
    {
        // User항목을 기준으로 시작
        DatabaseReference database = db.GetReference("User"); // User 항목의 데이터 베이스 
                                                              // User 지점이 시작 지점
                                                              // 키가 있는지 체크 하기 위해 시작 지점 User로 했다

        // 정보 가져오기
        database.Child(key).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            // 실패되는 경우
            if (task.IsCanceled)
            {
                // Debug.Log("읽기 취소");
            }

            // 데이터 정보를 실수로 날리는 경우 아직 실험 안해봐서 몰?루...
            else if (task.IsFaulted)
            {
                // Debug.Log("읽기 실패");
            }

            //성공한 경우
            else if (task.IsCompleted)
            {
                // IDictionary 키 자료형은 string, 값 자료형은 object이다
                var snapshot = (IDictionary)(task.Result.Value);


                // 만약 snapshot에 아래 구문 없으면 아래 구문 작동 안한다
                // int형과 string형 받아오는 방법
                // name = (string)snapshot["nickName"]; // 저장한 형태가 문자열이므로 문자열로 변환된거
                // win = (Convert.ToInt32(snapshot["win"])); // int.Parse((string)snapshot["win"]); 변형안됨
                // lose = (Convert.ToInt32(snapshot["lose"]));
                // score = (Convert.ToInt32(snapshot["score"]));

                // Debug.Log("계정 정보 잘 받아왔습니다.");
            }
        });
    }
    */
    #endregion 데이터베이스

    /// <summary>
    /// 이메일에서 @ 앞에 문자열만 받아오는 메소드
    /// </summary>
    /// <param name="str">이메일</param>
    /// <returns></returns>
    [Obsolete("현재는 안쓰는 메소드 추후에 쓰면 이 어트리뷰트 삭제 필수!")]
    private string GetKeyCode(string str) // 데이터베이스에 등록할 키값 가져오는 메소드
    {
        return str.Split('@')[0]; // 이메일 @ 앞에 문자열로 키값 설정
    }

    /// <summary>
    /// 파이어 베이스 에러 코드를 숫자로 반환
    /// </summary>
    /// <param name="exception">에러? 예외 클래스 넣기</param>
    /// <returns></returns>
    private int GetFirebaseErrorCode(AggregateException exception)
    {
        // 초기화
        FirebaseException firebaseException = null;

        // 파이어베이스에 해당되는 exception인지 확인 
        foreach (Exception e in exception.Flatten().InnerExceptions)
        {
            // 형 변환 안되면 null
            firebaseException = e as FirebaseException;

            if (firebaseException != null) // 존재하면 바로 반환
            {
                break;
            }
        }

        return firebaseException?.ErrorCode ?? 0; // ?? 는 null 인 경우 0으로 반환
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
