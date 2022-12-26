using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    [SerializeField] GameObject m_goPrefab = null;        // 잔해를 자식으로 가지고 있는 잔해 전체 프리펩을 가져와야함
    [SerializeField] private float m_force = 0f;          // 폭발 힘 크기
    [SerializeField] Vector3 m_offset = Vector3.zero;     // 폭발 위치
    [SerializeField] private int go_loofCount = 1;        // 반복생성할 횟수

    public void Explosion()                                                   // 폭발 메서드, 외부에서 해당 메서드를 호출하여 사용
    {
        for (int x = 0; x < go_loofCount; x++)                                // 잔해가 일부일 경우
        {
            GameObject t_clone = Instantiate(m_goPrefab, transform.position, Quaternion.identity);      // 잔해 더미 생성
            Rigidbody[] t_rigids = t_clone.GetComponentsInChildren<Rigidbody>();                        // Rigidbody를 소유한 모든 객체를 받아옴
            for (int i = 0; i < t_rigids.Length; i++)
            {
                t_rigids[i].AddExplosionForce(m_force, transform.position + m_offset, 10f);             // 모든 객체에 폭발 힘 부여
            }
            Destroy(gameObject);     // 파괴 전 오브젝트 파괴( 파괴전 파괴 애니메이션 실행 하는식으로 구현 가능)
            Destroy(t_clone, 5f);    // 잔해 더미를 파괴
        }
    }
}
