using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuUIManager : MonoBehaviour
{
    #region Variable

    [Header("AutoPlayPanel")]
    public GameObject startTitlePanel;  // 오른쪽 패널 VAX 큰글자
    public VideoPlayer vaxAutoPlayVideo;// VAX 자동 플레이 비디오
    public float videoPlayTime;         // 비디오 총 재생 시간
    public GameObject endingPanel;      // 엔딩 크레딧 패널
    public float startTitleTime;        // 시작 타이틀 몇 보 볼건지
    private float currentTime;
    private bool isVideoPlaying;        // 비디오 플레이 시작했는지
    private bool isVaxAutoVideoRestart; // 비디오 다시 재생 가능 여부

    // 연결 안되어있을 때 = 로그인 전
    [Header("DisconnectPanel")]
    public InputField emailInputField;  // 아이디 입력란
    public InputField pwInputField;     // 비밀번호 입력란
    public Button LoginButton;          // 로그인 버튼

    [Header("SettingPanel")]
    // 디스플레이 설정 UI
    public GameObject settingUI;

    #endregion Variable

    #region Unity Method

    private void Awake()
    {
        // 해상도 설정
        Screen.SetResolution(1920, 1080, true);
    }

    private void Start()
    {
        VaxAutoPlayVideoInit();
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        if (isVideoPlaying == false && endingPanel.activeSelf == false)
        {
            if (currentTime > startTitleTime)
            {
                startTitlePanel.SetActive(false);
                vaxAutoPlayVideo.Play();
                isVideoPlaying = true;
            }
        }
        else if(currentTime > videoPlayTime)
        {
            endingPanel.SetActive(true);
        }

        // 로그인창 일 때
        // 로그인 창에서 탭키를 누르면 실행
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // 이메일 입력 후 탭키를 누르면 비밀번호 입력란으로 포커스 이동
            if (emailInputField.isFocused == true && emailInputField.text != "")
            {
                pwInputField.Select();
            }
        }
        // 로그인 창에서 엔터키를 누르면 실행
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            // 이메일과 비밀번호를 모두 입력하고 엔터를 누르면 로그인 버튼 이벤트 실행
            if (emailInputField.text != "" && pwInputField.text != "")
            {
                LoginButton.onClick.Invoke();
            }
        }
    }

    #endregion Unity Method

    #region Method

    public void VaxAutoPlayVideoInit()
    {
        currentTime = 0f;
        startTitlePanel.SetActive(true);
        vaxAutoPlayVideo.Stop();
        endingPanel.SetActive(false);
        isVideoPlaying = false;
        isVaxAutoVideoRestart = false;
    }

    /// <summary>
    /// 설정창 열기
    /// </summary>
    public void BtnSettingOpen()
    {
        settingUI.SetActive(true);
    }

    /// <summary>
    /// 설정창 닫기
    /// </summary>
    public void BtnSettingClose()
    {
        settingUI.SetActive(false);
    }


    /// <summary>
    /// 게임종료
    /// </summary>
    public void BtnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion Method
}
