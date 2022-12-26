using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HandController : MeleeController
{
	// 게임씬 포톤 서버 UI 연동 매니저
    private GameNetworkUIManager gameNetworkUIManager;
	
    public bool isActivate = true;

    private AudioSource audioSource;

    [SerializeField] Vector3 attackOffset;

    [SerializeField] protected KeyController keyController;

    private void Start()
    {
        PV = GetComponentInParent<PhotonView>();

		// 씬에서 GameNetworkUIManager 오브젝트를 찾아 할당
        gameNetworkUIManager = FindObjectOfType<GameNetworkUIManager>();
		
        audioSource = GetComponent<AudioSource>();

        weaponManager.currentWeapon = currentHand.GetComponent<Transform>();
        weaponManager.currentWeaponAnim = currentHand.anim;
    }
    void Update()
    {
        if (!PV.IsMine) return;
        // 게임씬의 UI창 전체가 비활성화되어야만 액티브 기능 실행
        if (isActivate && !gameNetworkUIManager.isUIActive)
            TryAttack();

        Debug.DrawRay(attackPos.position + attackOffset, attackPos.forward * currentHand.range, Color.red);
    }

    protected override IEnumerator HitCoroutine()
    {
        PlaySE(currentHand.melee_Sound);

        while (isSwing)
        {
            if (CheckObject())
            {
                if (hitInfo.transform.GetComponent<BossEnemy>())
                {
                    hitInfo.transform.GetComponent<BossEnemy>().OnDamagedProcess(currentHand.damage);
                    isSwing = false;
                }
                Debug.Log(hitInfo.transform.name);
            }
            yield return null;
        }
    }

    public override void MeleeChange(Melee _melee)
    {
        base.MeleeChange(_melee);
        keyController.isActivate = false;
        //GunController.isActivate = false;          base.MeleeChanage 에서 구현되어 있음
        isActivate = true;
    }

    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }  
}
