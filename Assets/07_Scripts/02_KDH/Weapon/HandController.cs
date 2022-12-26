using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HandController : MeleeController
{
	// ���Ӿ� ���� ���� UI ���� �Ŵ���
    private GameNetworkUIManager gameNetworkUIManager;
	
    public bool isActivate = true;

    private AudioSource audioSource;

    [SerializeField] Vector3 attackOffset;

    [SerializeField] protected KeyController keyController;

    private void Start()
    {
        PV = GetComponentInParent<PhotonView>();

		// ������ GameNetworkUIManager ������Ʈ�� ã�� �Ҵ�
        gameNetworkUIManager = FindObjectOfType<GameNetworkUIManager>();
		
        audioSource = GetComponent<AudioSource>();

        weaponManager.currentWeapon = currentHand.GetComponent<Transform>();
        weaponManager.currentWeaponAnim = currentHand.anim;
    }
    void Update()
    {
        if (!PV.IsMine) return;
        // ���Ӿ��� UIâ ��ü�� ��Ȱ��ȭ�Ǿ�߸� ��Ƽ�� ��� ����
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
        //GunController.isActivate = false;          base.MeleeChanage ���� �����Ǿ� ����
        isActivate = true;
    }

    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }  
}
