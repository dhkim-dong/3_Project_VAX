using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class FadeInOut : MonoBehaviour
{
    #region Variable

    public float animTime = 2f;         // Fade 애니메이션 재생 시간 (단위:초).  
    private CanvasRenderer fadeObj;         // 페이드 인 아웃할 객체

    private float start = 1f;           // Mathf.Lerp 메소드의 첫번째 값.  
    private float end = 0f;             // Mathf.Lerp 메소드의 두번째 값.  
    private float time = 0f;            // Mathf.Lerp 메소드의 시간 값.  


    public bool stopIn = false; //false일때 실행되는건데, 초기값을 false로 한 이유는 게임 시작할때 페이드인으로 들어가려고...그게 싫으면 true로 하면됨.
    public bool stopOut = true;

    #endregion Variable

    #region Unity Method

    private void Awake()
    {
        // 컴포넌트 초기화 
        fadeObj = GetComponent<CanvasRenderer>();
    }

    private void Start()
    {

    }

    private void Update()
    {
        // 투명해지는 = FadeIn 애니메이션 재생.  
        if (stopIn == false && time <= 2)
        {
            PlayFadeIn();
        }
        /*if (stopOut == false && time <= 2)
        {
            PlayFadeOut();
        }*/
        if (time >= 2 && stopIn == false)
        {
            stopIn = true;
            time = 0;
            Debug.Log("StopIn");
        }
        /*if (time >= 2 && stopOut == false)
        {
            stopIn = false; //하얗게 전환되고 나서 씬 전환 후 다시 풀거라 넣었다. 그냥 게임 끝낼거면 넣을 필요 없음.
            stopOut = true;
            time = 0;
            Debug.Log("StopOut");
        }*/
    }

    #endregion Unity Method

    #region Method

    // 흰색->투명
    void PlayFadeIn()
    {
        // 경과 시간 계산.  
        // 2초(animTime)동안 재생될 수 있도록 animTime으로 나누기.  
        time += Time.deltaTime / animTime;

        // 알파 값 계산.  
        float alfa = Mathf.Lerp(start, end, time);
        // 계산한 알파 값 다시 설정.  
        fadeObj.SetAlpha(alfa);
        
        // Debug.Log(time);
    }

    // 투명->흰색
    void PlayFadeOut()
    {
        // 경과 시간 계산.  
        // 2초(animTime)동안 재생될 수 있도록 animTime으로 나누기.  
        time += Time.deltaTime / animTime;

        // 알파 값 계산.  
        float alfa = Mathf.Lerp(end, start, time);  //FadeIn과는 달리 start, end가 반대다.
        // 계산한 알파 값 다시 설정.  
        fadeObj.SetAlpha(alfa);
    }

    #endregion Method
}
