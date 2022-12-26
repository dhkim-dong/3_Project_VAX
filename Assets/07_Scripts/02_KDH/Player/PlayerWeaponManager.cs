using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviourPun
{
    [SerializeField] private PhotonView PV;

    [SerializeField] private WeaponManager weaponManager;

    [SerializeField] private GunController gunController;

    [SerializeField] private HandController handController;

    [SerializeField] private KeyController keyController;

    [PunRPC]
    public void PlayerWeaponRPC(string _type, string _name)
    {
        weaponManager.WeaponChange(_type, _name);
    }


    [PunRPC]
    private void MeleeAttack()
    {
        StartCoroutine(handController.AttackCoroutine());
    }

    [PunRPC]
    private void ShootRPC()
    {
        gunController.Shoot();
    }

    [PunRPC]
    private void ReloadRPC()
    {
        StartCoroutine(gunController.ReloadCoroutine());
    }

    [PunRPC]
    private void SetPrefabRPC(string _gameName)
    {
        DragSlot.instance.SetPrefab(_gameName);
    }
}
