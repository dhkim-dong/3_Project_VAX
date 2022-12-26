using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView PV;

    public static bool inventoryActivated = false; // 인벤토리 UI 불값

    // 필요 컴포넌트
    [SerializeField] private GameObject go_Inventory_Base; // 인벤토리 활성화 오브제
    
    [SerializeField] private GameObject go_SlotsParent;    // 인벤토리 Slot의 부모 게임오브젝트 지정

    [SerializeField] public Slot[] slots;                 // 인벤토리의 Slot을 배열로 관리

    [SerializeField] PlayerController playerController;    // 파쿠르 액션 중 인벤토리 사용 막기 위해 사용

    [SerializeField] WeaponManager theWeaponManager;           // 무기 구현 후 적용
    [SerializeField] ItemEffectDatabase itemEffectDatabase;    // 아이템 사용 효과 구현

    public bool isInventoryFull;                         // 인벤토리가 가득참
    public bool IsInventoryFull => isInventoryFull;       // 외부에 불값 반환
    public bool hasSameItem;                             // 같은 이름의 스택형 아이템을 가지고 있는가?
    public bool HasSameItem => hasSameItem;               // 외부에 불값 반환

    private void Start()
    {
        slots = go_SlotsParent.GetComponentsInChildren<Slot>(); // 부모 오브젝트들의 자식 오브젝트를 호출
    }

    void Update()
    {
      
        if (!PV.IsMine) return;
        CheckSlotIsFull();
        TryOpenInventory();                                       // I키를 눌러야 아이템을 드래그해서 버릴 수 있음

        if (Input.GetKeyDown(KeyCode.Alpha1))                     // 키패드 1~5번 눌러서 아이템 사용하기
        {
            UseItem(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseItem(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            UseItem(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            UseItem(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            UseItem(4);
        }
    }

    public void UseItem(int _num)                  // 외부에서 이 메서드를 불러와서 아이템 사용
    {

        if (slots[_num].item != null)
        {
            itemEffectDatabase.UseItem(slots[_num].item);           // 이제 아이템 데이타 베이스에서 무기 변경과

            if (slots[_num].item.itemType == Item.ItemType.Used)    // 소모성 아이템 사용을 같이 작동하게 바꿈
                slots[_num].SetSlotCount(-1);

            //if (slots[_num].item.itemType == Item.ItemType.Equipment)
            //{
            //    StartCoroutine(theWeaponManager.ChangeWeaponCoroutine(slots[_num].item.weaponType, slots[_num].item.itemName));
            //}
            //else
            //{
            //    Debug.Log(slots[_num].item.itemName + "을 사용했습니다.");
            //    slots[_num].SetSlotCount(-1);                                // 아이템 사용 로직을 구현해야함
            //}
        }
    }

    private void TryOpenInventory()                             // 인벤토리 호출 입력 키 지정
    {
        if (Input.GetKeyDown(KeyCode.I) && playerController.HasControl) // 파쿠르 액션 중에는 인벤토리 기능 막아 놓음
        {
            inventoryActivated = !inventoryActivated;                   // 키를 누를 때 마다 상태 변환 ( 닫기 / 열기 )

            if (inventoryActivated)
                OpenInventory();
            else
                CloseInventory();
        }                                                                                                                                                                                                                                                       
    }

    // 게임 오브젝트 활성화 비활성화
    #region TryOpen & Close
    private void OpenInventory()  // Close 기능 사용하지 안하므로 역시 사용안해도 무방.
    {
        go_Inventory_Base.SetActive(true);
    }

    private void CloseInventory() // 항상 인벤토리 보이게 하려고 해당 기능 미사용
    {
        //go_Inventory_Base.SetActive(false);
    }
    #endregion   
    [PunRPC]
    public void AcquireItem(Item _item, int _count = 1)           // 아이템 획득 메서드
    {
        if (!PV.IsMine) return;
        // 장비템이 아닌 경우
        if (Item.ItemType.Equipment != _item.itemType)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item != null)                        // 아이템이 있을 경우에는
                {
                    if (slots[i].item.itemName == _item.itemName) // 이미 슬롯에 아이템이 있다면
                    {
                        hasSameItem = true;
                        slots[i].SetSlotCount(_count);            // 해당 슬롯에 아이템을 추가한다.
                        return;
                    }
                    else
                    {
                        hasSameItem = false;
                    }
                }
            }
        }

        for (int i = 0; i < slots.Length; i++)                             // 아이템이 없는 케이스에는
        {
            if (CheckSlotIsFull()) return;                                 // 인벤토리가 가득 찼는 지 먼저 체크한다.
            if (slots[i].item == null)                                // 이미 슬롯에 아이템이 없다면
            {
                slots[i].AddItem(_item, _count);                      // count 수만큼 해당 아이템 획득
                return;
                
            }

        }
    }

 

    private bool CheckSlotIsFull()                    // 아이템이 가득 찼는지 확인하는 메서드
    {
        for (int i = 0; i < slots.Length; i++)        // 하나라도 아이템 슬롯을 사용하지 않는 칸이 있다면 false 반환
        {
            if (slots[i].item == null)
            {
                isInventoryFull = false;              // isInventoryFull 불값을 ActionController에서 가져와서 CanPickUp메서드의 조건으로 사용
                return false;
            }
        }

        // 아이템 가득 찼을 때만 실행
        CheckHasSameItem();

        isInventoryFull = true;
        return true;
    }
    private bool CheckHasSameItem()
    {

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item.itemType == Item.ItemType.Used) // 이미 슬롯에 아이템이 있다면
            {
                hasSameItem = true;
                return true;
            }
        }

        hasSameItem = false;
        return false;
    }
}
