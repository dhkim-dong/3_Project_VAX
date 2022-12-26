using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class KeyController : MeleeController            // 근접 무기 Controller를 상속받아 구현
{
    public bool isActivate = false;              // GUN, HAND, KEY 상태일 때 현재 사용중인 isActivate만 true로 작동시킴

    private AudioSource audioSource;

    [SerializeField] private HandController handController;

    private void Start()
    {
        PV = GetComponentInParent<PhotonView>();

        audioSource = GetComponent<AudioSource>();

        weaponManager.currentWeapon = currentHand.GetComponent<Transform>();
        weaponManager.currentWeaponAnim = currentHand.anim;
    }
    void Update()
    {

        if (!PV.IsMine) return;

        if (isActivate)
            TryAttack();        // MeleeController에 내장된 TryAttack 사용
    }

    public override IEnumerator AttackCoroutine()    // 공격 감지 -> 공격 -> 피격 형식으로 구현 각각의 기능을 분할하여 구현
    {
        if (CheckObject())                              // 공격이 감지(true)상태일때 공격한다.(키를 사용한다)
        {
            if (hitInfo.transform.GetComponent<BigDoorOpen>())      // 대상에 문이 있을 경우에만 공격을 실행
            {

                isAttack = true;                                                // 공격 전체를 감지하는 불 = isAttack
                weaponManager.currentWeaponAnim.CrossFade("Attack", 0.1f);      // 애니메이터에서 실행

                yield return new WaitForSeconds(currentHand.attackDelayA);      // 데미지 입기 전 까지의 시간
                isSwing = true;                                                 // 공격 피격을 감지하는 불 = isSwing

                StartCoroutine(HitCoroutine());                                 // isSwing = true일때만 피격할 수 있도록

                yield return new WaitForSeconds(currentHand.attackDelayB);      // 데미지 적용 시간
                isSwing = false;

                yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB);  // 전체 공격 딜레이

                isAttack = false;
            }
        }
    }

    protected override IEnumerator HitCoroutine()
    {
        PlaySE(currentHand.melee_Sound);

        while (isSwing)
        {
            if (CheckObject())
            {
                if (!hitInfo.transform.GetComponent<BigDoorOpen>())
                   yield return null;
                else
                {
                    // 해당 하는 문인지 체크하는 로직 구현 필요
                    if (hitInfo.transform.GetComponent<BigDoorOpen>().IsNeedKey)           // NeedKey인 문에만 접근할 수 있음
                    {
                        hitInfo.transform.GetComponent<BigDoorOpen>().CheckHasKey();       // 문의 상태를 Lock-> Close로 변환
                        isSwing = false;                                                   // 코루틴에서 벗어남
                    }
                }
            }
            yield return null;
        }
    }

    public override void MeleeChange(Melee _melee)      // 현재 무기로 바꾸기 위한 메서드
    {
        base.MeleeChange(_melee);
        handController.isActivate = false;              // 다른 무기는 비활성화
        //GunController.isActivate = false;  base.MeleeChanage 에서 구현되어 있음
        isActivate = true;
    }

    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }
}
