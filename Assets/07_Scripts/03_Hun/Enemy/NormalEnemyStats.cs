using Photon.Pun.Demo.Cockpit;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 좀비의 스텟 정보를 보유하고 있는 오브젝트
/// 최대 체력, 공격력, 방어력, 이속, 추적 거리, 추적 각도, 공격 거리, 공격 각도, 행동 간격
/// maxHp, atk, def, moveSpd, trackingRadius, trackingAngle, atkRadius, atkAngle, actionTime
/// 단, actionTime의 값이 0이하면 안된다!
/// </summary>
[CreateAssetMenu(fileName = "Zombie Data", menuName = "Scriptable Object/Zombie Data", order = int.MaxValue)]
public class NormalEnemyStats : ScriptableObject
{
    /// <summary>
    /// 최대 체력
    /// </summary>
    public int maxHp;

    /// <summary>
    /// 공격력
    /// </summary>
    public int atk;

    /// <summary>
    /// 방어력
    /// </summary>
    public int def;

    /// <summary>
    /// 이동 속도
    /// </summary>
    public float moveSpd;

    /// <summary>
    /// 추적 상태로 바꿀 거리
    /// </summary>
    public float trackingRadius;

    /// <summary>
    /// 추적 상태로 바꿀 각도
    /// </summary>
    public float trackingAngle;

    /// <summary>
    /// 공격 상태로 바꿀 거리
    /// </summary>
    public float atkRadius;

    /// <summary>
    /// 공격 상태로 바꿀 각도
    /// </summary>
    public float atkAngle;

    /// <summary>
    /// 상태를 확인할 간격
    /// </summary>
    public float actionTime;
}
