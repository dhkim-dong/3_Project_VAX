using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
//using static UnityEditor.PlayerSettings;   �����Ƚ� �����𸣰ڽ�

public class ItemBox : MonoBehaviourPun
{
    public PhotonView PV;                   // [PunRPC]�� ���� �ϱ� ���� ����

    [SerializeField] private Animator ani;  // �ڽ� �ִϸ����͸� �־��ּ���

    [SerializeField] private int[] weight;  //������ ������ ��� ���� ����ġ �迭 


    // �ڽ��� ��� ���� ������ �ε� ������ Ȯ���� ����ġ �迭�� ����־ �迭�̶� ������ ���� �¾ƾ� �ؿ� 
    [SerializeField] private GameObject[] items;

    // �ڽ� ��� ����
    [SerializeField] private float drop_Count;       

    // �ڿ��� drop_count ���� ����� �ִ� �ּҰ� �׽�Ʈ���̶� 1�� �صΰ� �־�� 
    [SerializeField] private int maxDropCount = 2;   
    [SerializeField] private int minDropCount = 1;

    // ������ �޾ƿ��� false�� ������ ���� �� ���� ���� true�� ������ �ִ»���  
    [SerializeField] private Transform[] spawn;      

    public Transform[] Spawn => spawn;

    public bool Box_state = false;  //�ڽ� ���»��� üũ true�� ���»��� 
    public bool item_state = false; //�������� ������ ���� �ִ��� true�� �������� ���� false�� ó�� ���� true���¸� �ٽô� ���� �ȵ�


    public int total = 0;           // �����۵� ����ġ ���� 100���� ���ߴ°� ���� ���� ���ؿ� 
    public int itemCarryNum;        // �� ���ں� ������ ���� ���ϴ°� �Ҹ�ǰ :2�� ������� :1 �� 
    public GameObject selsetItem;   // ������ ������ �޾ƿ� 
    public GameObject item_potion;  //�Ҹ�ǰ ���� �ƴ��� Ȯ���ϴ� �뵵     
    public GameObject ParentSpawn;  // �θ�ü�� �ڽĸ� �ѱ������ ��� (����� �Ȼ�����ε� Ȯ�强������ ���ܵ�)
    public int DestroyCount;        //���ڸ� �ı��ϱ� ���� ī��Ʈ 
    public int viewID;              //������ ����並 ã�Ƽ� ID�����صδ°� 
    public RandomSelect get_list;   //���ڿ��� ���� ������ ������ �޾ƿ������� �뵵 randomlist��ũ��Ʈ �޾ƿ�
    int item_weigth;                //�޾ƿ� �������� ����ġ�� ���Ұ��� 
    int itemDestroy_Check = 0;      //������ �������� ���� �ı��Ǿ����� ���� üũ�� 

    /// <summary>
    /// ������Ʈ �ʱ�ȭ
    /// </summary>
    void Start()
    {
        //�����ɶ� �����ۻ��� �並 ������
        viewID = GetComponent<ItemBox>().PV.ViewID;

        //������ ����Ʈ�� �޾ƿ� (�ν����Ϳ��� ���� �����������մϴ�)
        get_list = GetComponent<RandomSelect>();

        // ������ ���� �޾ƿ� 
        itemCarryNum = spawn.Length;

        //���� ��ü�� ����ִ� ī��Ʈ�� ������ ������ ����ȭ 
        DestroyCount = itemCarryNum;                    
    }

    /// <summary>
    /// �� �����Ӹ��� ����
    /// </summary>
    private void Update()
    {
        //ȣ���ڰ� ���� �ƴ϶�� ���� ���ϰ� ����
        if (!PV.IsMine) return;                      

        if (DestroyCount == 0)
        {
            itemDestroy_Check++;

            //ī��Ʈ�� 0�̵ǰ� �������� ���� �ı��� �Ǿ����� 
            if (DestroyCount == 0 && itemDestroy_Check == 1)    
            {
                PV.RPC("Boxstate", RpcTarget.All);       
            }
        }
    }

    /// <summary>
    /// ������ ����� ����Ǵ� �޼���
    /// </summary>
    [PunRPC]
    public void ItemCalCount()
    {
        //�ڽ� ī��Ʈ �ٿ��� 0���� ����� �ڽ� �ı���Ŵ  ������ ����� ���� 
        DestroyCount--;           
    }

