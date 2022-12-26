using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    [SerializeField] GameObject m_goPrefab = null;        // ���ظ� �ڽ����� ������ �ִ� ���� ��ü �������� �����;���
    [SerializeField] private float m_force = 0f;          // ���� �� ũ��
    [SerializeField] Vector3 m_offset = Vector3.zero;     // ���� ��ġ
    [SerializeField] private int go_loofCount = 1;        // �ݺ������� Ƚ��

    public void Explosion()                                                   // ���� �޼���, �ܺο��� �ش� �޼��带 ȣ���Ͽ� ���
    {
        for (int x = 0; x < go_loofCount; x++)                                // ���ذ� �Ϻ��� ���
        {
            GameObject t_clone = Instantiate(m_goPrefab, transform.position, Quaternion.identity);      // ���� ���� ����
            Rigidbody[] t_rigids = t_clone.GetComponentsInChildren<Rigidbody>();                        // Rigidbody�� ������ ��� ��ü�� �޾ƿ�
            for (int i = 0; i < t_rigids.Length; i++)
            {
                t_rigids[i].AddExplosionForce(m_force, transform.position + m_offset, 10f);             // ��� ��ü�� ���� �� �ο�
            }
            Destroy(gameObject);     // �ı� �� ������Ʈ �ı�( �ı��� �ı� �ִϸ��̼� ���� �ϴ½����� ���� ����)
            Destroy(t_clone, 5f);    // ���� ���̸� �ı�
        }
    }
}
