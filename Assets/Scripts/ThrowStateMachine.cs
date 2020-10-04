using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowStateMachine : StateMachineBehaviour
{
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        animator.GetComponent<CharacterAnimationEventReceiver>().ThrowCompleteAnimEvent();
    }
}
