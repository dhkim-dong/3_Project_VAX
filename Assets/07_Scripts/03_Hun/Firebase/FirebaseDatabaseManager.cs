using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.UI;

public class FirebaseDatabaseManager : MonoBehaviour
{
    public InputField email; // email 입력 UI
    public InputField password; // password 입력 UI

    public void ReadID() // 계정 정보 읽기
    {
        DatabaseReference AuthManager = FirebaseDatabase.DefaultInstance.GetReference("AuthManager"); // Firebase DB 저장소에 AuthManager 항목 불러오기
        AuthManager.GetValueAsync().ContinueWithOnMainThread(
            task =>
            {
                if (task.IsFaulted) // AuthManager 항목이 없는 경우
                {
                    Debug.LogError("ReadError"); // 에러 
                }
                else if (task.IsCompleted) // 성공적으로 접근했을 경우
                {
                    DataSnapshot snapshot = task.Result; // 읽어온 항목
                    Debug.Log("ChildrenCount : " + snapshot.ChildrenCount); // AuthManager의 자식 수
                    
                    foreach (var message in snapshot.Children) // 키, 아이디, 
                    {
                        Debug.Log(message.Key + "\n"  
                                  + "Id : " + message.Child("Id").Value.ToString() + "\n" 
                                  + "Pw : " + message.Child("Pw").Value.ToString());
                    }
                }
            });
    }

    public void WriteID() // 계정 추가
    {
        DatabaseReference AuthManager = FirebaseDatabase.DefaultInstance.GetReference("AuthManager"); // AuthManager 항목에
        // string key = AuthManager.Push().Key;
        string key = GetUserID(); // email에 @앞에 문자만 가져오는거 추후 @ 2개 이상 방지 코드 필요

        Dictionary<string, string> authDic = new Dictionary<string, string>(); // 계정에 저장할 dic
        authDic.Add("Id", email.text.ToString()); // email 저장
        authDic.Add("Pw", password.text.ToString()); // password 저장

        Dictionary<string, object> updateAuth = new Dictionary<string, object>(); // 계정명과 앞의 정보 dic
        updateAuth.Add(key,authDic);

        AuthManager.UpdateChildrenAsync(updateAuth).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Update Complete"); // 제대로 됐는지 확인용
            }
        });
    }

    private string GetUserID() // @앞의 문자만 가져오는 메소드
    {
        string[] splitId = email.text.Split('@');
        return splitId[0];
    }
}
