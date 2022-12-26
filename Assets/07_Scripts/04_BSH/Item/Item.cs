using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Item/item")]
public class Item : ScriptableObject          // ������ �����ͺ��̽��� ������Ʈ ���� ���
{
    public string itemName;       // �������� �̸�
    public ItemType itemType;     // �������� ����
    public Sprite itemImage;      // �κ��丮 ������ �̹���
    public GameObject itemPrefab; // �������� ������(��� ������)

    public string weaponType;     // ���� ����(�ϵ��ڵ� : GUN, HAND) �Է�

    public enum ItemType 
    {
        Equipment,
        Used,
        Ingredient,               // ���� �̻��
        ETC                       // ���� �̻��
    }
}
    