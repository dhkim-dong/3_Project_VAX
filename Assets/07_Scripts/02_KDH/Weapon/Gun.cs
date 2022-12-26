using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public string gunName;                       // 총 이름
    public float range;                          // 총의 공격사거리
    public float accuracy;                       // 정확도
    public float fireRate;                       // 공격속도
    public float reloadTime;                     // 장전 시간

    public int damage;                           // 무기의 공격력
    public int reloadBulletCount;                // 탄창 수
    public int currentBulletCount;               // 현재 장전 중인 총알 수
    public int maxBulletCount;                   // 들고 다니는 총알의 최대 갯수 제한 .. 현재 사용 안함
    public int carryBulletCount;                 // 현재 소지한 총알의 수

    public float retroActionForce;               // 총의 반동력 
    public float retroActionFineShightForce;     // 총의 조준시 반동력

    public Vector3 fineSightOriginPos;           // 정조준 위치

    public ParticleSystem muzzleFlash;           // 총구 이팩트

    public AudioClip fire_Sound;                 // 총 사운드

    public Animator anim;                        // 해당 총 애니메이션
}
