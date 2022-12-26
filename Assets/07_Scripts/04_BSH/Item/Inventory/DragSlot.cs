using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragSlot : MonoBehaviourPun
{
    static public DragSlot instance;              // 싱글톤 구현

    public Slot dragSlot;                         // 드래그 중 저장할 Slot 데이터

    [SerializeField] private Image imageItem;     // 아이템 이미지

    [SerializeField] Transform playerPos;         // 드랍 위치 정보 값
    [SerializeField] Vector3 dropOffset;          // 드랍 위치 차이 값

    [SerializeField] PhotonView PV;

    private void Start()
    {
        instance = this;
    }

    public void DragSetImage(Image _itemImage)    // 드래그 중 아이템 이미지 사용
    {
        imageItem.sprite = _itemImage.sprite;     // 드래그 아이템의 이미지를 아이템 정보와 일치시키기
        SetColor(1);                              // 이미지 보이게 출력
    }

    public void SetColor(float _alpha)            // 알파값으로 이미지 출력 여부 
    {
        Color color = imageItem.color;
        color.a = _alpha;
        imageItem.color = color;                  // 알파값(0)을 받으면 투명, (1)을 받으면 보이게
    }

    public void SetPrefab(string _gameName)   // 아이템을 프리펩 형식으로 생성하기
    {
        // 드랍 아이템의 위치 선정
        dropOffset = playerPos.forward;
        dropOffset.y = 0;
        dropOffset = dropOffset.normalized;
        // 드랍 아이템 프리펩 생성
        var DropItem = PhotonNetwork.Instantiate("Item/" + _gameName, playerPos.position + dropOffset, Quaternion.identity);
    }
}
