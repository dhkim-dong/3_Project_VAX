using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static FirebaseAuthManager;

public class LogInSystem : MonoBehaviour
{
    // 연결 안되어있을 때 = 로그인 전
    // [Header("로그인 화면")]
    [Header("DisconnectPanel")]
    public GameObject disconnectPanel;          // 로그인 UI
    public InputField disconnectEmailInput;     // 이메일 입력 ui
    public InputField disconnectPasswordInput;  // 비밀번호 입력 ui
    public Button disconnectLoginBtn;           // 로그인 버튼
    public Button disconnectRegisterBtn;        // 회원가입 버튼

    // disconnectRegisterBtn을 눌렀을 경우 = 계정 생성 화면
    [Header("RegisterPannel")]
    public GameObject registerUI;               // 생성 UI
    public InputField registerEmail;            // 이메일 입력 ui
    public InputField registerPassword;         // 비밀번호 입력 ui
    public InputField registerNickName;         // 닉네임 입력 ui


    /// <summary>
    /// 회원가입 버튼을 누르면 작동하는 이벤트 메소드
    /// </summary>
    public void OpenCreateUI() // 생성 ui 활성화
    {
        registerUI.SetActive(true);
    }

    /// <summary>
    /// 회원가입 화면에 있는 닫기 버튼에 등록할 이벤트 메소드
    /// </summary>
    public void CloseCreateUI() // 생성 ui 비활성화
    {
        registerUI.SetActive(false);
    }

    /// <summary>
    /// 회원가입 화면에 있는 생성 버튼에 등록할 이벤트 메소드
    /// </summary>
    public void Create() // 계정 생성 및 데이터 등록
    {
        FAM.Create(registerEmail.text.ToString(), registerPassword.text.ToString(), registerNickName.text.ToString()); // 계정 생성 및 데이터 등록
    }

    /// <summary> 
    /// 로그인 화면에 있는 로그인 버튼에 등록할 이벤트 메소드
    /// </summary>
    public void LogIn()
    {
        FAM.Login(disconnectEmailInput.text.ToString(), disconnectPasswordInput.text.ToString());  // user는 정보를 담기 위해 설정
    }


    /// <summary>
    /// 팝업 화면에 있는 닫기 버튼
    /// </summary>
    public void CloseInfoUI()
    {
        FAM.popUpUI.SetActive(false);
    }
}
