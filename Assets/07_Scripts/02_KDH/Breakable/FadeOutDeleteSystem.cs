using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutDeleteSystem : MonoBehaviour      // ���� �� Rigidbdoy�� ���Ե� ��ü���� ���� �� ��
{
    [SerializeField] private float dieYPosition;      // ���� Y ������

    private float currentTime;  // ���� ��� �ð� ���


    private void Update()
    {

        currentTime += Time.deltaTime;

        if(currentTime > 1)
        {
            if (TryGetComponent<Collider>(out Collider collider))      // 1�� �� �浹ü�� ��Ȱ��ȭ�ؼ� �ٴ����� �������Բ�
            {
                collider.enabled = false;
            }


            if (transform.position.y < dieYPosition)                   // Y��� �������� ���� �����
            {
                Destroy(gameObject);
            }
        }
    }
}
