using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
//using static UnityEditor.PlayerSettings;   내가안슴 뭔지모르겠슴

public class ItemBox : MonoBehaviourPun
{
    public PhotonView PV;                   // [PunRPC]를 실행 하기 위해 선언

    [SerializeField] private Animator ani;  // 박스 애니메이터를 넣어주세요

    [SerializeField] private int[] weight;  //아이템 각각이 들고 있을 가중치 배열 


    // 박스가 들고 있을 아이템 인데 아이템 확률은 가중치 배열이 들고있어서 배열이랑 아이템 순서 맞아야 해요 
    [SerializeField] private GameObject[] items;

    // 박스 드랍 갯수
    [SerializeField] private float drop_Count;       

    // 뒤에서 drop_count 에서 사용할 최대 최소값 테스트용이라 1로 해두고 있어요 
    [SerializeField] private int maxDropCount = 2;   
    [SerializeField] private int minDropCount = 1;

    // 스폰너 받아오기 false가 아이템 받을 수 없는 상태 true가 받을수 있는상태  
    [SerializeField] private Transform[] spawn;      

    public Transform[] Spawn => spawn;

    public bool Box_state = false;  //박스 오픈상태 체크 true가 오픈상태 
    public bool item_state = false; //아이템이 생성된 적이 있는지 true는 생성된적 있음 false는 처음 생성 true상태면 다시는 스폰 안됨


    public int total = 0;           // 아이템들 가중치 총합 100으로 맞추는게 가장 보기 편해요 
    public int itemCarryNum;        // 각 상자별 스포너 개수 정하는곳 소모품 :2개 무기상자 :1 개 
    public GameObject selsetItem;   // 선택한 아이템 받아옴 
    public GameObject item_potion;  //소모품 인지 아닌지 확인하는 용도     
    public GameObject ParentSpawn;  // 부모객체로 자식를 넘기기위해 사용 (현재는 안사용중인데 확장성때문에 남겨둠)
    public int DestroyCount;        //상자를 파괴하기 위한 카운트 
    public int viewID;              //상자의 포톤뷰를 찾아서 ID저장해두는곳 
    public RandomSelect get_list;   //상자에서 나올 아이템 정보를 받아오기위한 용도 randomlist스크립트 받아옴
    int item_weigth;                //받아온 아이템의 가중치를 더할공간 
    int itemDestroy_Check = 0;      //상자의 아이템이 전부 파괴되었는지 상태 체크용 

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    void Start()
    {
        //생성될때 아이템상자 뷰를 가져옴
        viewID = GetComponent<ItemBox>().PV.ViewID;

        //아이템 리스트를 받아옴 (인스펙터에서 직접 정용시켜줘야합니다)
        get_list = GetComponent<RandomSelect>();

        // 스포너 개수 받아옴 
        itemCarryNum = spawn.Length;

        //상자 자체가 들고있는 카운트를 스포너 개수랑 동기화 
        DestroyCount = itemCarryNum;                    
    }

    /// <summary>
    /// 매 프레임마다 실행
    /// </summary>
    private void Update()
    {
        //호출자가 내가 아니라면 진행 못하게 제한
        if (!PV.IsMine) return;                      

        if (DestroyCount == 0)
        {
            itemDestroy_Check++;

            //카운트가 0이되고 아이템이 전부 파괴가 되었는지 
            if (DestroyCount == 0 && itemDestroy_Check == 1)    
            {
                PV.RPC("Boxstate", RpcTarget.All);       
            }
        }
    }

    /// <summary>
    /// 아이템 습득시 실행되는 메서드
    /// </summary>
    [PunRPC]
    public void ItemCalCount()
    {
        //박스 카운트 줄여서 0으로 만들면 박스 파괴시킴  아이템 습득시 진행 
        DestroyCount--;           
    }

