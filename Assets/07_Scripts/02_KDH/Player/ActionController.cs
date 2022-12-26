using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// puncallback�� �Ϲ� pun���� ������ ������ȯ?..��°�� ��ȣ�ۿ�� ���ÿ� �ٷ� �̷�������Ҷ� ����ϴ°ɷ� ������ϴ�..
public class ActionController : MonoBehaviourPunCallbacks 
{
    [SerializeField] private PhotonView PV;             	// ����� ����Ϸ��� ���� 

    // �赿�� ��� ������Ʈ
    [SerializeField] private float maxRange;            	// ���� ������ �ִ� �Ÿ�
    [SerializeField] private float minRange;            	// ���� �ּ� �Ÿ�(�÷��̾� ĳ���Ϳ��� �Ÿ�)
    [SerializeField] private LayerMask itemLayer;       	// ������ ���̾� ����ũ
    [SerializeField] private Text actionText;           	// ��� �ؽ�Ʈ
    [SerializeField] private Camera cam;                	// ī�޶�
    [SerializeField] private Inventory theInventory;    	// �κ��丮 ��ũ��Ʈ 
    [SerializeField] private KeyController keyController;  	// ���� ���� �ݱ� ���� �ʿ���

    private bool pickupActivated = false;               	// ���� ���� ���� 
    private bool doorActivated = false;                 	// �� ���� ����
    public bool isKeyUse = false;      						// ���� ��� ���� üũ
    public RaycastHit hitInfo;                          	// �浹ü ���� ����

    // �輮�� ��� ������Ʈ
    public GameObject hitBox;                           	// �浹�� �ڽ� ����
    [SerializeField] private GameObject hitItem;        	// �浹�� ������ ����
    public ItemBox hitboxItem;                          	// �ڽ����� hitBox �����ҷ��� ������Դϴ� ItemCalCount() �ҷ��ö����
	
	// ray�� �޾ƿ� �������� �̸� CanPickUp���� ���� ���� �Ҹ����̶� rat�� �ν��ϴ� ���̶� �̸� ���Ҷ� ���
    private GameObject getItemName;                     	
	
	//�������� �����ɶ� �ڱ� �ڽ��� �������̵� �����ϴµ� ����մϴ�. ���߿� PRC �Ű������� ���
    public int PV_viewID;                               	

    /// <summary>
    /// �� �����Ӹ��� ����
    /// </summary>
    void Update()
    {
        //ȣ���ϴ� ����� �ڱ��ڽ��� �ƴѰ��� �۵����ϰ� 
        if (!PV.IsMine) return;

        //������ ���� Ȯ��
        CheckItem();

        //���� ���ų� �������� ���� �ϴ� �޼ҵ���� ����ֽ��ϴ�
        TryAction();                
    }

    /// <summary>
    /// 'E' ��ư�� ������ �� �׼� �̺�Ʈ ���� �޼���
    /// </summary>
    private void TryAction()
    {
        //ȣ���ڰ� �ڱ��ڽ��� ��쿡�� ��� 
        if (PV.IsMine)                            
        {
            // ��ǲ �� ����
            if (Input.GetKeyDown(KeyCode.E))      
            {
                // EŰ�� ������ �ʾƵ� �ֱ������� ������ Info Ȯ�� (update���� Ȯ���ϰ� e�ε� Ȯ��)
                CheckItem();

                // ������ �Լ��ϴ� ���
                CanPickUp();

                // ������(Door) ���� ���� ����
                CanOpenDoor();                    
            }
        }
    }

    /// <summary>
    /// ������ ���� 
    /// </summary>
    private void CanPickUp()                      
    {
        // CheckItem���� ItemInfoAppear=true�� �����ϸ� ���� �� ���ִ� �����Դϴ�.
        if (pickupActivated)                      
        {
            // ���̰� �ν��� ������Ʈ�� ������ ��� ����  
            if (hitInfo.transform.GetComponent<ItemBox>())  
            {
                Debug.Log("������1");

                //�ν��� ������ �ڽ� ������ ��ƵӴϴ� (�ڽ����� ���������� ��ũ��Ʈ ������ ���� ���� ����)
                hitBox = hitInfo.transform.GetComponent<ItemBox>().gameObject;
                hitBox.GetComponent<ItemBox>().PV.RPC("OpenBox", RpcTarget.All);
            }
            //���̰� �ν��� ������Ʈ�� ���ڰ� �ƴҰ�� (������)         
            else
            {
                Debug.Log("�Ⱦ�1");

                // �κ��丮�� �� ���� ���� ���¿��� �� �ൿ
                checkInven_NotFull();

                //�κ��丮�� �� á�� ���
                if (theInventory.IsInventoryFull)   
                {
                    Debug.Log("�Ⱦ�2");

                    //�κ��丮�� �Ҹ�ǰ ������ ���      
                    if (theInventory.HasSameItem)                                  
                    {
                        Debug.Log("�Ⱦ�3");

                        //�κ��丮 ������ üũ�մϴ� 
                        slotCheck_used();           
                    }
                    //�κ��丮�� ��á�� &&�Ҹ�ǰ�� ���� ��� 
                    else
                    {
                        Debug.Log("�Ⱦ�4");

                        //�ƹ��͵� �����ʰ� �ݺ��� ����
                        return;                                                   
                    }
                }
            }
            InfoDisAppear();
        }
    }

