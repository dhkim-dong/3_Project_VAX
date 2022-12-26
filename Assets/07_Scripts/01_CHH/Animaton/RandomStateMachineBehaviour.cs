using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomStateMachineBehaviour : StateMachineBehaviour
{
    #region Variable

    public int numberOfStates;              // Ȯ��� �ִϸ��̼� ����(0-2)
    public float minNormalTime = 0f;        // �ּ��� ��ŭ �������
    public float maxNormalTime = 5f;        // �ִ� �󸶳� �÷��� ����

    public float randomNormalTime;          // ����� ���� �ʿ�

    [SerializeField] private string hashName;
    //private int hashRandom;

    #endregion Variable

    #region Unity Method

    // ������Ʈ �ʱ�ȭ
    private void Start()
    {
        
    }

    // �⺻�� ���¿� ������ �� ����
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // transition(����)�� �ʿ��� �ð��� ���
        randomNormalTime = Random.Range(minNormalTime, maxNormalTime);
    }

    // �ִϸ��̼� ���°� ������Ʈ �ɶ� ����
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // transition(����) ���¿��� ����� ���� ���� �Ķ���Ͱ� -1�� �缳��
        if (animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == stateInfo.fullPathHash)
        {
            animator.SetInteger(hashName, -1);
        }

        // ���°� �̸� ������ �ð��� �ʰ��ϰ� ���� ��ȯ���� ���� ��� ���º���
        if (stateInfo.normalizedTime > randomNormalTime && !animator.IsInTransition(0))
        {
            animator.SetInteger(hashName, Random.Range(0, numberOfStates));
        }
    }

    #endregion Unity Method
}
