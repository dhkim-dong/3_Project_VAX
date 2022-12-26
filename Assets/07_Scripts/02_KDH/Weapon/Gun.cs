using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public string gunName;                       // �� �̸�
    public float range;                          // ���� ���ݻ�Ÿ�
    public float accuracy;                       // ��Ȯ��
    public float fireRate;                       // ���ݼӵ�
    public float reloadTime;                     // ���� �ð�

    public int damage;                           // ������ ���ݷ�
    public int reloadBulletCount;                // źâ ��
    public int currentBulletCount;               // ���� ���� ���� �Ѿ� ��
    public int maxBulletCount;                   // ��� �ٴϴ� �Ѿ��� �ִ� ���� ���� .. ���� ��� ����
    public int carryBulletCount;                 // ���� ������ �Ѿ��� ��

    public float retroActionForce;               // ���� �ݵ��� 
    public float retroActionFineShightForce;     // ���� ���ؽ� �ݵ���

    public Vector3 fineSightOriginPos;           // ������ ��ġ

    public ParticleSystem muzzleFlash;           // �ѱ� ����Ʈ

    public AudioClip fire_Sound;                 // �� ����

    public Animator anim;                        // �ش� �� �ִϸ��̼�
}
