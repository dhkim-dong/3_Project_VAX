using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class FadeInOut : MonoBehaviour
{
    #region Variable

    public float animTime = 2f;         // Fade �ִϸ��̼� ��� �ð� (����:��).  
    private CanvasRenderer fadeObj;         // ���̵� �� �ƿ��� ��ü

    private float start = 1f;           // Mathf.Lerp �޼ҵ��� ù��° ��.  
    private float end = 0f;             // Mathf.Lerp �޼ҵ��� �ι�° ��.  
    private float time = 0f;            // Mathf.Lerp �޼ҵ��� �ð� ��.  


    public bool stopIn = false; //false�϶� ����Ǵ°ǵ�, �ʱⰪ�� false�� �� ������ ���� �����Ҷ� ���̵������� ������...�װ� ������ true�� �ϸ��.
    public bool stopOut = true;

    #endregion Variable

    #region Unity Method

    private void Awake()
    {
        // ������Ʈ �ʱ�ȭ 
        fadeObj = GetComponent<CanvasRenderer>();
    }

    private void Start()
    {

    }

    private void Update()
    {
        // ���������� = FadeIn �ִϸ��̼� ���.  
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
            stopIn = false; //�Ͼ�� ��ȯ�ǰ� ���� �� ��ȯ �� �ٽ� Ǯ�Ŷ� �־���. �׳� ���� �����Ÿ� ���� �ʿ� ����.
            stopOut = true;
            time = 0;
            Debug.Log("StopOut");
        }*/
    }

    #endregion Unity Method

    #region Method

    // ���->����
    void PlayFadeIn()
    {
        // ��� �ð� ���.  
        // 2��(animTime)���� ����� �� �ֵ��� animTime���� ������.  
        time += Time.deltaTime / animTime;

        // ���� �� ���.  
        float alfa = Mathf.Lerp(start, end, time);
        // ����� ���� �� �ٽ� ����.  
        fadeObj.SetAlpha(alfa);
        
        // Debug.Log(time);
    }

    // ����->���
    void PlayFadeOut()
    {
        // ��� �ð� ���.  
        // 2��(animTime)���� ����� �� �ֵ��� animTime���� ������.  
        time += Time.deltaTime / animTime;

        // ���� �� ���.  
        float alfa = Mathf.Lerp(end, start, time);  //FadeIn���� �޸� start, end�� �ݴ��.
        // ����� ���� �� �ٽ� ����.  
        fadeObj.SetAlpha(alfa);
    }

    #endregion Method
}
