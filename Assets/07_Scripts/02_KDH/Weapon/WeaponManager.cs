using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviourPunCallbacks
{
    private PhotonView PV;

    public static bool isChangeWeapon;             // 무기 변경 중 행동 방지를 위한 변수

    public Transform currentWeapon;         // ?
    public Animator currentWeaponAnim;      // 현재 무기의 애니메이터 사용



    [SerializeField] private string currentWeaponType;          // 현재 무기의 타입
    [SerializeField] private float changeWeaponDelayTime;       // 무기 교체 딜레이 시간
    [SerializeField] private float changeWeaponEndDelayTime;    // 무기 교체가 끝나는 시간

    [SerializeField] private Gun[] guns;                        // Gun 스크립트가 포함된 원거리 무기 데이터를 배열로 저장
    [SerializeField] private  Melee[] melees;                   // Melee 스크립트가 포함된 무기 데이터를 저장

    Dictionary<string, Gun> gunDictionary = new Dictionary<string, Gun>();         // item DB에 저장된 이름과, Type으로 무기 변경을 위해 사용
    Dictionary<string, Melee> handDictionary = new Dictionary<string, Melee>(); 

    // 필요 컴포넌트
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

        // 기본으로 제공하는 주먹 공격. 6번 키로 사용 가능
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




    public IEnumerator ChangeWeaponCoroutine(string _type, string _name) // 무기 변경 코루틴
    {
        isChangeWeapon = true;                                 // 무기 변경 딜레이 타임 동안에 행동을 막기 위해 사용

        // 무기 변경 애니메이션 추가

        yield return new WaitForSeconds(changeWeaponDelayTime);

        CancelPreWeaponAction();
        WeaponChange(_type, _name);

        yield return new WaitForSeconds(changeWeaponEndDelayTime);
        currentWeaponType = _type;
        isChangeWeapon = false;
    }

    private void CancelPreWeaponAction()                  // 이전 무기의 정보 데이터 변경
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

    public void WeaponChange(string _type, string _name)        // 지정한 타입, 무기로 변경
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
