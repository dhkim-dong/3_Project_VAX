using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class randomlist    //리스트를 받기위한 공간 원래 하던방식은 확률따로 ,아이템따로라 검사를 두번씩 하는것도 문제인데  첫번째 배열아이템이 너무 유리하게 나와서 그냥 하나로 합침 
{
    public GameObject items;  //아이템 프리팹 넣으면 됩니다
    public int weight;       //가중치는 직접 원하는 만큼 설정하시면 됩니다 

}
