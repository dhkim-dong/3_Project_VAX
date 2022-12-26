using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private GunController gunController;
    private Gun currentGun;

    [SerializeField] private GameObject go_BulletHUD;

    [SerializeField] private TextMeshProUGUI[] text_Bullet;

    // Update is called once per frame
    void Update()
    {
        if (!gunController.isActivate)
        {
            go_BulletHUD.SetActive(false);
        }
        else
        {
            go_BulletHUD.SetActive(true);
        }

        ChecktBullet();
    }

    private void ChecktBullet()
    {
        currentGun = gunController.GetGun();
        text_Bullet[0].text = currentGun.carryBulletCount.ToString();
        text_Bullet[1].text = currentGun.reloadBulletCount.ToString();
        text_Bullet[2].text = currentGun.currentBulletCount.ToString();
    }
}
