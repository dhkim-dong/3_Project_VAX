using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeController : MonoBehaviourPunCallbacks
{
    protected PhotonView PV;

    [SerializeField] protected Melee currentHand;    // ������ ���� ���� ����

    [SerializeField] protected Transform attackPos;  // ���� ���� ����

    [SerializeField] protected PlayerController playerController; // �÷��̾� �ִϸ��̼�

    [SerializeField] protected WeaponManager weaponManager;

    [SerializeField] protected GunController gunController;

    protected bool isAttack = false;                //  ���� ���� ~ ��
    protected bool isSwing = false;                 //  ���� ���� ����

    protected RaycastHit hitInfo;                   //   ��� ���� ���� ����

    protected void TryAttack()                      // ���� �õ�
    {
        if (!PV.IsMine) return;

        if (Inventory.inventoryActivated || !playerController.HasControl)           // ���ݰ� ���ÿ� �߻��ϴ� ���� ����
            return;

        if (Input.GetButton("Fire1"))               // ���� ��ư
        {
            if (!isAttack)
            {
                PV.RPC("MeleeAttack", RpcTarget.All);
            }
        }
    }

    public virtual IEnumerator AttackCoroutine()          // ���� �ڷ�ƾ
    {
        isAttack = true;
        weaponManager.currentWeaponAnim.CrossFade("Attack", 0.1f);

        yield return new WaitForSeconds(currentHand.attackDelayA);      // ������ �Ա� �� ������ �ð�
        isSwing = true;

        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentHand.attackDelayB);      // ������ ���� �ð�
        isSwing = false;

        yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB);  // ��ü ���� ������

        isAttack = false;
    }

    protected abstract IEnumerator HitCoroutine();                       // �ڽ� Ŭ�������� �ϼ�

    protected bool CheckObject()                                         // ���� ��� ���� üũ�� �޼���
    {
        if (Physics.Raycast(attackPos.position, attackPos.forward, out hitInfo, currentHand.range)) // ���� ��ü Layer�Ǻ��ϰ� �����Ƿ� Monster Layer �߰� ����
        {
            return true;
        }
        else
            return false;
    }


    public virtual void MeleeChange(Melee _Melee) // 
    {

        if (weaponManager.currentWeapon != null)
            weaponManager.currentWeapon.gameObject.SetActive(false);         // ���� ���⸦ ��Ȱ��ȭ

        currentHand = _Melee; // ����� MeleeŬ���� �����͸� �����´�.
        weaponManager.currentWeapon = currentHand.GetComponent<Transform>(); // Transform���� Script�� �������� ���� ����
        weaponManager.currentWeaponAnim = currentHand.anim;                  

        //currentHand.transform.localPosition = Vector3.zero; ������ �ٲ� �� ���� ��ġ ������ ���� ����Ǿ�����, ���� zero������ �ϸ� �̻��ؼ� ���� ����
        currentHand.gameObject.SetActive(true);
        playerController.anim.runtimeAnimatorController = gameObject.GetComponentInChildren<Animator>().runtimeAnimatorController; // �÷��̾��� �ִϸ����͸� ������� ���⺰ �ִϸ��̼� ����
        gunController.isActivate = false; // �� �϶��� �ൿ�� ���� ���� false��
    } 
}
