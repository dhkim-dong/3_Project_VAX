using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]       // �ν����� â���� ItemEffect �߰� ����
public class ItemEffect
{
    public string itemName; // ������ �̸� (Ű��) => ScriptableObj�� �ִ� ������ �̸��� ��ġ�ؾ� �ҷ����� ���� ����������.
    [Tooltip("���� �Է��� �� �ִ� �� HP, SP �̿� ���� �߻�")]
    public string[] part;  // ȿ���� ���� ���� �����ϰ� �迭�� ���� => HP,SP ��� ������ ȿ���� ����� �κ��� �ִ´�. 1�����ۿ� ���� ȿ�� ȸ�� ����
    public int[] num; //      ȿ�� ��ġ �� // part������ ��ġ�ϰ� ��ġ�� �־���� �Ѵ�.
}

public class ItemEffectDatabase : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView PV;

    [SerializeField] private ItemEffect[] itemEffects;    // ���� ������ ȿ��

    // �ʿ��� ������Ʈ
    [SerializeField] private StatusController statusController;    // �������ͽ��� �����Ű�� ���� �ҷ��;���. Player �ֻ��� ������Ʈ�� �������. 
    [SerializeField] private WeaponManager weaponManager;          // Player. WeaponHolder ���ӿ�����Ʈ�� �־������. Scene���� ������ Ŭ���ϸ� ���� ã�� �� ����.
    private const string HP = "HP", SP = "SP";

    public void UseItem(Item _item)
    {
        if (_item.itemType == Item.ItemType.Equipment)              // DataBase ��ũ��Ʈ���� ��� ������ ������ ��� ȿ�� �Ѵ� ����
        {
            if (!PV.IsMine) return;
            PV.RPC("PlayerWeaponRPC", RpcTarget.All, _item.weaponType, _item.itemName);              // WeaponManager�� �ִ� ChanagerWeapon�� RPC�� ȣ��
        }

        else if (_item.itemType == Item.ItemType.Used)               // �Ҹ� ������ ���
        {
            for (int x = 0; x < itemEffects.Length; x++)            // database�� ��ϵ� ȿ���� ã�Ƽ�
            {

                if (itemEffects[x].itemName == _item.itemName)       // 1. database�� �ִ� �̸��� ������ �������� �̸��� ������ üũ
                {

                    for (int y = 0; y < itemEffects[x].part.Length; y++) // 2. �̸��� ���ٸ�, database�� Part �κп� ���� SP, HP�� Ž���Ͽ� ����
                    {

                        switch (itemEffects[x].part[y])                           // �߰� �ϰ� ���� ������ ȿ��  case�� �߰� ���ָ� �ȴ�.
                        {
                            case HP:
                                statusController.IncreaseHP(itemEffects[x].num[y]);
                                break;
                            case SP:
                                statusController.IncreaseSP(itemEffects[x].num[y]);
                                break;
                            default:
                                Debug.Log("�߸��� Status ����");
                                break;
                        }

                    }
                    return;
                }


            }
            Debug.Log(" itemEffectDatabase�� ��ġ�ϴ� itemName�� �����ϴ�");
        }
    }
}
