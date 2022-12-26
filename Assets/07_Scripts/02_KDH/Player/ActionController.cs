using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// puncallback이 일반 pun보다 더빠른 정보교환?..어째뜬 상호작용과 동시에 바로 이루어져야할때 사용하는걸로 배웠습니다..
public class ActionController : MonoBehaviourPunCallbacks 
{
    [SerializeField] private PhotonView PV;             	// 포톤뷰 사용하려고 선언 

    // 김동훈 사용 컴포넌트
    [SerializeField] private float maxRange;            	// 습득 가능한 최대 거리
    [SerializeField] private float minRange;            	// 습득 최소 거리(플레이어 캐릭터와의 거리)
    [SerializeField] private LayerMask itemLayer;       	// 반응할 레이어 마스크
    [SerializeField] private Text actionText;           	// 출력 텍스트
    [SerializeField] private Camera cam;                	// 카메라
    [SerializeField] private Inventory theInventory;    	// 인벤토리 스크립트 
    [SerializeField] private KeyController keyController;  	// 문을 열고 닫기 위해 필요함

    private bool pickupActivated = false;               	// 습득 가능 여부 
    private bool doorActivated = false;                 	// 문 열기 여부
    public bool isKeyUse = false;      						// 열쇠 사용 여부 체크
    public RaycastHit hitInfo;                          	// 충돌체 정보 저장

    // 배석현 사용 컴포넌트
    public GameObject hitBox;                           	// 충돌한 박스 저장
    [SerializeField] private GameObject hitItem;        	// 충돌한 아이템 저장
    public ItemBox hitboxItem;                          	// 박스별로 hitBox 저장할려고 사용중입니다 ItemCalCount() 불러올때사용
	
	// ray가 받아온 아이템의 이름 CanPickUp에서 내가 가진 소모템이랑 rat가 인식하는 템이랑 이름 비교할때 사용
    private GameObject getItemName;                     	
	
	//아이템이 생성될때 자기 자신의 포톤뷰아이디를 저장하는데 사용합니다. 나중에 PRC 매개변수로 사용
    public int PV_viewID;                               	

    /// <summary>
    /// 매 프레임마다 실행
    /// </summary>
    void Update()
    {
        //호출하는 사람이 자기자신이 아닌경우는 작동못하게 
        if (!PV.IsMine) return;

        //아이템 정보 확인
        CheckItem();

        //문을 열거나 아이템을 습득 하는 메소드들을 담고있습니다
        TryAction();                
    }

    /// <summary>
    /// 'E' 버튼을 눌렀을 때 액션 이벤트 실행 메서드
    /// </summary>
    private void TryAction()
    {
        //호출자가 자기자신일 경우에만 사용 
        if (PV.IsMine)                            
        {
            // 인풋 값 설정
            if (Input.GetKeyDown(KeyCode.E))      
            {
                // E키를 누르지 않아도 주기적으로 아이템 Info 확인 (update에서 확인하고 e로도 확인)
                CheckItem();

                // 아이템 입수하는 방식
                CanPickUp();

                // 아이템(Door) 열기 위한 로직
                CanOpenDoor();                    
            }
        }
    }

    /// <summary>
    /// 아이템 습득 
    /// </summary>
    private void CanPickUp()                      
    {
        // CheckItem에서 ItemInfoAppear=true로 변경하면 습득 할 수있는 상태입니다.
        if (pickupActivated)                      
        {
            // 레이가 인식한 오브젝트가 상자일 경우 실행  
            if (hitInfo.transform.GetComponent<ItemBox>())  
            {
                Debug.Log("문열어1");

                //인식한 아이템 박스 정도를 담아둡니다 (박스마다 개별적으로 스크립트 실행을 위해 따로 저장)
                hitBox = hitInfo.transform.GetComponent<ItemBox>().gameObject;
                hitBox.GetComponent<ItemBox>().PV.RPC("OpenBox", RpcTarget.All);
            }
            //레이가 인식한 오브젝트가 상자가 아닐경우 (아이템)         
            else
            {
                Debug.Log("픽업1");

                // 인벤토리가 꽉 차지 않은 상태에서 할 행동
                checkInven_NotFull();

                //인벤토리가 꽉 찼을 경우
                if (theInventory.IsInventoryFull)   
                {
                    Debug.Log("픽업2");

                    //인벤토리에 소모품 이있을 경우      
                    if (theInventory.HasSameItem)                                  
                    {
                        Debug.Log("픽업3");

                        //인벤토리 슬롯을 체크합니다 
                        slotCheck_used();           
                    }
                    //인벤토리가 꽉찼고 &&소모품은 없을 경우 
                    else
                    {
                        Debug.Log("픽업4");

                        //아무것도 하지않고 반복문 나감
                        return;                                                   
                    }
                }
            }
            InfoDisAppear();
        }
    }

