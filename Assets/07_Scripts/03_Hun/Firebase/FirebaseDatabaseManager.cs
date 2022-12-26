using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.UI;

public class FirebaseDatabaseManager : MonoBehaviour
{
    public InputField email; // email �Է� UI
    public InputField password; // password �Է� UI

    public void ReadID() // ���� ���� �б�
    {
        DatabaseReference AuthManager = FirebaseDatabase.DefaultInstance.GetReference("AuthManager"); // Firebase DB ����ҿ� AuthManager �׸� �ҷ�����
        AuthManager.GetValueAsync().ContinueWithOnMainThread(
            task =>
            {
                if (task.IsFaulted) // AuthManager �׸��� ���� ���
                {
                    Debug.LogError("ReadError"); // ���� 
                }
                else if (task.IsCompleted) // ���������� �������� ���
                {
                    DataSnapshot snapshot = task.Result; // �о�� �׸�
                    Debug.Log("ChildrenCount : " + snapshot.ChildrenCount); // AuthManager�� �ڽ� ��
                    
                    foreach (var message in snapshot.Children) // Ű, ���̵�, 
                    {
                        Debug.Log(message.Key + "\n"  
                                  + "Id : " + message.Child("Id").Value.ToString() + "\n" 
                                  + "Pw : " + message.Child("Pw").Value.ToString());
                    }
                }
            });
    }

    public void WriteID() // ���� �߰�
    {
        DatabaseReference AuthManager = FirebaseDatabase.DefaultInstance.GetReference("AuthManager"); // AuthManager �׸�
        // string key = AuthManager.Push().Key;
        string key = GetUserID(); // email�� @�տ� ���ڸ� �������°� ���� @ 2�� �̻� ���� �ڵ� �ʿ�

        Dictionary<string, string> authDic = new Dictionary<string, string>(); // ������ ������ dic
        authDic.Add("Id", email.text.ToString()); // email ����
        authDic.Add("Pw", password.text.ToString()); // password ����

        Dictionary<string, object> updateAuth = new Dictionary<string, object>(); // ������� ���� ���� dic
        updateAuth.Add(key,authDic);

        AuthManager.UpdateChildrenAsync(updateAuth).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Update Complete"); // ����� �ƴ��� Ȯ�ο�
            }
        });
    }

    private string GetUserID() // @���� ���ڸ� �������� �޼ҵ�
    {
        string[] splitId = email.text.Split('@');
        return splitId[0];
    }
}