    /// <summary>
    /// 아이템 생성기 위치에 가중치 기반의 랜덤 아이템 생성
    /// </summary>
    [PunRPC]
    public void SetWeightBox()
    {
        if (!PV.IsMine) return;

        //스포너의 개수만큼 반복
        for (int i = 0; i < spawn.Length; i++)   
        {
            float random = UnityEngine.Random.Range(0.0f, 1.0f);
            item_weigth = 0;                            //가중치 받을 곳 , 더하는곳 
            int Selitem = 0;                                //선택된 가중치 
            Selitem = Mathf.RoundToInt(get_list.total * random);     //선택할 아이템의 수치 라고해야하나.. 기준치 입니다 

            //아이템 갯수만큼 확률을 돌립니다 
            for (int j = 0; j < get_list.ITEM_LIST.Count; j++)         
            {
                //기준을 만족 하지못하면 가중치 추가 
                item_weigth += get_list.ITEM_LIST[j].weight;

                //현재 가중치 보다 작거나 같은 선택된 가중치의 아이템을 불러옴 &&스포너의 상태가 true (아이템을 먹으면 false로 변함)
                if (Selitem <= item_weigth && spawn[i].gameObject.activeSelf == true)   
                {
                    //생성될 아이템은 itemPickUp을 가지고 있으므로 item형식으로 저장 아래에 타입비교할때 쓰입니다 
                    var spawnItem = get_list.ITEM_LIST[j].items.GetComponent<ItemPickUp>().item;

                    //위치 바꾸시고 싶으면 하이라이키에서 그냥 이동 시키면 됩니다
                    Vector3 createPos = spawn[i].transform.position;

                    //생성되는 장비의 타입이 Equipment 일경우 로테이션을 다르게 사용해서 구분해줬습니다.
                    if (spawnItem.itemType == Item.ItemType.Equipment)             
                    {
                        if (!PhotonNetwork.IsMasterClient) return;
                        StartCoroutine(item_Weapon_spawn(i, j, createPos));

                        //안넣으면 아이템 연속으로 나옴 조건 추가보단 이게 깔끔해 보입니다 
                        break;

                    }
                    //장비가 아니라면 = 소모품
                    else
                    {                                                               
                        if (!PhotonNetwork.IsMasterClient) return;
                        string path = "Item/";
                        path += get_list.ITEM_LIST[j].items.name;
                        item_potion = PhotonNetwork.Instantiate(path, createPos, get_list.ITEM_LIST[j].items.transform.rotation);
                        item_potion.gameObject.GetComponent<ItemPickUp>().itembox = this.gameObject.GetComponent<ItemBox>();
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 아이템 상자 열기
    /// </summary>
    [PunRPC]
    public void OpenBox()
    {
        //최소 최대 정하는데 어차피 스포너 1개에 한아이템만 등장시킬꺼라 하셔서 그냥 인수로 넣어도 될듯합니다 
        drop_Count = UnityEngine.Random.Range(minDropCount, maxDropCount);

        //박스가 열리지않았고 아이템이 등장하지 않았을때 
        if (!Box_state && !item_state)                                     
        {
            //박스 오픈 , 아이템 등장 상태로 변경
            Box_state = true;                                               
            item_state = true;

            //애니메이션은 한번만 
            ani.SetTrigger("boxOpen");

            //아이템 박스는 1회용이라 다시인식 못하게 하려고 콜라이더 비활성화 시켰습니다
            gameObject.GetComponent<BoxCollider>().enabled = false; 

            for (int i = 0; i < drop_Count; i++)
            {
                //랜덤 생성 실행
                SetWeightBox(); 
            }
        }
    }

    /// <summary>
    /// 아이템을 다먹은후 박스를 제거할때 사용 
    /// </summary>
    [PunRPC]
    public void Boxstate()                                      
    {
        Box_state = false;          //박스를 닫음 상태로 변경
        item_state = true;          //아이템이 생성되었다는 상태 

        //액션 스크립트에서 박스 스포너와 아이템 ?득수를 비교할때 사용 합니다 박스를 제거할것이기에 ?득수 0으로 초기화
        ani.SetTrigger("boxClose");  
        
        // 오브젝트 1초 후 삭제
        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// 무기 아이템 생성 코루틴 실행 메서드
    /// </summary>
    /// <param name="Spawn">스포너 수</param>
    /// <param name="Index">go_dropItem 수</param>
    /// <param name="pos">위치값</param>
    [PunRPC]
    void itemWeaponSpawnRPC(int Spawn, int Index, Vector3 pos)
    {
        StartCoroutine(item_Weapon_spawn(Spawn, Index, pos));
    }

    /// <summary>
    /// 무기 아이템 생성
    /// </summary>
    /// <param name="Spawn">스포너 수</param>
    /// <param name="Index">go_dropItem 수</param>
    /// <param name="pos"> 위치값</param>
    /// <returns></returns>
    IEnumerator item_Weapon_spawn(int Spawn, int Index, Vector3 pos)
    {
        yield return new WaitForSeconds(0.5f);
        string path = "Item/";
        path += get_list.ITEM_LIST[Index].items.name;
        Vector3 spawnPos = pos;
        spawnPos.y += 0.1f;
        pos = spawnPos;
        GameObject item_Weapon = PhotonNetwork.Instantiate(path, pos, get_list.ITEM_LIST[Index].items.transform.rotation);
        item_Weapon.gameObject.GetComponent<ItemPickUp>().itembox = this.gameObject.GetComponent<ItemBox>();
    }
}
