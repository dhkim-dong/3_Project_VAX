using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour
{
    public string meleeName;   // ����� ���������� �̸�,  �ϵ��ڵ��� ���� ����

    public float range;        // ������ ���� ��Ÿ�
    public int damage;         // ������ ���ݷ�
    public float attackDelay;  // ���� ������
    public float attackDelayA; // ���� Ȱ��ȭ ����
    public float attackDelayB; // ���� ��Ȱ��ȭ ����

    public AudioClip melee_Sound;

    public Animator anim;
}
