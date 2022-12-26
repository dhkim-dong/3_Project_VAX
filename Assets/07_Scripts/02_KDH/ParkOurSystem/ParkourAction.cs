using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/New parkour action")]
public class ParkourAction : ScriptableObject                        // 파쿠르 액션별 기능을 담고 있는 데이터 클래스
{
    [SerializeField] string animName;                                // 애니메이션 실행을 위해 가져올 이름 변수
    [SerializeField] string obstacleTag;                             // VaultAction 체크용 Tag이름

    [SerializeField] float minHeight;                                // 애니메이션 실행 조건 최소 높이
    [SerializeField] float maxHeight;                                // 애니메이션 실행 조건 최대 높이

    [Tooltip("파쿠르 액션 중 전방 오브젝트의 normal(반대 방향)으로 이동할 것인가?")] 
    [SerializeField] bool rotateToObstacle;                          // 파쿠르 액션 중 전방 오브젝트의 normal(반대 방향)으로 이동할 것인가?
    [SerializeField] float postActionDelay;                          // 2번째 애니메이션이 있을 경우 해당 애니메이션의 실행 시간

    [Header("Target Matching")]
    [SerializeField] bool enableTargetMatching = true;               // 타겟 매칭
    [SerializeField] protected AvatarTarget matchBodyPart;           // 애니메이션 동작과 매칭시킬 몸의 파츠
    [SerializeField] float matchStartTime;                           // 매칭할 애니메이션의 정규화한 시간( % 초)
    [SerializeField] float matchTargetTime;                          // 매칭 애니메이션의 위치가 끝나는 시간 ( % 초)
    [SerializeField] Vector3 matchPosWeight = new Vector3(0, 1, 0);  // 타겟 위치 가중치

    // 게터,세터
    public Quaternion TargetRotation { get; set; }

    public Vector3 MatchPos { get; set; }

    public bool Mirror { get; set; }

    public virtual bool CheckIfPossible(ObstalceHitData hitData, Transform player)    // 파쿠르 가능한 오브젝트인지 판별하는 메서드
    {
        // Tag Check
        if (!string.IsNullOrEmpty(obstacleTag) && hitData.forwardHit.transform.tag != obstacleTag) // 태그로 1차 탐색
            return false;

        // Heigth Check
        float height = hitData.heightHit.point.y - player.position.y;                             // 높이로 2차 탐색
        if (height < minHeight || height > maxHeight)
            return false;
                                                                                       // 파쿠르액션의 방향(Player의 Rot값)을 고정하기 위해 구현
        if (rotateToObstacle)                                                          // 해당 Bool을 Check하면 오브젝트의 -Normal 방향으로만 애니메이션 실행
            TargetRotation = Quaternion.LookRotation(-hitData.forwardHit.normal);

        if (enableTargetMatching)                                                      // 파쿠르 액션의 타겟매칭 기능을 위해 구현
            MatchPos = hitData.heightHit.point;

        return true;
    }

    public string AnimName => animName;

    public bool RotateToObstacle => rotateToObstacle;
    public float PostActionDelay => postActionDelay;

    public bool EnableTargetMatching => enableTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;
    public Vector3 MatchPosWeight => matchPosWeight;
}
