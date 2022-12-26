using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;

public class Slot : MonoBehaviourPunCallbacks, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Item item; // 획득한 아이템
    public int itemCount; // 획득한 아이템의 개수
    public Image itemImage; // 아이템의 이미지


    // 필요 컴포넌트
    [SerializeField] private Text text_Count; // 아이템의 수
    [SerializeField] private GameObject go_CountImage; // 오브젝트 온 오프
    [SerializeField] private ItemEffectDatabase itemEffectDatabase;

    public PhotonView PV;

    //[SerializeField] private WeaponManager theWeaponManager;

    private void Update()
    {
        if (PV.gameObject.GetComponent<ActionController>().isKeyUse)
            SlotDoorKeyUse();
    }

    private void SlotDoorKeyUse()
    {
        if (item.itemName == "Key")
        {
            PV.gameObject.GetComponent<ActionController>().isKeyUse = false;
            ClearSlot();
        }
    }


    private void SetColor(float _alpha) // 이미지의 알파 값 설정 
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    // 아이템 추가
    public void AddItem(Item _item, int _count = 1)
    {
        item = _item;                                      // 슬롯에 아이템 데이터베이스 저장
        itemCount = _count;                                // 아이템 정보의 갯수 ( 여러 아이템을 드랍하는 아이템 묶음이라면 수량을 변경)
        itemImage.sprite = item.itemImage;                 // item DB에 저장되어 있는 이미지를 받아옴

        if (item.itemType != Item.ItemType.Equipment)      // 장비 아이템이 아닌 경우
        {
            go_CountImage.SetActive(true);                 // UI오른쪽 하단에 있는 아이템 갯수를 알려주는 오브젝트 활성화(동그라미 이미지랑, 자식으로 텍스트 보유 된거)
            text_Count.text = itemCount.ToString();        // 아이템 갯수 입력
        }
        else
        {
            text_Count.text = "0";                         // 장비아이템은 해당 오브젝트 안씀 (단일 보유 아이템이라서)
            go_CountImage.SetActive(false);
        }

        SetColor(1);                                       // 1일 때 이미지 보임, 0일 때 안보임
    }

    // 아이템 개수 조정
    public void SetSlotCount(int _count)                       // _count에 갯수 지정해서 아이템 감소나 아이템 증가 시켜주는 기능
    {
        itemCount += _count;
        text_Count.text = itemCount.ToString();

        if (itemCount <= 0)                                   // 아이템 갯수가 0보다 작거나 같아질때 Slot에 있는 아이템 정보 정리 메서드 호출
            ClearSlot();
    }

    // 슬롯 초기화
    private void ClearSlot()                                // 슬롯 정보 초기화
    {
        item = null;
        itemCount = 0;
        itemImage.sprite = null;
        SetColor(0);

        text_Count.text = "0";
        go_CountImage.SetActive(false);

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) // 이 객체의 오브젝트에 우클릭을 한다면
        {
            if (item != null)
            {
                if (item.itemType == Item.ItemType.Equipment)     // 인벤토리 아이템 클릭 시 장비하는 기능인대 해당 기능 사용안함
                {
                    //StartCoroutine(theWeaponManager.ChangeWeaponCoroutine(item.weaponType, item.itemName));
                }
                else
                {
                    itemEffectDatabase.UseItem(item);                // 데이터베이스에 있는 아이템 효과 사용
                    Debug.Log(item.itemName + "을 사용했습니다.");
                    SetSlotCount(-1);                              // 소모성 아이템 클릭 시 갯수만 줄어듬, 전체적으로 사용안하기 때문에 테스트 후 없애도 무방 할 듯
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)             // 마우스 드래그 시작 이벤트
    {
        if (item != null)
        {
            DragSlot.instance.dragSlot = this;                        // DragSlot에 싱글톤으로 생성된 dragSlot에 이 slot 데이터를 대입함
            DragSlot.instance.DragSetImage(itemImage);                // 이미지 세팅

            DragSlot.instance.transform.position = eventData.position;// 드래그한 위치로 이동
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            DragSlot.instance.transform.position = eventData.position; // 드래그 이동할 때 위치값 받아옴 
        }
    }

    public void OnEndDrag(PointerEventData eventData)                  // 드래그를 UI가 없는 곳으로 놓았을 때 이벤트
    {
        Debug.Log("OnEndDrag");
        DropItem();
        DragSlot.instance.SetColor(0);
        DragSlot.instance.dragSlot = null;
    }

    public void OnDrop(PointerEventData eventData)                    // 다른 UI Slot에 정확히 넣었을 때 이벤트
    {
        Debug.Log("OnDrop");
        if (DragSlot.instance.dragSlot != null)   // Drag Obj의 null오류 방지
        {
            ChangeSlot();
        }
    }

    private void ChangeSlot()
    {
        // 복사 한다
        // 해당 위치로 옮긴다
        // 복사한 것을 바꾼 위치로 옮긴다.

        Item _tempItem = item;
        int _tempItemCount = itemCount;

        AddItem(DragSlot.instance.dragSlot.item, DragSlot.instance.dragSlot.itemCount);

        if (_tempItem != null) // 빈 자리가 아니라면
        {
            DragSlot.instance.dragSlot.AddItem(_tempItem, _tempItemCount);
        }
        else // 빈 자리라면 빈칸 넣기
        {
            DragSlot.instance.dragSlot.ClearSlot();
        }
    }

    private void DropItem()
    {
        // 아이템이 인벤토리에 있을 경우에만 사용한다
        // Drag Slot에 있는 아이템의 정보 해당 아이템에 있는 아이템 prefab을 플레이어의 정면에 생성시킨다.
        if (item != null)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                DragSlot.instance.SetPrefab(DragSlot.instance.dragSlot.item.itemName);
            }
            else
            {
                PV.RPC("SetPrefabRPC", RpcTarget.MasterClient, item.itemName);
            }

            SetSlotCount(-1);
            PV.RPC("PlayerWeaponRPC", RpcTarget.All, "HAND", "Hand");
        }
    }
}
