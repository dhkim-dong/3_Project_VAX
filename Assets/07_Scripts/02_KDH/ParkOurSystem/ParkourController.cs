using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviourPunCallbacks                        // �÷��̾ ��Ʈ�ѷ� ��ũ��Ʈ�� �־��ּ���
{
    [SerializeField] private PhotonView PV;

    [SerializeField] List<ParkourAction> parkourActions;              // ��� �� �����׼� ����Ʈ
    [SerializeField] ParkourAction jumpDownAction;                    // �ϰ� ���� �ִϸ��̼�
    [SerializeField] float autoDropHeight = 1f;                       // �Է����� �ʾƵ� �ڵ����� �������� ����

    bool inAction; // �ൿ �ľ� ����

    // �ش� Ŭ������ ����ϱ� ���ؼ��� ���� ���ӿ�����Ʈ�� ������Ʈ�� �־�� �Ѵ�.
    EnvironmentScanner environmentScanner;                    
    Animator animator;
    PlayerController playerController;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (!PV.IsMine) return;

        var hitData = environmentScanner.ObstacleCheck(); // ���� ������ ������Ʈ���� Ž���� Struct�����͸� �ҷ���

        if (Input.GetButton("Jump") && !inAction && playerController.IsGrounded) // ���� �ൿ�� ���� �������
        {
            if (hitData.forwardHitFound)
            {
                foreach (var action in parkourActions)            // ��Ʈ�ѷ��� �ִ� ���� Data�߿� 
                {
                    if (action.CheckIfPossible(hitData, transform)) // ���ǿ� �ش��ϴ� ������ ã�� �����϶�
                    {
                        StartCoroutine(DoParkourAction(action));    // ���� �߰��� ������ ���� �����ϱ� ������ List�� ���� ��ġ�� �߿��ϴ�.
                        break;                                      // ������� VaultAction�� JumpUp������ ���ѱ��̰� ������ Tag������ VaultAction�� List �� ��(��)�� �־���Ѵ�.
                    }
                }
            }
        }

        if (playerController.IsOnLedge && !inAction && !hitData.forwardHitFound)       // Ledge���¿����� �׼� �̺�Ʈ
        {
            bool shouldJump = true;
            if (playerController.LedgeData.height > autoDropHeight && !Input.GetButton("Jump")) // ������ �ȴ����� ������ �������� �ʴٰ�
                shouldJump = false;

            if (shouldJump && playerController.LedgeData.angle <= 50)            // ����Ű�� ������ �ϰ� �׼��� �����Ѵ�.
            {
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAction(jumpDownAction));
            }
        }
    }

    IEnumerator DoParkourAction(ParkourAction action)
    {
        inAction = true;
        playerController.SetControl(false);

        animator.SetBool("mirrorAction", action.Mirror);  // ���� ����� �¿� �߾Ӱ� ���� ġ������ ��ġ�� ����Ͽ� �ִϸ��̼��� �̷��� �����Ų��.
        animator.CrossFade(action.AnimName, 0.2f); // �ִ� ���̼� �ϷḦ �˸��� ���? => ���� �ִϸ��̼� ���� �ð��� Ȯ���Ѵ�
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(action.AnimName))
            Debug.LogError("���� �ִϸ��̼��� �̸��� �ùٸ��� �ʽ��ϴ�.");

        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;

            // player�� rotate�� ��ȯ���ش�.
            if (action.RotateToObstacle)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.TargetRotation, playerController.RotSpeed * Time.deltaTime);

            if (action.EnableTargetMatching)
                MatchTarget(action);

            if (animator.IsInTransition(0) && timer > 0.5f)
                break;

            yield return null;
        }

        yield return new WaitForSeconds(action.PostActionDelay);

        playerController.SetControl(true);
        inAction = false;
    }

    void MatchTarget(ParkourAction action)    // Ÿ�� ��ġ ���
    {
        if (animator.isMatchingTarget) return;

        animator.MatchTarget(action.MatchPos, transform.rotation, action.MatchBodyPart, new MatchTargetWeightMask(action.MatchPosWeight, 0),
            action.MatchStartTime, action.MatchTargetTime);
    }
}
