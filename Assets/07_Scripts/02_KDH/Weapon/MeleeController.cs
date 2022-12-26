using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeController : MonoBehaviourPunCallbacks
{
    protected PhotonView PV;

    [SerializeField] protected Melee currentHand;    // 가져올 근저 무기 정보

    [SerializeField] protected Transform attackPos;  // 공격 시작 지점

    [SerializeField] protected PlayerController playerController; // 플레이어 애니메이션

    [SerializeField] protected WeaponManager weaponManager;

    [SerializeField] protected GunController gunController;

    protected bool isAttack = false;                //  공격 시전 ~ 끝
    protected bool isSwing = false;                 //  공격 적용 시작

    protected RaycastHit hitInfo;                   //   대상 정보 저장 변수

    protected void TryAttack()                      // 공격 시도
    {
        if (!PV.IsMine) return;

        if (Inventory.inventoryActivated || !playerController.HasControl)           // 공격과 동시에 발생하는 것을 막기
            return;

        if (Input.GetButton("Fire1"))               // 공격 버튼
        {
            if (!isAttack)
            {
                PV.RPC("MeleeAttack", RpcTarget.All);
            }
        }
    }

    public virtual IEnumerator AttackCoroutine()          // 공격 코루틴
    {
        isAttack = true;
        weaponManager.currentWeaponAnim.CrossFade("Attack", 0.1f);

        yield return new WaitForSeconds(currentHand.attackDelayA);      // 데미지 입기 전 까지의 시간
        isSwing = true;

        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentHand.attackDelayB);      // 데미지 적용 시간
        isSwing = false;

        yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB);  // 전체 공격 딜레이

        isAttack = false;
    }

    protected abstract IEnumerator HitCoroutine();                       // 자식 클래스에서 완성

    protected bool CheckObject()                                         // 공격 대상 정보 체크용 메서드
    {
        if (Physics.Raycast(attackPos.position, attackPos.forward, out hitInfo, currentHand.range)) // 현재 전체 Layer판별하고 있으므로 Monster Layer 추가 가능
        {
            return true;
        }
        else
            return false;
    }


    public virtual void MeleeChange(Melee _Melee) // 
    {

        if (weaponManager.currentWeapon != null)
            weaponManager.currentWeapon.gameObject.SetActive(false);         // 현재 무기를 비활성화

        currentHand = _Melee; // 선언된 Melee클래스 데이터를 가져온다.
        weaponManager.currentWeapon = currentHand.GetComponent<Transform>(); // Transform에서 Script를 가져오기 위해 선언
        weaponManager.currentWeaponAnim = currentHand.anim;                  

        //currentHand.transform.localPosition = Vector3.zero; 아이템 바꿀 때 마다 위치 변경을 위해 선언되었지만, 현재 zero값으로 하면 이상해서 막아 놓음
        currentHand.gameObject.SetActive(true);
        playerController.anim.runtimeAnimatorController = gameObject.GetComponentInChildren<Animator>().runtimeAnimatorController; // 플레이어의 애니메이터를 변경시켜 무기별 애니메이션 적용
        gunController.isActivate = false; // 총 일때의 행동을 막기 위해 false로
    } 
}
