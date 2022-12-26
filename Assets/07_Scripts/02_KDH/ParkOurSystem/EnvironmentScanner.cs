using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviourPunCallbacks        // ���� �׼��� �� �� �ִ� ������Ʈ�ִ��� �Ǻ��ϴ� ��ũ��Ʈ
{
    [SerializeField] private PhotonView PV;

    [SerializeField] Vector3 forwardRayOffset = new Vector3(0, 0.25f, 0);  // ������Ʈ üũ ���� ��ġ ����
    [SerializeField] float forwardRayLength = 0.8f;                       // ���� ������Ʈ ���� üũ
    [SerializeField] float heightRayLength = 5f;                          // ������Ʈ ���� ���� üũ
    [SerializeField] float ledgeRayLength = 5f;                           // �ٴ� ���� üũ
    [SerializeField] LayerMask obstacleLayer;                             // ���� ��� ���̾� üũ
    [SerializeField] LayerMask groundLayer;                             //  �� ���̾� üũ
    [SerializeField] float ledgeHeightThreshold = 0.75f;                  // �ش� ���̺��� �Ǻ� ���̰� ������ ���� �ִϸ��̼� ����

    private void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    public ObstalceHitData ObstacleCheck()                 // ���� ������Ʈ�� üũ �� struct �����ͷ� �������ִ� �޼ҵ�
    {
        var hitData = new ObstalceHitData();               // ���� �׼��� ���� �ʿ��� (����,��ܿ��� �Ʒ�)RayHit������ �Ұ� ������

        var forwardOrigin = transform.position + forwardRayOffset;

        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward, out hitData.forwardHit
            , forwardRayLength, obstacleLayer);

        Debug.DrawRay(forwardOrigin, transform.forward * forwardRayLength, (hitData.forwardHitFound) ? Color.red : Color.white); // �浹�ϸ� editor���� �Ӱ�

        if (hitData.forwardHitFound) // ���鿡 ��ü�� �ִٸ� �ش� Point������ �Ʒ��� �� ���� üũ
        {
            var heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLength;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down,
                  out hitData.heightHit, heightRayLength, obstacleLayer);

            Debug.DrawRay(heightOrigin, Vector3.down * heightRayLength, (hitData.heightHitFound) ? Color.red : Color.white);
        }

        return hitData;
    }

    public bool LedgeCheck(Vector3 moveDir, out LedgeData ledgeData)    // �÷��̾� ���� ���� �Ʒ��� üũ�Ͽ� ��������(Ledge) üũ
    {
        ledgeData = new LedgeData();

        if (moveDir == Vector3.zero)
            return false;

        float originOffset = 0.5f;
        var origin = transform.position + moveDir * originOffset + Vector3.up;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, ledgeRayLength, groundLayer))
        {
            Debug.DrawRay(origin, Vector3.down * ledgeRayLength, Color.red);

            var surfaceRayOrigin = transform.position + moveDir - new Vector3(0, 0.1f, 0); // ���� ��ġ���� �̵��������� �̵��� ��ġ���� ��¦ �Ʒ���Y��
            if (Physics.Raycast(surfaceRayOrigin, -moveDir, out RaycastHit surfaceHit, 2, obstacleLayer)) // �̵������� �ݴ�������� ���� Hit�� ������Ʈ ����
            {
                float height = transform.position.y - hit.point.y;

                if (height > ledgeHeightThreshold)        // ���Ͼִϸ��̼��� �ʿ��� �ּ� ���̺��� ���̰� ũ�ٸ� �ش� �����͸� ledge����ü�� ����
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

