using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Item/item")]
public class Item : ScriptableObject          // 아이템 데이터베이스용 오브젝트 생성 방식
{
    public string itemName;       // 아이템의 이름
    public ItemType itemType;     // 아이템의 유형
    public Sprite itemImage;      // 인벤토리 아이템 이미지
    public GameObject itemPrefab; // 아이템의 프리펩(드랍 아이템)

    public string weaponType;     // 무기 유형(하드코딩 : GUN, HAND) 입력

    public enum ItemType 
    {
        Equipment,
        Used,
        Ingredient,               // 현재 미사용
        ETC                       // 현재 미사용
    }
}
    