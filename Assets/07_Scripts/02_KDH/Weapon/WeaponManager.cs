using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviourPunCallbacks
{
    private PhotonView PV;

    public static bool isChangeWeapon;             // ���� ���� �� �ൿ ������ ���� ����

    public Transform currentWeapon;         // ?
    public Animator currentWeaponAnim;      // ���� ������ �ִϸ����� ���



    [SerializeField] private string currentWeaponType;          // ���� ������ Ÿ��
    [SerializeField] private float changeWeaponDelayTime;       // ���� ��ü ������ �ð�
    [SerializeField] private float changeWeaponEndDelayTime;    // ���� ��ü�� ������ �ð�

    [SerializeField] private Gun[] guns;                        // Gun ��ũ��Ʈ�� ���Ե� ���Ÿ� ���� �����͸� �迭�� ����
    [SerializeField] private  Melee[] melees;                   // Melee ��ũ��Ʈ�� ���Ե� ���� �����͸� ����

    Dictionary<string, Gun> gunDictionary = new Dictionary<string, Gun>();         // item DB�� ����� �̸���, Type���� ���� ������ ���� ���
    Dictionary<string, Melee> handDictionary = new Dictionary<string, Melee>(); 

    // �ʿ� ������Ʈ
    [SerializeField] private GunController theGunController;
    [SerializeField] private HandController theHandController;
    [SerializeField] private KeyController theKeyController;

    void Start()
    {
        PV = GetComponentInParent<PhotonView>();

        for (int i = 0; i < guns.Length; i++)
        {
            gunDictionary.Add(guns[i].gunName, guns[i]);
        }

        for (int i = 0; i < melees.Length; i++)
        {
            handDictionary.Add(melees[i].meleeName, melees[i]);
        }

        //PV.RPC("ChangeWeaponRPC", RpcTarget.AllBuffered, "HAND", "Hand");
        StartCoroutine(ChangeWeaponCoroutine("HAND", "Hand"));
    }

    void Update()
    {
        if (!PV.IsMine) return;

        // �⺻���� �����ϴ� �ָ� ����. 6�� Ű�� ��� ����
        if (!isChangeWeapon)
        {
            if (Input.GetKeyDown(KeyCode.Alpha6))
                //PV.RPC("ChangeWeaponRPC", RpcTarget.AllBuffered, "HAND", "Hand");
                StartCoroutine(ChangeWeaponCoroutine("HAND", "Hand"));
        }
    }

    public void ChangeWeapon(string _type, string _name)
    {
        StartCoroutine(ChangeWeaponCoroutine(_type, _name));
    }




    public IEnumerator ChangeWeaponCoroutine(string _type, string _name) // ���� ���� �ڷ�ƾ
    {
        isChangeWeapon = true;                                 // ���� ���� ������ Ÿ�� ���ȿ� �ൿ�� ���� ���� ���

        // ���� ���� �ִϸ��̼� �߰�

        yield return new WaitForSeconds(changeWeaponDelayTime);

        CancelPreWeaponAction();
        WeaponChange(_type, _name);

        yield return new WaitForSeconds(changeWeaponEndDelayTime);
        currentWeaponType = _type;
        isChangeWeapon = false;
    }

    private void CancelPreWeaponAction()                  // ���� ������ ���� ������ ����
    {
        switch (currentWeaponType)
        {
            case "Gun":
                theGunController.CancelReload();
                theGunController.isActivate = false;
                theKeyController.isActivate = false;
                break;
            case "Hand":
                theHandController.isActivate = false;
                theKeyController.isActivate = false; 
                break;
            case "Key":
                theHandController.isActivate = false;
                theGunController.isActivate = false;
                break;
        }
    }

    public void WeaponChange(string _type, string _name)        // ������ Ÿ��, ����� ����
    {
        if (_type == "GUN")
        {
            theGunController.GunChange(gunDictionary[_name]);
        }
        else if (_type == "HAND")
        {
            theHandController.MeleeChange(handDictionary[_name]);
        }else if(_type == "KEY")
        {
            theKeyController.MeleeChange(handDictionary[_name]);
        }
    }
}
