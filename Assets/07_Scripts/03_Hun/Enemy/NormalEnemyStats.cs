using Photon.Pun.Demo.Cockpit;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������ ���� ������ �����ϰ� �ִ� ������Ʈ
/// �ִ� ü��, ���ݷ�, ����, �̼�, ���� �Ÿ�, ���� ����, ���� �Ÿ�, ���� ����, �ൿ ����
/// maxHp, atk, def, moveSpd, trackingRadius, trackingAngle, atkRadius, atkAngle, actionTime
/// ��, actionTime�� ���� 0���ϸ� �ȵȴ�!
/// </summary>
[CreateAssetMenu(fileName = "Zombie Data", menuName = "Scriptable Object/Zombie Data", order = int.MaxValue)]
public class NormalEnemyStats : ScriptableObject
{
    /// <summary>
    /// �ִ� ü��
    /// </summary>
    public int maxHp;

    /// <summary>
    /// ���ݷ�
    /// </summary>
    public int atk;

    /// <summary>
    /// ����
    /// </summary>
    public int def;

    /// <summary>
    /// �̵� �ӵ�
    /// </summary>
    public float moveSpd;

    /// <summary>
    /// ���� ���·� �ٲ� �Ÿ�
    /// </summary>
    public float trackingRadius;

    /// <summary>
    /// ���� ���·� �ٲ� ����
    /// </summary>
    public float trackingAngle;

    /// <summary>
    /// ���� ���·� �ٲ� �Ÿ�
    /// </summary>
    public float atkRadius;

    /// <summary>
    /// ���� ���·� �ٲ� ����
    /// </summary>
    public float atkAngle;

    /// <summary>
    /// ���¸� Ȯ���� ����
    /// </summary>
    public float actionTime;
}
