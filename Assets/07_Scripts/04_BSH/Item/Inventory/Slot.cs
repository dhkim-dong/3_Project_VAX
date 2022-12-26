using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;

public class Slot : MonoBehaviourPunCallbacks, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Item item; // ȹ���� ������
    public int itemCount; // ȹ���� �������� ����
    public Image itemImage; // �������� �̹���


    // �ʿ� ������Ʈ
    [SerializeField] private Text text_Count; // �������� ��
    [SerializeField] private GameObject go_CountImage; // ������Ʈ �� ����
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


    private void SetColor(float _alpha) // �̹����� ���� �� ���� 
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }

    // ������ �߰�
    public void AddItem(Item _item, int _count = 1)
    {
        item = _item;                                      // ���Կ� ������ �����ͺ��̽� ����
        itemCount = _count;                                // ������ ������ ���� ( ���� �������� ����ϴ� ������ �����̶�� ������ ����)
        itemImage.sprite = item.itemImage;                 // item DB�� ����Ǿ� �ִ� �̹����� �޾ƿ�

        if (item.itemType != Item.ItemType.Equipment)      // ��� �������� �ƴ� ���
        {
            go_CountImage.SetActive(true);                 // UI������ �ϴܿ� �ִ� ������ ������ �˷��ִ� ������Ʈ Ȱ��ȭ(���׶�� �̹�����, �ڽ����� �ؽ�Ʈ ���� �Ȱ�)
            text_Count.text = itemCount.ToString();        // ������ ���� �Է�
        }
        else
        {
            text_Count.text = "0";                         // ���������� �ش� ������Ʈ �Ⱦ� (���� ���� �������̶�)
            go_CountImage.SetActive(false);
        }

        SetColor(1);                                       // 1�� �� �̹��� ����, 0�� �� �Ⱥ���
    }

    // ������ ���� ����
    public void SetSlotCount(int _count)                       // _count�� ���� �����ؼ� ������ ���ҳ� ������ ���� �����ִ� ���
    {
        itemCount += _count;
        text_Count.text = itemCount.ToString();

        if (itemCount <= 0)                                   // ������ ������ 0���� �۰ų� �������� Slot�� �ִ� ������ ���� ���� �޼��� ȣ��
            ClearSlot();
    }

    // ���� �ʱ�ȭ
    private void ClearSlot()                                // ���� ���� �ʱ�ȭ
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
        if (eventData.button == PointerEventData.InputButton.Left) // �� ��ü�� ������Ʈ�� ��Ŭ���� �Ѵٸ�
        {
            if (item != null)
            {
                if (item.itemType == Item.ItemType.Equipment)     // �κ��丮 ������ Ŭ�� �� ����ϴ� ����δ� �ش� ��� ������
                {
                    //StartCoroutine(theWeaponManager.ChangeWeaponCoroutine(item.weaponType, item.itemName));
                }
                else
                {
                    itemEffectDatabase.UseItem(item);                // �����ͺ��̽��� �ִ� ������ ȿ�� ���
                    Debug.Log(item.itemName + "�� ����߽��ϴ�.");
                    SetSlotCount(-1);                              // �Ҹ� ������ Ŭ�� �� ������ �پ��, ��ü������ �����ϱ� ������ �׽�Ʈ �� ���ֵ� ���� �� ��
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)             // ���콺 �巡�� ���� �̺�Ʈ
    {
        if (item != null)
        {
            DragSlot.instance.dragSlot = this;                        // DragSlot�� �̱������� ������ dragSlot�� �� slot �����͸� ������
            DragSlot.instance.DragSetImage(itemImage);                // �̹��� ����

            DragSlot.instance.transform.position = eventData.position;// �巡���� ��ġ�� �̵�
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            DragSlot.instance.transform.position = eventData.position; // �巡�� �̵��� �� ��ġ�� �޾ƿ� 
        }
    }

    public void OnEndDrag(PointerEventData eventData)                  // �巡�׸� UI�� ���� ������ ������ �� �̺�Ʈ
    {
        Debug.Log("OnEndDrag");
        DropItem();
        DragSlot.instance.SetColor(0);
        DragSlot.instance.dragSlot = null;
    }

    public void OnDrop(PointerEventData eventData)                    // �ٸ� UI Slot�� ��Ȯ�� �־��� �� �̺�Ʈ
    {
        Debug.Log("OnDrop");
        if (DragSlot.instance.dragSlot != null)   // Drag Obj�� null���� ����
        {
            ChangeSlot();
        }
    }

    private void ChangeSlot()
    {
        // ���� �Ѵ�
        // �ش� ��ġ�� �ű��
        // ������ ���� �ٲ� ��ġ�� �ű��.

        Item _tempItem = item;
        int _tempItemCount = itemCount;

        AddItem(DragSlot.instance.dragSlot.item, DragSlot.instance.dragSlot.itemCount);

        if (_tempItem != null) // �� �ڸ��� �ƴ϶��
        {
            DragSlot.instance.dragSlot.AddItem(_tempItem, _tempItemCount);
        }
        else // �� �ڸ���� ��ĭ �ֱ�
        {
            DragSlot.instance.dragSlot.ClearSlot();
        }
    }

    private void DropItem()
    {
        // �������� �κ��丮�� ���� ��쿡�� ����Ѵ�
        // Drag Slot�� �ִ� �������� ���� �ش� �����ۿ� �ִ� ������ prefab�� �÷��̾��� ���鿡 ������Ų��.
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
