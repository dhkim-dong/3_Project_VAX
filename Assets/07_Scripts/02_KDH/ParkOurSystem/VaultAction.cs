using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/Custom ACtions / New vault action")]
public class VaultAction : ParkourAction           // 타 파쿠르 액션에 추가 기능을 넣기 위해 ParkourAction을 상속받아 추가 기능을 구현함
{
    public override bool CheckIfPossible(ObstalceHitData hitData, Transform player)
    {
        if (!base.CheckIfPossible(hitData, player))
            return false;

        var hitPoint = hitData.forwardHit.transform.InverseTransformPoint(hitData.forwardHit.point);

        if (hitPoint.z < 0 && hitPoint.x < 0 || hitPoint.z > 0 && hitPoint.x > 0) // z 확인하는 이유는 앞뒤 local에 따라 방향이 바뀌기 때문
        {
            // 왼쪽 Mirror 출력
            Mirror = true;
            matchBodyPart = AvatarTarget.RightHand;
        }
        else
        {
            // 오른쪽 Don't Mirror
            Mirror = false;
            matchBodyPart = AvatarTarget.LeftHand;
        }

        return true;
    }
}
