using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSelect : MonoBehaviour
{
    public List<randomlist> ITEM_LIST = new List<randomlist>();
    public int total = 0;  // 카드들의 가중치 총 합

    void Start()
    {
        for (int i = 0; i < ITEM_LIST.Count; i++)
        {
            total += ITEM_LIST[i].weight;
        }
    }

}
