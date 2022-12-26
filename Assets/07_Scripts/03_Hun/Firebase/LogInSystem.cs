using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static FirebaseAuthManager;

public class LogInSystem : MonoBehaviour
{
    // ���� �ȵǾ����� �� = �α��� ��
    // [Header("�α��� ȭ��")]
    [Header("DisconnectPanel")]
    public GameObject disconnectPanel;          // �α��� UI
    public InputField disconnectEmailInput;     // �̸��� �Է� ui
    public InputField disconnectPasswordInput;  // ��й�ȣ �Է� ui
    public Button disconnectLoginBtn;           // �α��� ��ư
    public Button disconnectRegisterBtn;        // ȸ������ ��ư

    // disconnectRegisterBtn�� ������ ��� = ���� ���� ȭ��
    [Header("RegisterPannel")]
    public GameObject registerUI;               // ���� UI
    public InputField registerEmail;            // �̸��� �Է� ui
    public InputField registerPassword;         // ��й�ȣ �Է� ui
    public InputField registerNickName;         // �г��� �Է� ui


    /// <summary>
    /// ȸ������ ��ư�� ������ �۵��ϴ� �̺�Ʈ �޼ҵ�
    /// </summary>
    public void OpenCreateUI() // ���� ui Ȱ��ȭ
    {
        registerUI.SetActive(true);
    }

    /// <summary>
    /// ȸ������ ȭ�鿡 �ִ� �ݱ� ��ư�� ����� �̺�Ʈ �޼ҵ�
    /// </summary>
    public void CloseCreateUI() // ���� ui ��Ȱ��ȭ
    {
        registerUI.SetActive(false);
    }

    /// <summary>
    /// ȸ������ ȭ�鿡 �ִ� ���� ��ư�� ����� �̺�Ʈ �޼ҵ�
    /// </summary>
    public void Create() // ���� ���� �� ������ ���
    {
        FAM.Create(registerEmail.text.ToString(), registerPassword.text.ToString(), registerNickName.text.ToString()); // ���� ���� �� ������ ���
    }

    /// <summary> 
    /// �α��� ȭ�鿡 �ִ� �α��� ��ư�� ����� �̺�Ʈ �޼ҵ�
    /// </summary>
    public void LogIn()
    {
        FAM.Login(disconnectEmailInput.text.ToString(), disconnectPasswordInput.text.ToString());  // user�� ������ ��� ���� ����
    }


    /// <summary>
    /// �˾� ȭ�鿡 �ִ� �ݱ� ��ư
    /// </summary>
    public void CloseInfoUI()
    {
        FAM.popUpUI.SetActive(false);
    }
}