    /// <summary>
    /// ������ ���� Ȯ��
    /// </summary>
    private void CheckItem()
    {
        // Editorâ���� Ȯ��
        Debug.DrawRay(cam.transform.position + cam.transform.forward * minRange, cam.transform.forward * maxRange, Color.blue);

        // �ν����� â���� Layer ����(item����) ������ �߰� �ؾ��� 
        if (Physics.Raycast(cam.transform.position + cam.transform.forward * minRange, cam.transform.forward, out hitInfo, maxRange, itemLayer)) 
        {
            if (hitInfo.transform.tag == "Item")
            {
                // ���� �ν��ϰ� UI ������ִ� ����
                ItemInfoAppear();
            }
            else if (hitInfo.transform.tag == "door")
            {
                if (!hitInfo.transform.GetComponent<BigDoorOpen>())
                    DoorInfoAppear();

                else if (hitInfo.transform.GetComponent<BigDoorOpen>().IsNeedKey)
                {
                    DoorLockInfoAppear();
                }
                else
                    DoorInfoAppear();
            }
        }
        else
        {
            InfoDisAppear();
            DoorInfoDisAppear();
        }
    }
    #region ������ ���� �޼ҵ�

    /// <summary>
    /// //�κ��丮�� �� ��á���� 
    /// </summary>
    void checkInven_NotFull()        
    {
        Debug.Log("����1");

        //���̰� ��� ����Ű�������� �� �����ߴ°� �������� �����߰� 
        if (hitInfo.transform.GetComponent<ItemPickUp>() == null) return; 

        //CanPickUp���� �����۰� �κ��丮 ������ �̸��� �� (�κ��丮�� ���� �̸��� �Ҹ�ǰ�� �ִ��� ������ �˻�) �� ������Ʈ ����
        //slotCheck_used() ���� ����ҰŶ� ���⼭ ���� ���ص��Ǵµ� ���߿� Ư�������� (������ 1���� ������ ���� �� �ִٴ���) ���� ������ ���ü��� �������ؼ� �׳� �����Ҷ� ������ѳ��� �ֽ��ϴ�
        getItemName = hitInfo.transform.GetComponent<ItemPickUp>().gameObject;

        //�κ��丮�� ���� �������� ���¶��
        if (!theInventory.IsInventoryFull)            
        {
            Debug.Log("����1");

            //������ �Ա� ����
            getItem();                              
        }
    }

    /// <summary>
    /// ������ �Ա�  
    /// ���ڿ��� ���� �������� ?�� ī��Ʈ�� ���� �θ� ã���ִ� ������ �ؼ� �������� �ϸ� �ȵ� ���߿� �θ� ��ü�� ���ֱ⿡ ������� 
    /// </summary>
    void getItem()                                  
    {
        Debug.Log("�����۸���1");

        //�������� ������ ������ ������ �߰����ݴϴ�
        theInventory.AcquireItem(hitInfo.transform.GetComponent<ItemPickUp>().item, 1);

        //������ ���� Ŭ���̾�Ʈ�� �ٸ� ������Ʈ�� ������ ������ ���� ������ �����ϱ� ���� ����
        int itemPVID = hitInfo.transform.GetComponent<PhotonView>().ViewID;

        //������ Ŭ���̾�Ʈ�� ���
        if (PhotonNetwork.IsMasterClient)    
        {
            Debug.Log("�����۸���1");

            //���� ������ ����� ���̵� �˻��ؼ� ������Ʈ�� �����մϴ� 
            GameObject PV_item = PhotonView.Find(itemPVID).gameObject;

            //������ ������Ʈ�� ������ �ڽ� ������ ���� �ƴ϶�� ���� (����������� �ڽ������� �� �Դϴ� , �ڽ��� ��� ī��Ʈ�� �����ϸ� �ο����� ���缭 ���� �߰� )
            if (PV_item.GetComponent<ItemPickUp>().itembox != null)
            {
                //����� �ڽ� �޼ҵ带 ���� ��ŵ�ϴ� (�ڽ� �������� �Ծ����� , �ٸ����� �ڽ� �ı�)
                PV_item.GetComponent<ItemPickUp>().itembox.ItemCalCount();
            }

            //���� ������Ʈ �ı� 
            PhotonNetwork.Destroy(PV_item);
        }
        //�����Ͱ� �ƴ� Ŭ���̾�Ʈ�� ���
        else
        {
            Debug.Log("Ŭ�� �����۸���1");
            PV.RPC("RPC_itemboxCount", RpcTarget.MasterClient, itemPVID);
            PV.RPC("RPC_Destoryitem", RpcTarget.MasterClient, itemPVID);
        }
    }

