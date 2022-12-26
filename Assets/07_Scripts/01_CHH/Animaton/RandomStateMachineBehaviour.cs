using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomStateMachineBehaviour : StateMachineBehaviour
{
    #region Variable

    public int numberOfStates;              // 확장된 애니메이션 개수(0-2)
    public float minNormalTime = 0f;        // 최소한 얼만큼 실행될지
    public float maxNormalTime = 5f;        // 최대 얼마나 플레이 할지

    public float randomNormalTime;          // 계산을 위해 필요

    [SerializeField] private string hashName;
    //private int hashRandom;

    #endregion Variable

    #region Unity Method

    // 컴포넌트 초기화
    private void Start()
    {
        
    }

    // 기본적 상태에 들어왔을 때 실행
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // transition(이전)에 필요한 시간을 계산
        randomNormalTime = Random.Range(minNormalTime, maxNormalTime);
    }

    // 애니메이션 상태가 업데이트 될때 실행
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // transition(이전) 상태에서 벗어나면 랜덤 유휴 파라미터가 -1로 재설정
        if (animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == stateInfo.fullPathHash)
        {
            animator.SetInteger(hashName, -1);
        }

        // 상태가 미리 결정된 시간을 초과하고 아직 전환되지 않은 경우 상태변경
        if (stateInfo.normalizedTime > randomNormalTime && !animator.IsInTransition(0))
        {
            animator.SetInteger(hashName, Random.Range(0, numberOfStates));
        }
    }

    #endregion Unity Method
}
