using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviourPunCallbacks        // 파쿠르 액션을 할 수 있는 오브젝트있는지 판별하는 스크립트
{
    [SerializeField] private PhotonView PV;

    [SerializeField] Vector3 forwardRayOffset = new Vector3(0, 0.25f, 0);  // 오브젝트 체크 시작 위치 조정
    [SerializeField] float forwardRayLength = 0.8f;                       // 전방 오브젝트 길이 체크
    [SerializeField] float heightRayLength = 5f;                          // 오브젝트 높이 길이 체크
    [SerializeField] float ledgeRayLength = 5f;                           // 바닥 높이 체크
    [SerializeField] LayerMask obstacleLayer;                             // 파쿠르 대상 레이어 체크
    [SerializeField] LayerMask groundLayer;                             //  땅 레이어 체크
    [SerializeField] float ledgeHeightThreshold = 0.75f;                  // 해당 길이보다 판별 길이가 작으면 낙하 애니메이션 없음

    private void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    public ObstalceHitData ObstacleCheck()                 // 전방 오브젝트를 체크 후 struct 데이터로 전달해주는 메소드
    {
        var hitData = new ObstalceHitData();               // 파쿠르 액션을 위해 필요한 (정면,상단에서 아래)RayHit정보와 불값 데이터

        var forwardOrigin = transform.position + forwardRayOffset;

        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward, out hitData.forwardHit
            , forwardRayLength, obstacleLayer);

        Debug.DrawRay(forwardOrigin, transform.forward * forwardRayLength, (hitData.forwardHitFound) ? Color.red : Color.white); // 충돌하면 editor에서 붉게

        if (hitData.forwardHitFound) // 정면에 물체가 있다면 해당 Point위에서 아래로 쏴 높이 체크
        {
            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLength;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down,
                  out hitData.heightHit, heightRayLength, obstacleLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength, (hitData.heightHitFound) ? Color.red : Color.white);
        }

        return hitData;
    }

    public bool LedgeCheck(Vector3 moveDir, out LedgeData ledgeData)    // 플레이어 전방 방향 아래를 체크하여 낙하지점(Ledge) 체크
    {
        ledgeData = new LedgeData();

        if (moveDir == Vector3.zero)
            return false;

        float originOffset = 0.5f;
        var origin = transform.position + moveDir * originOffset + Vector3.up;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, ledgeRayLength, groundLayer))
        {
            Debug.DrawRay(origin, Vector3.down * ledgeRayLength, Color.red);

            var surfaceRayOrigin = transform.position + moveDir - new Vector3(0, 0.1f, 0); // 현재 위치에서 이동방향으로 이동한 위치보다 살짝 아래의Y값
            if (Physics.Raycast(surfaceRayOrigin, -moveDir, out RaycastHit surfaceHit, 2, obstacleLayer)) // 이동방향의 반대방향으로 쏴서 Hit한 오브젝트 감별
            {
                float height = transform.position.y - hit.point.y;

                if (height > ledgeHeightThreshold)        // 낙하애니메이션이 필요한 최소 길이보다 높이가 크다면 해당 데이터를 ledge구조체에 저장
                {
                    ledgeData.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
                    ledgeData.height = height;
                    ledgeData.surfaceHit = surfaceHit;

                    return true;
                }
            }


        }

        return false;
    }
}

public struct ObstalceHitData
{
    public bool forwardHitFound;
    public bool heightHitFound;
    public RaycastHit forwardHit;
    public RaycastHit heightHit;
}

public struct LedgeData
{
    public float height;
    public float angle;
    public RaycastHit surfaceHit;
}