    /// <summary>
    /// ������ ������ ��ġ�� ����ġ ����� ���� ������ ����
    /// </summary>
    [PunRPC]
    public void SetWeightBox()
    {
        if (!PV.IsMine) return;

        //�������� ������ŭ �ݺ�
        for (int i = 0; i < spawn.Length; i++)   
        {
            float random = UnityEngine.Random.Range(0.0f, 1.0f);
            item_weigth = 0;                            //����ġ ���� �� , ���ϴ°� 
            int Selitem = 0;                                //���õ� ����ġ 
            Selitem = Mathf.RoundToInt(get_list.total * random);     //������ �������� ��ġ ����ؾ��ϳ�.. ����ġ �Դϴ� 

            //������ ������ŭ Ȯ���� �����ϴ� 
            for (int j = 0; j < get_list.ITEM_LIST.Count; j++)         
            {
                //������ ���� �������ϸ� ����ġ �߰� 
                item_weigth += get_list.ITEM_LIST[j].weight;

                //���� ����ġ ���� �۰ų� ���� ���õ� ����ġ�� �������� �ҷ��� &&�������� ���°� true (�������� ������ false�� ����)
                if (Selitem <= item_weigth && spawn[i].gameObject.activeSelf == true)   
                {
                    //������ �������� itemPickUp�� ������ �����Ƿ� item�������� ���� �Ʒ��� Ÿ�Ժ��Ҷ� ���Դϴ� 
                    var spawnItem = get_list.ITEM_LIST[j].items.GetComponent<ItemPickUp>().item;

                    //��ġ �ٲٽð� ������ ���̶���Ű���� �׳� �̵� ��Ű�� �˴ϴ�
                    Vector3 createPos = spawn[i].transform.position;

                    //�����Ǵ� ����� Ÿ���� Equipment �ϰ�� �����̼��� �ٸ��� ����ؼ� ����������ϴ�.
                    if (spawnItem.itemType == Item.ItemType.Equipment)             
                    {
                        if (!PhotonNetwork.IsMasterClient) return;
                        StartCoroutine(item_Weapon_spawn(i, j, createPos));

                        //�ȳ����� ������ �������� ���� ���� �߰����� �̰� ����� ���Դϴ� 
                        break;

                    }
                    //��� �ƴ϶�� = �Ҹ�ǰ
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
    /// ������ ���� ����
    /// </summary>
    [PunRPC]
    public void OpenBox()
    {
        //�ּ� �ִ� ���ϴµ� ������ ������ 1���� �Ѿ����۸� �����ų���� �ϼż� �׳� �μ��� �־ �ɵ��մϴ� 
        drop_Count = UnityEngine.Random.Range(minDropCount, maxDropCount);

        //�ڽ��� �������ʾҰ� �������� �������� �ʾ����� 
        if (!Box_state && !item_state)                                     
        {
            //�ڽ� ���� , ������ ���� ���·� ����
            Box_state = true;                                               
            item_state = true;

            //�ִϸ��̼��� �ѹ��� 
            ani.SetTrigger("boxOpen");

            //������ �ڽ��� 1ȸ���̶� �ٽ��ν� ���ϰ� �Ϸ��� �ݶ��̴� ��Ȱ��ȭ ���׽��ϴ�
            gameObject.GetComponent<BoxCollider>().enabled = false; 

            for (int i = 0; i < drop_Count; i++)
            {
                //���� ���� ����
                SetWeightBox(); 
            }
        }
    }

    /// <summary>
    /// �������� �ٸ����� �ڽ��� �����Ҷ� ��� 
    /// </summary>
    [PunRPC]
    public void Boxstate()                                      
    {
        Box_state = false;          //�ڽ��� ���� ���·� ����
        item_state = true;          //�������� �����Ǿ��ٴ� ���� 

        //�׼� ��ũ��Ʈ���� �ڽ� �����ʿ� ������ ?����� ���Ҷ� ��� �մϴ� �ڽ��� �����Ұ��̱⿡ ?��� 0���� �ʱ�ȭ
        ani.SetTrigger("boxClose");  
        
        // ������Ʈ 1�� �� ����
        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// ���� ������ ���� �ڷ�ƾ ���� �޼���
    /// </summary>
    /// <param name="Spawn">������ ��</param>
    /// <param name="Index">go_dropItem ��</param>
    /// <param name="pos">��ġ��</param>
    [PunRPC]
    void itemWeaponSpawnRPC(int Spawn, int Index, Vector3 pos)
    {
        StartCoroutine(item_Weapon_spawn(Spawn, Index, pos));
    }

    /// <summary>
    /// ���� ������ ����
    /// </summary>
    /// <param name="Spawn">������ ��</param>
    /// <param name="Index">go_dropItem ��</param>
    /// <param name="pos"> ��ġ��</param>
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