    /// <summary>
    /// 아이템 정보 확인
    /// </summary>
    private void CheckItem()
    {
        // Editor창에서 확인
        Debug.DrawRay(cam.transform.position + cam.transform.forward * minRange, cam.transform.forward * maxRange, Color.blue);

        // 인스팩터 창에서 Layer 선택(item으로) 없으면 추가 해야함 
        if (Physics.Raycast(cam.transform.position + cam.transform.forward * minRange, cam.transform.forward, out hitInfo, maxRange, itemLayer)) 
        {
            if (hitInfo.transform.tag == "Item")
            {
                // 문을 인식하고 UI 출력해주는 로직
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
    #region 아이템 습득 메소드

    /// <summary>
    /// //인벤토리가 다 안찼을때 
    /// </summary>
    void checkInven_NotFull()        
    {
        Debug.Log("안참1");

        //레이가 허공 가리키고있을때 널 오류뜨는거 막으려고 조건추가 
        if (hitInfo.transform.GetComponent<ItemPickUp>() == null) return; 

        //CanPickUp에서 아이템과 인벤토리 아이템 이름을 비교 (인벤토리에 같은 이름의 소모품이 있는지 없는지 검사) 할 오브젝트 저장
        //slotCheck_used() 에서 사용할거라 여기서 저장 안해도되는데 나중에 특수아이템 (무조건 1개만 가지고 있을 수 있다던가) 같은 조건이 나올수도 있을듯해서 그냥 진입할때 저장시켜놓고 있습니다
        getItemName = hitInfo.transform.GetComponent<ItemPickUp>().gameObject;

        //인벤토리가 가득 차지않은 상태라면
        if (!theInventory.IsInventoryFull)            
        {
            Debug.Log("안참1");

            //아이템 먹기 실행
            getItem();                              
        }
    }

    /// <summary>
    /// 아이템 먹기  
    /// 상자에서 나온 아이템은 ?득 카운트와 상위 부모를 찾아주는 역할을 해서 없어지게 하면 안됨 나중에 부모 자체를 없애기에 상관없음 
    /// </summary>
    void getItem()                                  
    {
        Debug.Log("아이템먹음1");

        //슬롯으로 아이템 정보와 수량을 추가해줍니다
        theInventory.AcquireItem(hitInfo.transform.GetComponent<ItemPickUp>().item, 1);

        //마스터 외의 클라이언트가 다른 오브젝트에 접근할 권한이 없어 포톤뷰로 접근하기 위해 저장
        int itemPVID = hitInfo.transform.GetComponent<PhotonView>().ViewID;

        //마스터 클라이언트일 경우
        if (PhotonNetwork.IsMasterClient)    
        {
            Debug.Log("아이템먹음1");

            //위에 저장한 포톤뷰 아이디를 검색해서 오브젝트로 저장합니다 
            GameObject PV_item = PhotonView.Find(itemPVID).gameObject;

            //저장한 오브젝트의 아이템 박스 정보가 널이 아니라면 실행 (드랍아이템은 박스정보가 널 입니다 , 박스가 없어서 카운트를 실행하면 널오류로 멈춰서 조건 추가 )
            if (PV_item.GetComponent<ItemPickUp>().itembox != null)
            {
                //저장된 박스 메소드를 실행 시킵니다 (박스 아이템을 먹었는지 , 다먹으면 박스 파괴)
                PV_item.GetComponent<ItemPickUp>().itembox.ItemCalCount();
            }

            //먹은 오브젝트 파괴 
            PhotonNetwork.Destroy(PV_item);
        }
        //마스터가 아닌 클라이언트일 경우
        else
        {
            Debug.Log("클라가 아이템먹음1");
            PV.RPC("RPC_itemboxCount", RpcTarget.MasterClient, itemPVID);
            PV.RPC("RPC_Destoryitem", RpcTarget.MasterClient, itemPVID);
        }
    }

    /// <summary>
    /// 아이템 박스 카운트 줄이기 
    /// </summary>
    /// <param name="itemPV"></param>
    [PunRPC]
    void RPC_itemboxCount(int itemPV)
    {
        // 아이템 박스 카운트 줄이기 
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
    /// 드랍한 아이템 ?득
    /// </summary>
    void dorpItem()  
    {
        //플레이어가 드랍한 아이템은 딱히 체크할 기능이없으므로 그냥 삭제 시킴 
        theInventory.AcquireItem(hitInfo.transform.GetComponent<ItemPickUp>().item, 1);

        // 드랍 아이템 삭제
        Destroy(hitInfo.transform.GetComponent<ItemPickUp>().gameObject);
    }

    /// <summary>
    /// 인벤토리 슬롯 체크
    /// </summary>
    void slotCheck_used()  
    {
        for (int i = 0; i < theInventory.slots.Length; i++)
        {
            //인벤토리 슬롯에 아이템이 있을 경우에만 검사하고 레이에 맞은 아이템의 이름이 슬롯에 이름이 같을 경우만 실행
            //인벤토리에 같이 넣어둘까 했는데 업데이트에서 계속 돌리는게 자원낭비같아서 그냥 액션에 반응할때 실행하도록 했습니다 
            if (theInventory.slots[i].item != null)                  
            {
                if (theInventory.slots[i].item.itemName == getItemName.GetComponent<ItemPickUp>().item.itemName)
                {
                    //상자에서 먹는경우
                    if (hitInfo.transform.GetComponent<ItemPickUp>().itembox != null)         
                    {
                        Debug.Log("슬롯 다참 겟 아이템");

                        //아이템 먹기 실행 
                        getItem();        
                    }
                    //드랍 아이템일 경우 
                    else
                    {
                        Debug.Log("슬롯 다참 드랍 아이템");

                        //드랍아이템 먹기 실행  
                        dorpItem();      
                    }
                }
            }
        }
    }

    #endregion 아이템 ?득 메서드

    /// <summary>
    /// 문을 여는 메서드
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

    // 아이템 UI 출력 관련 메서드
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
        actionText.text = hitInfo.transform.name + " 상호작용 " + "<color=red>" + "(E)키" + "</color>";
    }

    private void DoorLockInfoAppear()
    {
        doorActivated = true;
        actionText.gameObject.SetActive(true);
        actionText.text = hitInfo.transform.name + " 이 잠겼습니다. " + "<color=red>" + "카드 키 필요" + "</color>";
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
        actionText.text = hitInfo.transform.GetComponent<ItemPickUp>().item.itemName + " 획득 " + "<color=yellow>" + "(E)키" + "</color>";
    }
    #endregion
}
