using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView PV;

    public static bool inventoryActivated = false; // �κ��丮 UI �Ұ�

    // �ʿ� ������Ʈ
    [SerializeField] private GameObject go_Inventory_Base; // �κ��丮 Ȱ��ȭ ������
    
    [SerializeField] private GameObject go_SlotsParent;    // �κ��丮 Slot�� �θ� ���ӿ�����Ʈ ����

    [SerializeField] public Slot[] slots;                 // �κ��丮�� Slot�� �迭�� ����

    [SerializeField] PlayerController playerController;    // ���� �׼� �� �κ��丮 ��� ���� ���� ���

    [SerializeField] WeaponManager theWeaponManager;           // ���� ���� �� ����
    [SerializeField] ItemEffectDatabase itemEffectDatabase;    // ������ ��� ȿ�� ����

    public bool isInventoryFull;                         // �κ��丮�� ������
    public bool IsInventoryFull => isInventoryFull;       // �ܺο� �Ұ� ��ȯ
    public bool hasSameItem;                             // ���� �̸��� ������ �������� ������ �ִ°�?
    public bool HasSameItem => hasSameItem;               // �ܺο� �Ұ� ��ȯ

    private void Start()
    {
        slots = go_SlotsParent.GetComponentsInChildren<Slot>(); // �θ� ������Ʈ���� �ڽ� ������Ʈ�� ȣ��
    }

    void Update()
    {
      
        if (!PV.IsMine) return;
        CheckSlotIsFull();
        TryOpenInventory();                                       // IŰ�� ������ �������� �巡���ؼ� ���� �� ����

        if (Input.GetKeyDown(KeyCode.Alpha1))                     // Ű�е� 1~5�� ������ ������ ����ϱ�
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

    public void UseItem(int _num)                  // �ܺο��� �� �޼��带 �ҷ��ͼ� ������ ���
    {

        if (slots[_num].item != null)
        {
            itemEffectDatabase.UseItem(slots[_num].item);           // ���� ������ ����Ÿ ���̽����� ���� �����

            if (slots[_num].item.itemType == Item.ItemType.Used)    // �Ҹ� ������ ����� ���� �۵��ϰ� �ٲ�
                slots[_num].SetSlotCount(-1);

            //if (slots[_num].item.itemType == Item.ItemType.Equipment)
            //{
            //    StartCoroutine(theWeaponManager.ChangeWeaponCoroutine(slots[_num].item.weaponType, slots[_num].item.itemName));
            //}
            //else
            //{
            //    Debug.Log(slots[_num].item.itemName + "�� ����߽��ϴ�.");
            //    slots[_num].SetSlotCount(-1);                                // ������ ��� ������ �����ؾ���
            //}
        }
    }

    private void TryOpenInventory()                             // �κ��丮 ȣ�� �Է� Ű ����
    {
        if (Input.GetKeyDown(KeyCode.I) && playerController.HasControl) // ���� �׼� �߿��� �κ��丮 ��� ���� ����
        {
            inventoryActivated = !inventoryActivated;                   // Ű�� ���� �� ���� ���� ��ȯ ( �ݱ� / ���� )

            if (inventoryActivated)
                OpenInventory();
            else
                CloseInventory();
        }                                                                                                                                                                                                                                                       
    }

    // ���� ������Ʈ Ȱ��ȭ ��Ȱ��ȭ
    #region TryOpen & Close
    private void OpenInventory()  // Close ��� ������� ���ϹǷ� ���� �����ص� ����.
    {
        go_Inventory_Base.SetActive(true);
    }

    private void CloseInventory() // �׻� �κ��丮 ���̰� �Ϸ��� �ش� ��� �̻��
    {
        //go_Inventory_Base.SetActive(false);
    }
    #endregion   
    [PunRPC]
    public void AcquireItem(Item _item, int _count = 1)           // ������ ȹ�� �޼���
    {
        if (!PV.IsMine) return;
        // ������� �ƴ� ���
        if (Item.ItemType.Equipment != _item.itemType)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item != null)                        // �������� ���� ��쿡��
                {
                    if (slots[i].item.itemName == _item.itemName) // �̹� ���Կ� �������� �ִٸ�
                    {
                        hasSameItem = true;
                        slots[i].SetSlotCount(_count);            // �ش� ���Կ� �������� �߰��Ѵ�.
                        return;
                    }
                    else
                    {
                        hasSameItem = false;
                    }
                }
            }
        }

        for (int i = 0; i < slots.Length; i++)                             // �������� ���� ���̽�����
        {
            if (CheckSlotIsFull()) return;                                 // �κ��丮�� ���� á�� �� ���� üũ�Ѵ�.
            if (slots[i].item == null)                                // �̹� ���Կ� �������� ���ٸ�
            {
                slots[i].AddItem(_item, _count);                      // count ����ŭ �ش� ������ ȹ��
                return;
                
            }

        }
    }

 

    private bool CheckSlotIsFull()                    // �������� ���� á���� Ȯ���ϴ� �޼���
    {
        for (int i = 0; i < slots.Length; i++)        // �ϳ��� ������ ������ ������� �ʴ� ĭ�� �ִٸ� false ��ȯ
        {
            if (slots[i].item == null)
            {
                isInventoryFull = false;              // isInventoryFull �Ұ��� ActionController���� �����ͼ� CanPickUp�޼����� �������� ���
                return false;
            }
        }

        // ������ ���� á�� ���� ����
        CheckHasSameItem();

        isInventoryFull = true;
        return true;
    }
    private bool CheckHasSameItem()
    {

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item.itemType == Item.ItemType.Used) // �̹� ���Կ� �������� �ִٸ�
            {
                hasSameItem = true;
                return true;
            }
        }

        hasSameItem = false;
        return false;
    }
}
