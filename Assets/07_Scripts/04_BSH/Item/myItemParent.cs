using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class myItemParent : MonoBehaviour        // ������ �ڽ����� ������ �����۵��� �θ� �˱� ���� ������ Ŭ����
{
    public ItemBox m_Pbox;
    private void Start()
    {
        m_Pbox = GetComponentInParent<ItemBox>();
    }
}
