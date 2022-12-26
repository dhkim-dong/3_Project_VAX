using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/Custom ACtions / New vault action")]
public class VaultAction : ParkourAction           // Ÿ ���� �׼ǿ� �߰� ����� �ֱ� ���� ParkourAction�� ��ӹ޾� �߰� ����� ������
{
    public override bool CheckIfPossible(ObstalceHitData hitData, Transform player)
    {
        if (!base.CheckIfPossible(hitData, player))
            return false;

        var hitPoint = hitData.forwardHit.transform.InverseTransformPoint(hitData.forwardHit.point);

        if (hitPoint.z < 0 && hitPoint.x < 0 || hitPoint.z > 0 && hitPoint.x > 0) // z Ȯ���ϴ� ������ �յ� local�� ���� ������ �ٲ�� ����
        {
            // ���� Mirror ���
            Mirror = true;
            matchBodyPart = AvatarTarget.RightHand;
        }
        else
        {
            // ������ Don't Mirror
            Mirror = false;
            matchBodyPart = AvatarTarget.LeftHand;
        }

        return true;
    }
}
