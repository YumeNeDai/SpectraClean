using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerAbstractState
{
    public PlayerAttackState(PlayerStateMachine playerState, StateFactory playerStateFactory)
       : base(playerState, playerStateFactory)
    {

    }

    public override void EnterState() { }

    public override void UpdateState() { }

    public override void ExitState() { }

    public override void CheckSwitchState() { }

    public override void InitializeSubState() { }

}
