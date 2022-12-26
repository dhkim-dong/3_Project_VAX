using Photon.Pun;
using UnityEngine;

/// <summary>
/// �Ϲ� ���� �ִϸ����� ����
/// </summary>
public class NormalEnemyAnimator : MonoBehaviourPunCallbacks
{
    #region Variable

    //private PhotonView PV;

    /// <summary>
    /// Proto Type - Idle, Move, Attack, Damaged, Dead ���� �ۿ� ����.
    /// �ִϸ����� params�δ� walkBool, atkBool ������ ���� 
    /// ���Ŀ� ���� ���¶� �ٸ� �ൿ�� ���̱� ���� int�� ������ ����
    /// 0�� false, ����� true
    /// </summary>
    [SerializeField][Tooltip("�ִϸ�����")] private Animator anim;

    /// <summary>
    /// ���, ����, ����, �ǰ�, ��� �ִϸ��̼� ����
    /// ����� 4, 8, 8, 5, 2 �� �ִ�
    /// </summary>
    public static int[] maxAnimNum = { 4, 8, 8, 5, 2 };

    /// <summary>
    /// �ִϸ����� �ѹ�
    /// ���, ����, ����, �ǰ�, ��� ��ȣ�̴�
    /// </summary>
    [SerializeField]
    [Tooltip("�ִϸ��̼� �ѹ�")]
    private int[] animNum = new int[5]; // �ִϸ����� �ѹ�

    #endregion Variable


    #region Unity �޼ҵ�
    private void Awake()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>(); // �ִϸ����� �޾ƿ���
        }

        // ����� ������Ʈ �Ҵ�
        //PV = GetComponent<PhotonView>();

        SetAnimNum(); // �ִϸ��̼� ����
    }
    #endregion

    #region ���� ����
    /// <summary>
    /// �ִϸ��̼� ��ȣ ���
    /// </summary>
    private void SetAnimNum() // �ִϸ��̼� ����
    {
        for (int i = 0; i < animNum.Length; i++) // �ִϸ��̼� ��ȣ ���� ����
        {
            animNum[i] = Random.Range(0, maxAnimNum[i]);
        }

        // �ִϸ��̼� �ѹ� ����
        anim.SetInteger("idleInt", animNum[0]); // ��� ��ȣ
        anim.SetInteger("trackingInt", animNum[1]); // ���� ��ȣ
        anim.SetInteger("atkInt", animNum[2]); // ���� ��ȣ
        anim.SetInteger("dmgInt", animNum[3]); // �ǰ� ��ȣ
        anim.SetInteger("deadInt", animNum[4]); // ��� ��ȣ
    }

    #endregion


    #region FSM���� �� �޼ҵ�
    /// <summary>
    /// Idle ���¿��� Move ���·� ���� �� ������ �ִϸ��̼� �޼ҵ�
    /// </summary>
    public void IdleToMove()
    {
        anim.SetBool("walkBool", true); 
    }

    /// <summary>
    /// Move ���¿��� Idle ���·� ���� �� ������ �ִϸ��̼� �޼ҵ�
    /// </summary>
    public void MoveToIdle()
    {
        anim.SetBool("atkBool", false);
        anim.SetBool("walkBool", false); 
    }

    /// <summary>
    /// Move ���¿��� Attack ���·� ���� �� ������ �ִϸ��̼� �޼ҵ�
    /// </summary>
    public void MoveToAttack()
    {
        anim.SetBool("atkBool", true);
    }

    /// <summary>
    /// Attack ���¿��� Move ���·� ���� �� ������ �ִϸ��̼� �޼ҵ�
    /// </summary>
    public void AttackToMove()
    {
        anim.SetBool("atkBool", false);
    }

    /// <summary>
    /// Damaged ���¿��� ������ �ִϸ��̼� �޼ҵ�
    /// </summary>
    public void SetDamaged()
    {
        anim.SetTrigger("dmgTrigger");
    }

    /// <summary>
    /// Dead ���¿��� ������ �ִϸ��̼� �޼ҵ�
    /// </summary>
    public void SetDead()
    {
        anim.SetTrigger("dieTrigger");
    }
    #endregion
}
