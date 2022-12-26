using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]       // 인스팩터 창에서 ItemEffect 추가 가능
public class ItemEffect
{
    public string itemName; // 아이템 이름 (키값) => ScriptableObj에 있는 아이템 이름과 일치해야 불러오는 것이 가능해진다.
    [Tooltip("현재 입력할 수 있는 값 HP, SP 이외 오류 발생")]
    public string[] part;  // 효과가 복수 적용 가능하게 배열로 선언 => HP,SP 등등 아이템 효과가 적용될 부분을 넣는다. 1아이템에 복수 효과 회복 가능
    public int[] num; //      효과 수치 값 // part순서와 일치하게 수치를 넣어줘야 한다.
}

public class ItemEffectDatabase : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView PV;

    [SerializeField] private ItemEffect[] itemEffects;    // 복수 아이템 효과

    // 필요한 컴포넌트
    [SerializeField] private StatusController statusController;    // 스테이터스를 적용시키기 위해 불러와야함. Player 최상위 오브젝트를 넣으면됨. 
    [SerializeField] private WeaponManager weaponManager;          // Player. WeaponHolder 게임오브젝트를 넣어줘야함. Scene에서 오른손 클릭하면 쉽게 찾을 수 있음.
    private const string HP = "HP", SP = "SP";

    public void UseItem(Item _item)
    {
        if (_item.itemType == Item.ItemType.Equipment)              // DataBase 스크립트에서 장비 장착과 아이템 사용 효과 둘다 관리
        {
            if (!PV.IsMine) return;
            PV.RPC("PlayerWeaponRPC", RpcTarget.All, _item.weaponType, _item.itemName);              // WeaponManager에 있는 ChanagerWeapon을 RPC로 호출
        }

        else if (_item.itemType == Item.ItemType.Used)               // 소모성 아이템 사용
        {
            for (int x = 0; x < itemEffects.Length; x++)            // database에 등록된 효과를 찾아서
            {

                if (itemEffects[x].itemName == _item.itemName)       // 1. database에 있는 이름과 습득한 아이템의 이름이 같은지 체크
                {

                    for (int y = 0; y < itemEffects[x].part.Length; y++) // 2. 이름이 같다면, database의 Part 부분에 적힌 SP, HP를 탐지하여 적용
                    {

                        switch (itemEffects[x].part[y])                           // 추가 하고 싶은 아이템 효과  case로 추가 해주면 된다.
                        {
                            case HP:
                                statusController.IncreaseHP(itemEffects[x].num[y]);
                                break;
                            case SP:
                                statusController.IncreaseSP(itemEffects[x].num[y]);
                                break;
                            default:
                                Debug.Log("잘못된 Status 부위");
                                break;
                        }

                    }
                    return;
                }


            }
            Debug.Log(" itemEffectDatabase에 일치하는 itemName이 없습니다");
        }
    }
}
