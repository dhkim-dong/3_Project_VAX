using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutDeleteSystem : MonoBehaviour      // 폭발 후 Rigidbdoy가 포함된 객체에만 적용 할 것
{
    [SerializeField] private float dieYPosition;      // 제거 Y 포지션

    private float currentTime;  // 낙하 대기 시간 계산


    private void Update()
    {

        currentTime += Time.deltaTime;

        if(currentTime > 1)
        {
            if (TryGetComponent<Collider>(out Collider collider))      // 1초 후 충돌체를 비활성화해서 바닥으로 떨어지게끔
            {
                collider.enabled = false;
            }


            if (transform.position.y < dieYPosition)                   // Y기반 데드존에 의해 사라짐
            {
                Destroy(gameObject);
            }
        }
    }
}
