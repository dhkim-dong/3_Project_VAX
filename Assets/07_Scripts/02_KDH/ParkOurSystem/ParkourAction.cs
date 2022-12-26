using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/New parkour action")]
public class ParkourAction : ScriptableObject                        // ���� �׼Ǻ� ����� ��� �ִ� ������ Ŭ����
{
    [SerializeField] string animName;                                // �ִϸ��̼� ������ ���� ������ �̸� ����
    [SerializeField] string obstacleTag;                             // VaultAction üũ�� Tag�̸�

    [SerializeField] float minHeight;                                // �ִϸ��̼� ���� ���� �ּ� ����
    [SerializeField] float maxHeight;                                // �ִϸ��̼� ���� ���� �ִ� ����

    [Tooltip("���� �׼� �� ���� ������Ʈ�� normal(�ݴ� ����)���� �̵��� ���ΰ�?")] 
    [SerializeField] bool rotateToObstacle;                          // ���� �׼� �� ���� ������Ʈ�� normal(�ݴ� ����)���� �̵��� ���ΰ�?
    [SerializeField] float postActionDelay;                          // 2��° �ִϸ��̼��� ���� ��� �ش� �ִϸ��̼��� ���� �ð�

    [Header("Target Matching")]
    [SerializeField] bool enableTargetMatching = true;               // Ÿ�� ��Ī
    [SerializeField] protected AvatarTarget matchBodyPart;           // �ִϸ��̼� ���۰� ��Ī��ų ���� ����
    [SerializeField] float matchStartTime;                           // ��Ī�� �ִϸ��̼��� ����ȭ�� �ð�( % ��)
    [SerializeField] float matchTargetTime;                          // ��Ī �ִϸ��̼��� ��ġ�� ������ �ð� ( % ��)
    [SerializeField] Vector3 matchPosWeight = new Vector3(0, 1, 0);  // Ÿ�� ��ġ ����ġ

    // ����,����
    public Quaternion TargetRotation { get; set; }

    public Vector3 MatchPos { get; set; }

    public bool Mirror { get; set; }

    public virtual bool CheckIfPossible(ObstalceHitData hitData, Transform player)    // ���� ������ ������Ʈ���� �Ǻ��ϴ� �޼���
    {
        // Tag Check
        if (!string.IsNullOrEmpty(obstacleTag) && hitData.forwardHit.transform.tag != obstacleTag) // �±׷� 1�� Ž��
            return false;

        // Heigth Check
        float height = hitData.heightHit.point.y - player.position.y;                             // ���̷� 2�� Ž��
        if (height < minHeight || height > maxHeight)
            return false;
                                                                                       // �����׼��� ����(Player�� Rot��)�� �����ϱ� ���� ����
        if (rotateToObstacle)                                                          // �ش� Bool�� Check�ϸ� ������Ʈ�� -Normal �������θ� �ִϸ��̼� ����
            TargetRotation = Quaternion.LookRotation(-hitData.forwardHit.normal);

        if (enableTargetMatching)                                                      // ���� �׼��� Ÿ�ٸ�Ī ����� ���� ����
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
