using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ItemPickUp : MonoBehaviourPun  // �������� ��� ���� Ŭ����
{
    public Item item;
    public PhotonView PV;
    public ItemBox itembox;  //�����ɽ� �����۹ڽ� ������Ʈ�� �����ϱ� ���� ���� 
    public int viewID;    //�����۹ڽ��� ����信 �����ϱ����� ����

}
