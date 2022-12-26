using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class KeyController : MeleeController            // ���� ���� Controller�� ��ӹ޾� ����
{
    public bool isActivate = false;              // GUN, HAND, KEY ������ �� ���� ������� isActivate�� true�� �۵���Ŵ

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
            TryAttack();        // MeleeController�� ����� TryAttack ���
    }

    public override IEnumerator AttackCoroutine()    // ���� ���� -> ���� -> �ǰ� �������� ���� ������ ����� �����Ͽ� ����
    {
        if (CheckObject())                              // ������ ����(true)�����϶� �����Ѵ�.(Ű�� ����Ѵ�)
        {
            if (hitInfo.transform.GetComponent<BigDoorOpen>())      // ��� ���� ���� ��쿡�� ������ ����
            {

                isAttack = true;                                                // ���� ��ü�� �����ϴ� �� = isAttack
                weaponManager.currentWeaponAnim.CrossFade("Attack", 0.1f);      // �ִϸ����Ϳ��� ����

                yield return new WaitForSeconds(currentHand.attackDelayA);      // ������ �Ա� �� ������ �ð�
                isSwing = true;                                                 // ���� �ǰ��� �����ϴ� �� = isSwing

                StartCoroutine(HitCoroutine());                                 // isSwing = true�϶��� �ǰ��� �� �ֵ���

                yield return new WaitForSeconds(currentHand.attackDelayB);      // ������ ���� �ð�
                isSwing = false;

                yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB);  // ��ü ���� ������

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
                    // �ش� �ϴ� ������ üũ�ϴ� ���� ���� �ʿ�
                    if (hitInfo.transform.GetComponent<BigDoorOpen>().IsNeedKey)           // NeedKey�� ������ ������ �� ����
                    {
                        hitInfo.transform.GetComponent<BigDoorOpen>().CheckHasKey();       // ���� ���¸� Lock-> Close�� ��ȯ
                        isSwing = false;                                                   // �ڷ�ƾ���� ���
                    }
                }
            }
            yield return null;
        }
    }

    public override void MeleeChange(Melee _melee)      // ���� ����� �ٲٱ� ���� �޼���
    {
        base.MeleeChange(_melee);
        handController.isActivate = false;              // �ٸ� ����� ��Ȱ��ȭ
        //GunController.isActivate = false;  base.MeleeChanage ���� �����Ǿ� ����
        isActivate = true;
    }

    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }
}
