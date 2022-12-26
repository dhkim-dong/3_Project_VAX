using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ItemPickUp : MonoBehaviourPun  // 아이템을 담기 위한 클래스
{
    public Item item;
    public PhotonView PV;
    public ItemBox itembox;  //생성될시 아이템박스 오브젝트를 추적하기 위해 선언 
    public int viewID;    //아이템박스의 포톤뷰에 접근하기위해 선언

}
