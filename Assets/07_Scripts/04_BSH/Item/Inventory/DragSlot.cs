using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragSlot : MonoBehaviourPun
{
    static public DragSlot instance;              // �̱��� ����

    public Slot dragSlot;                         // �巡�� �� ������ Slot ������

    [SerializeField] private Image imageItem;     // ������ �̹���

    [SerializeField] Transform playerPos;         // ��� ��ġ ���� ��
    [SerializeField] Vector3 dropOffset;          // ��� ��ġ ���� ��

    [SerializeField] PhotonView PV;

    private void Start()
    {
        instance = this;
    }

    public void DragSetImage(Image _itemImage)    // �巡�� �� ������ �̹��� ���
    {
        imageItem.sprite = _itemImage.sprite;     // �巡�� �������� �̹����� ������ ������ ��ġ��Ű��
        SetColor(1);                              // �̹��� ���̰� ���
    }

    public void SetColor(float _alpha)            // ���İ����� �̹��� ��� ���� 
    {
        Color color = imageItem.color;
        color.a = _alpha;
        imageItem.color = color;                  // ���İ�(0)�� ������ ����, (1)�� ������ ���̰�
    }

    public void SetPrefab(string _gameName)   // �������� ������ �������� �����ϱ�
    {
        // ��� �������� ��ġ ����
        dropOffset = playerPos.forward;
        dropOffset.y = 0;
        dropOffset = dropOffset.normalized;
        // ��� ������ ������ ����
        var DropItem = PhotonNetwork.Instantiate("Item/" + _gameName, playerPos.position + dropOffset, Quaternion.identity);
    }
}
