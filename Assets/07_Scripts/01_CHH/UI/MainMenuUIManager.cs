using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuUIManager : MonoBehaviour
{
    #region Variable

    [Header("AutoPlayPanel")]
    public GameObject startTitlePanel;  // ������ �г� VAX ū����
    public VideoPlayer vaxAutoPlayVideo;// VAX �ڵ� �÷��� ����
    public float videoPlayTime;         // ���� �� ��� �ð�
    public GameObject endingPanel;      // ���� ũ���� �г�
    public float startTitleTime;        // ���� Ÿ��Ʋ �� �� ������
    private float currentTime;
    private bool isVideoPlaying;        // ���� �÷��� �����ߴ���
    private bool isVaxAutoVideoRestart; // ���� �ٽ� ��� ���� ����

    // ���� �ȵǾ����� �� = �α��� ��
    [Header("DisconnectPanel")]
    public InputField emailInputField;  // ���̵� �Է¶�
    public InputField pwInputField;     // ��й�ȣ �Է¶�
    public Button LoginButton;          // �α��� ��ư

    [Header("SettingPanel")]
    // ���÷��� ���� UI
    public GameObject settingUI;

    #endregion Variable

    #region Unity Method

    private void Awake()
    {
        // �ػ� ����
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

        // �α���â �� ��
        // �α��� â���� ��Ű�� ������ ����
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // �̸��� �Է� �� ��Ű�� ������ ��й�ȣ �Է¶����� ��Ŀ�� �̵�
            if (emailInputField.isFocused == true && emailInputField.text != "")
            {
                pwInputField.Select();
            }
        }
        // �α��� â���� ����Ű�� ������ ����
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            // �̸��ϰ� ��й�ȣ�� ��� �Է��ϰ� ���͸� ������ �α��� ��ư �̺�Ʈ ����
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
    /// ����â ����
    /// </summary>
    public void BtnSettingOpen()
    {
        settingUI.SetActive(true);
    }

    /// <summary>
    /// ����â �ݱ�
    /// </summary>
    public void BtnSettingClose()
    {
        settingUI.SetActive(false);
    }


    /// <summary>
    /// ��������
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
