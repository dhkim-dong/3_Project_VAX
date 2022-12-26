using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour
{
    public string meleeName;   // 사용할 근접무기의 이름,  하드코딩에 사용될 변수

    public float range;        // 무기의 공격 사거리
    public int damage;         // 무기의 공격력
    public float attackDelay;  // 공격 딜레이
    public float attackDelayA; // 공격 활성화 시점
    public float attackDelayB; // 공격 비활성화 시점

    public AudioClip melee_Sound;

    public Animator anim;
}
