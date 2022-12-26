using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class myItemParent : MonoBehaviour        // 아이템 박스에서 생성된 아이템들의 부모를 알기 위해 생성한 클래스
{
    public ItemBox m_Pbox;
    private void Start()
    {
        m_Pbox = GetComponentInParent<ItemBox>();
    }
}
