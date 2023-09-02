using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallingState : PlayerAbstractState, IRootState
{
    public PlayerFallingState(PlayerStateMachine playerState, StateFactory playerStateFactory)
       : base(playerState, playerStateFactory)
    {
        _isRootState = true;
        InitializeSubState();
    }

    public override void EnterState() 
    {
        _playerStateMachine.Gravity = -25f;
    }

    public override void UpdateState() 
    {
        HandleGravity();
        CheckSwitchState();
    }

    public override void ExitState()
    {
        _playerStateMachine.Gravity = -15f;
    }

    public override void CheckSwitchState()
    {
        if (_playerStateMachine.Grounded)
        {
            SwitchState(_stateFactory.Grounded());
        }
    }

    public override void InitializeSubState()
    {
        SetSubState(_stateFactory.Walk());
    }

    public void HandleGravity()
    {
        if (_playerStateMachine.VerticalVelocity < _playerStateMachine.TerminalVelocity)
        {
            _playerStateMachine.VerticalVelocity += _playerStateMachine.Gravity * Time.deltaTime;
        }
    }

}