    /// <summary>
    /// ������ �ڽ� ī��Ʈ ���̱� 
    /// </summary>
    /// <param name="itemPV"></param>
    [PunRPC]
    void RPC_itemboxCount(int itemPV)
    {
        // ������ �ڽ� ī��Ʈ ���̱� 
        if (PhotonView.Find(itemPV).gameObject.GetComponent<ItemPickUp>().itembox != null)
            PhotonView.Find(itemPV).gameObject.GetComponent<ItemPickUp>().itembox.ItemCalCount();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="viewID"></param>
    [PunRPC]
    void RPC_Destoryitem(int viewID)
    {
        PhotonView itemView = PhotonView.Find(viewID);
        if (!itemView.IsMine) return;
        if (itemView == null) return;
        PhotonNetwork.Destroy(itemView.gameObject);
    }

    /// <summary>
    /// ����� ������ ?��
    /// </summary>
    void dorpItem()  
    {
        //�÷��̾ ����� �������� ���� üũ�� ����̾����Ƿ� �׳� ���� ��Ŵ 
        theInventory.AcquireItem(hitInfo.transform.GetComponent<ItemPickUp>().item, 1);

        // ��� ������ ����
        Destroy(hitInfo.transform.GetComponent<ItemPickUp>().gameObject);
    }

    /// <summary>
    /// �κ��丮 ���� üũ
    /// </summary>
    void slotCheck_used()  
    {
        for (int i = 0; i < theInventory.slots.Length; i++)
        {
            //�κ��丮 ���Կ� �������� ���� ��쿡�� �˻��ϰ� ���̿� ���� �������� �̸��� ���Կ� �̸��� ���� ��츸 ����
            //�κ��丮�� ���� �־�ѱ� �ߴµ� ������Ʈ���� ��� �����°� �ڿ����񰰾Ƽ� �׳� �׼ǿ� �����Ҷ� �����ϵ��� �߽��ϴ� 
            if (theInventory.slots[i].item != null)                  
            {
                if (theInventory.slots[i].item.itemName == getItemName.GetComponent<ItemPickUp>().item.itemName)
                {
                    //���ڿ��� �Դ°��
                    if (hitInfo.transform.GetComponent<ItemPickUp>().itembox != null)         
                    {
                        Debug.Log("���� ���� �� ������");

                        //������ �Ա� ���� 
                        getItem();        
                    }
                    //��� �������� ��� 
                    else
                    {
                        Debug.Log("���� ���� ��� ������");

                        //��������� �Ա� ����  
                        dorpItem();      
                    }
                }
            }
        }
    }

    #endregion ������ ?�� �޼���

    /// <summary>
    /// ���� ���� �޼���
    /// </summary>
    private void CanOpenDoor()  
    {
        if (doorActivated)
        {
            if (hitInfo.transform != null)
            {
                if (hitInfo.transform.GetComponent<BigDoorOpen>() == null) return;

				PhotonView doorPV = hitInfo.transform.GetComponent<BigDoorOpen>().PV;

                doorPV.RPC("TryHandle", RpcTarget.All);

                if (hitInfo.transform.GetComponent<BigDoorOpen>().IsNeedKey && keyController.isActivate)
                {
                    doorPV.RPC("CheckHasKey", RpcTarget.All);
                    isKeyUse = true;
                    PV.RPC("PlayerWeaponRPC", RpcTarget.All, "HAND", "Hand");
                }
            }
        }
    }

    // ������ UI ��� ���� �޼���
    #region UserInfo
    private void DoorInfoDisAppear()
    {
        doorActivated = false;
        actionText.gameObject.SetActive(false);
    }

    private void DoorInfoAppear()
    {
        doorActivated = true;
        actionText.gameObject.SetActive(true);
        actionText.text = hitInfo.transform.name + " ��ȣ�ۿ� " + "<color=red>" + "(E)Ű" + "</color>";
    }

    private void DoorLockInfoAppear()
    {
        doorActivated = true;
        actionText.gameObject.SetActive(true);
        actionText.text = hitInfo.transform.name + " �� �����ϴ�. " + "<color=red>" + "ī�� Ű �ʿ�" + "</color>";
    }

    private void InfoDisAppear()
    {
        pickupActivated = false;
        actionText.gameObject.SetActive(false);
    }

    private void ItemInfoAppear()
    {
        pickupActivated = true;
        actionText.gameObject.SetActive(true);
        actionText.text = hitInfo.transform.GetComponent<ItemPickUp>().item.itemName + " ȹ�� " + "<color=yellow>" + "(E)Ű" + "</color>";
    }
    #endregion
}
